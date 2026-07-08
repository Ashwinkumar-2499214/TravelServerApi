using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TravelEaseServer.Controllers;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Tests
{
    [TestFixture]
    public class AuthenticationControllerTests
    {
        private Mock<IAuthenticationService> _mockAuthService;
        private Mock<INotificationService> _mockNotificationService;
        private AuthenticationController _controller;

        [SetUp]
        public void SetUp()
        {
            // Initialize the mock service before each test to ensure a clean state
            _mockAuthService = new Mock<IAuthenticationService>();
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new AuthenticationController(_mockAuthService.Object, _mockNotificationService.Object);
        }

        #region Login Tests

        [Test]
        public async Task Login_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Login(null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [TestCase("", "password123")]
        [TestCase("user@test.com", "")]
        [TestCase(" ", "password123")]
        public async Task Login_InvalidInputFields_ReturnsBadRequest(string email, string password)
        {
            // Arrange
            var loginDto = new LoginRequestDto { Email = email, Password = password };

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Login_ValidCredentials_ReturnsOkWithData()
        {
            // Arrange
            var loginDto = new LoginRequestDto { Email = "test@travelease.com", Password = "SecurePassword123" };
            var expectedResponse = new LoginResponseDto 
            { 
                UserId = 1, 
                Name = "John Doe", 
                Email = "test@travelease.com", 
                Role = "User", 
                Token = "mocked-jwt-token" 
            };

            _mockAuthService
                .Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginRequestDto { Email = "wrong@travelease.com", Password = "WrongPassword" };
            
            _mockAuthService
                .Setup(s => s.LoginAsync(loginDto))
                .ReturnsAsync((LoginResponseDto?)null); // Simulating auth failure

            // Act
            var result = await _controller.Login(loginDto);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        #endregion

        #region Logout Tests

        [Test]
        public async Task Logout_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Logout(null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task Logout_ValidSession_ReturnsOk()
        {
            // Arrange
            var logoutDto = new LogoutRequestDto { UserId = 1 };
            _mockAuthService
                .Setup(s => s.LogoutAsync(logoutDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Logout(logoutDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task Logout_InvalidSessionOrFailure_ReturnsUnauthorized()
        {
            // Arrange
            var logoutDto = new LogoutRequestDto { UserId = 999 };
            _mockAuthService
                .Setup(s => s.LogoutAsync(logoutDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.Logout(logoutDto);

            // Assert
            Assert.IsInstanceOf<UnauthorizedObjectResult>(result);
        }

        #endregion

        #region Reset Password Tests

        [Test]
        public async Task ResetPassword_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.ResetPassword(null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task ResetPassword_Success_ReturnsOk()
        {
            // Arrange
            var resetDto = new PasswordResetDto 
            { 
                Email = "user@test.com", 
                OldPassword = "OldPassword123", 
                NewPassword = "NewPassword123" 
            };
            
            _mockAuthService
                .Setup(s => s.ResetPasswordAsync(resetDto))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.ResetPassword(resetDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task ResetPassword_Failure_ReturnsBadRequest()
        {
            // Arrange
            var resetDto = new PasswordResetDto 
            { 
                Email = "user@test.com", 
                OldPassword = "WrongOldPassword", 
                NewPassword = "NewPassword123" 
            };
            
            _mockAuthService
                .Setup(s => s.ResetPasswordAsync(resetDto))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.ResetPassword(resetDto);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion
    }
}