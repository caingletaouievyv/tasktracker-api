// TaskTrackerApi/Services/Interfaces/IAdminService.cs

using TaskTrackerApi.DTOs;

public interface IAdminService
{
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    Task<bool> DeleteUserAsync(string userId);
    Task<bool> DeleteTaskAsync(int taskId);
    Task<bool> UpdateUserRolesAsync(string userId, List<string> roles);
    Task<string> GenerateUsersCsvAsync(IEnumerable<UserDto> users);
    Task<byte[]> GenerateUsersPdfAsync(IEnumerable<UserDto> users);
}

