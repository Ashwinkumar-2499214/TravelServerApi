namespace TravelEaseServer.Dto
{
    public class UserRequestDto
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public string? Password { get; set; }
        public int Role { get; set; }
    }

    public class UserResponseDto
    {
        public long UserId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Phone { get; set; }
        public required string Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class UserSearchDto
    {
        public required string SearchTerm { get; set; }
        public int? Role { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class UserRoleAssignmentDto
    {
        public long UserId { get; set; }
        public int NewRole { get; set; }
    }

    public class PasswordResetDto
    {
        public required string Email { get; set; }
        public required string OldPassword { get; set; }
        public required string NewPassword { get; set; }
    }
}
