// TaskTrackerApi/Services/ITaskService.cs

public interface ITaskService
{
    Task<IEnumerable<TaskDto>> GetAllTasksAsync(string? search, string? status, string? sort, string? userId);
    Task<TaskDto?> GetTaskByIdAsync(int id);
    Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, string userId);
    Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto);
    Task<bool> DeleteTaskAsync(int id);
    Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId);
}