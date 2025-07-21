// TaskTrackerApi/Services/TaskService.cs

using TaskTrackerApi.Data;
using TaskTrackerApi.Models;
using Microsoft.EntityFrameworkCore;

public class TaskService : ITaskService
{
    private readonly TaskDbContext _context;

    public TaskService(TaskDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync(string? search, string? status, string? sort, string? userId)
    {
        var query = _context.TaskItems.AsQueryable();

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(t => t.UserId == userId);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t => (t.Title != null && t.Title.Contains(search)) ||
                                     (t.Description != null && t.Description.Contains(search))
);

        if (!string.IsNullOrEmpty(status))
        {
            query = status.ToLower() switch
            {
                "completed" => query.Where(t => t.IsCompleted),
                "active" => query.Where(t => !t.IsCompleted && t.DueDate >= DateTime.Now),
                "overdue" => query.Where(t => !t.IsCompleted && t.DueDate < DateTime.Now),
                _ => query
            };
        }

        query = sort?.ToLower() switch
        {
            "title" => query.OrderBy(t => t.Title),
            "duedate" => query.OrderBy(t => t.DueDate),
            _ => query.OrderBy(t => t.Id)
        };

        return await query
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted
            })
            .ToListAsync();
    }

    public async Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        return task == null ? null : new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted
        };
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskDto taskDto, string userId)
    {
        var task = new TaskItem
        {
            Title = taskDto.Title,
            Description = taskDto.Description,
            DueDate = taskDto.DueDate,
            IsCompleted = false,
            UserId = userId
        };
        _context.TaskItems.Add(task);
        await _context.SaveChangesAsync();

        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            IsCompleted = task.IsCompleted
        };
    }

    public async Task<bool> UpdateTaskAsync(int id, UpdateTaskDto taskDto)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null) return false;

        task.Title = taskDto.Title;

        task.Description = taskDto.Description;
        task.DueDate = taskDto.DueDate;
        task.IsCompleted = taskDto.IsCompleted;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        var task = await _context.TaskItems.FindAsync(id);
        if (task == null) return false;

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TaskDto>> GetTasksByUserIdAsync(string userId)
    {
        return await _context.TaskItems
            .Where(t => t.UserId == userId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                UserId = t.UserId
            })
            .ToListAsync();
    }
}
