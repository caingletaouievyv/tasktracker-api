// TaskTrackerApi/Controllers/AdminController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("tasks")]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _adminService.GetAllTasksAsync();
        return Ok(tasks);
    }

    [HttpPost("updateUserRoles")]
    public async Task<IActionResult> UpdateUserRoles([FromBody] UpdateUserRolesRequest request)
    {
        var result = await _adminService.UpdateUserRolesAsync(request.UserId, request.Roles);
        if (!result) return BadRequest("Failed to update roles.");
        return Ok();
    }

    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var success = await _adminService.DeleteUserAsync(userId);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("tasks/{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var success = await _adminService.DeleteTaskAsync(taskId);
        return success ? NoContent() : NotFound();
    }

    [HttpGet("export/users/csv")]
    public async Task<IActionResult> ExportUsersCsv()
    {
        var users = await _adminService.GetAllUsersAsync();
        var csv = await _adminService.GenerateUsersCsvAsync(users);
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "users.csv");
    }

    [HttpGet("export/users/pdf")]
    public async Task<IActionResult> ExportUsersPdf()
    {
        var users = await _adminService.GetAllUsersAsync();
        var pdfBytes = await _adminService.GenerateUsersPdfAsync(users);
        return File(pdfBytes, "application/pdf", "users.pdf");
    }
}
