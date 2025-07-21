// TaskTrackerApi/Models/ApplicationUser.cs

using Microsoft.AspNetCore.Identity;

namespace TaskTrackerApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
