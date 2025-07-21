// TaskTrackerApi/Services/Interfaces/ITokenService.cs

using TaskTrackerApi.Models;

namespace TaskTrackerApi.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(ApplicationUser user);
        string GenerateRefreshToken();
    }
}
