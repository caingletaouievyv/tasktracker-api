// TaskTrackerApi/DTO/UserDto.cs

namespace TaskTrackerApi.DTOs
{
     public class UserDto
    {
        public string? Id { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class UserWithTasksDto
    {
        public UserDto? User { get; set; }
        public IEnumerable<TaskDto>? Tasks { get; set; }
    }

    public class RegisterDto
    {
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class LoginDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
