using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum; // Added to reference UserRole enum
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Implementation;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;
using Microsoft.AspNetCore.Http;

namespace TravelEaseServer.Service.Implementation
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IConfiguration _configuration;
        private readonly IComplianceRepository _complianceRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IAuthenticationRepository authenticationRepository, IConfiguration configuration, IComplianceRepository complianceRepository, IHttpContextAccessor httpContextAccessor)
        {
            _authenticationRepository = authenticationRepository;
            _configuration = configuration;
            _complianceRepository = complianceRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                throw new ArgumentException("Email and password are required.");
            }

            // Check if multiple users exist with same email (duplicate entries)
            var emailCount = await _authenticationRepository.CountUsersByEmailAsync(loginDto.Email);
            if (emailCount > 1)
            {
                throw new ArgumentException(UserConstants.EmailAlreadyExists);
            }

            var user = await _authenticationRepository.AuthenticateUserAsync(loginDto.Email, loginDto.Password);

            if (user == null || !user.IsActive)
                return null;

            var token = GenerateJwtToken(user);

            var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
            clientIp = (string.IsNullOrEmpty(clientIp) || clientIp == "::1") ? "127.0.0.1" : clientIp;
            await _complianceRepository.LogAuditEventAsync(new AuditLog
            {
                UserId = user.UserId,
                Action = "Login",
                EntityType = "User",
                EntityId = user.UserId,
                OldValues = string.Empty,
                NewValues = $"Email: {user.Email}",
                Timestamp = DateTime.UtcNow,
                IpAddress = clientIp
            });

            return new LoginResponseDto
            {
                UserId = user.UserId,
                Name = user.Name,
                Email = user.Email,
                Role = ((UserRole)user.Role).ToString(),
                Token = token
            };
        }

        public async Task<bool> LogoutAsync(LogoutRequestDto logoutDto)
        {
            if (logoutDto == null)
                return false;

            var result = await _authenticationRepository.LogoutAsync(logoutDto.UserId);
            if (result)
            {
                var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                clientIp = (string.IsNullOrEmpty(clientIp) || clientIp == "::1") ? "127.0.0.1" : clientIp;
                await _complianceRepository.LogAuditEventAsync(new AuditLog
                {
                    UserId = logoutDto.UserId,
                    Action = "Logout",
                    EntityType = "User",
                    EntityId = logoutDto.UserId,
                    OldValues = string.Empty,
                    NewValues = string.Empty,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = clientIp
                });
            }
            return result;
        }

        public async Task<bool> ResetPasswordAsync(PasswordResetDto resetDto)
        {
            if (resetDto == null || string.IsNullOrWhiteSpace(resetDto.Email) || string.IsNullOrWhiteSpace(resetDto.OldPassword) || string.IsNullOrWhiteSpace(resetDto.NewPassword))
                return false;

            var user = await _authenticationRepository.AuthenticateUserAsync(resetDto.Email, resetDto.OldPassword);
            if (user == null)
                return false;

            var newHash = AuthenticationRepository.CreatePasswordHash(resetDto.NewPassword);
            return await _authenticationRepository.UpdatePasswordAsync(user.UserId, newHash);
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return false;

            var newHash = AuthenticationRepository.CreatePasswordHash(dto.NewPassword);
            return await _authenticationRepository.ForgotPasswordAsync(dto.Email, newHash);
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return Task.FromResult(false);

            var jwtSecret = _configuration["Auth:JwtSecret"];
            if (string.IsNullOrWhiteSpace(jwtSecret))
                return Task.FromResult(false);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var validations = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            };

            tokenHandler.ValidateToken(token, validations, out _);

            return Task.FromResult(true);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["Auth:JwtSecret"];

            if (string.IsNullOrWhiteSpace(jwtSecret))
                throw new InvalidOperationException("Missing configuration: Auth:JwtSecret");

            var jwtIssuer = _configuration["Auth:Issuer"];
            var jwtAudience = _configuration["Auth:Audience"];

            string userRoleString = ((UserRole)user.Role).ToString();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, userRoleString)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiresMinutes = _configuration.GetValue<int?>("Auth:TokenExpiryMinutes") ?? 60;

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}