// TaskTrackerApi/Services/Implementations/AdminService.cs

using TaskTrackerApi.Data;
using TaskTrackerApi.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TaskTrackerApi.Models;
using System.Text;
using TaskTrackerApi.Services.Implementations;

public class AdminService : IAdminService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TaskDbContext _context;
    private readonly IMapper _mapper;

    public AdminService(UserManager<ApplicationUser> userManager, TaskDbContext context, IMapper mapper)
    {
        _userManager = userManager;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        var userDtos = new List<UserDto>();
        foreach (var user in users)
        {
            var roles = (await _userManager.GetRolesAsync(user)).ToList();
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles;
            userDtos.Add(userDto);
        }

        return userDtos;
    }

    public async Task<UserWithTasksDto?> GetUserWithTasksAsync(string userId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        var roles = (await _userManager.GetRolesAsync(user)).ToList();
        var userDto = _mapper.Map<UserDto>(user);
        userDto.Roles = roles;

        var tasks = await _context.TaskItems
            .Where(t => t.UserId == userId)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted
            })
            .ToListAsync();

        return new UserWithTasksDto
        {
            User = userDto,
            Tasks = tasks
        };
    }

    public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        return await _context.TaskItems
            .Include(t => t.User)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                DueDate = t.DueDate,
                IsCompleted = t.IsCompleted,
                UserId = t.UserId,
                OwnerEmail = t.User!.Email
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateUserRolesAsync(string userId, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var currentRoles = await _userManager.GetRolesAsync(user);

        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded) return false;

        var addResult = await _userManager.AddToRolesAsync(user, roles);
        return addResult.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        var user = await _context.Users
            .Include(u => u.Tasks)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return false;

        _context.TaskItems.RemoveRange(user.Tasks);
        _context.RefreshTokens.RemoveRange(user.RefreshTokens);
        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(int taskId)
    {
        var task = await _context.TaskItems.FindAsync(taskId);
        if (task == null) return false;

        _context.TaskItems.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<string> GenerateUsersCsvAsync(IEnumerable<UserDto> users)
    {
        var sb = new StringBuilder();
        sb.AppendLine("UserName,Email,Roles");

        foreach (var user in users)
        {
            var roles = string.Join(";", user.Roles ?? []);
            sb.AppendLine($"{EscapeCsv(user.UserName)},{EscapeCsv(user.Email)},{EscapeCsv(roles)}");
        }

        return Task.FromResult(sb.ToString());

        static string EscapeCsv(string? input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            var escaped = input.Replace("\"", "\"\"");
            return escaped.Contains(',') ? $"\"{escaped}\"" : escaped;
        }
    }

    public Task<byte[]> GenerateUsersPdfAsync(IEnumerable<UserDto> users)
    {
        var exporter = new UserListPdfExporter();
        var result = exporter.GenerateUsersPdf(users);
        return Task.FromResult(result);
    }
}
