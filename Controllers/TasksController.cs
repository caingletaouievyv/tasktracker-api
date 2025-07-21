// TaskTrackerApi/Controllers/TasksController.cs

using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using TaskTrackerApi.Services.Implementations;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IAdminService _adminService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, IAdminService adminService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _adminService = adminService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(string? search = null, string? status = null, string? sort = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("Unable to extract user ID from token.");
            return Unauthorized();
        }

        _logger.LogInformation("Fetching all tasks with filters: search={Search}, status={Status}, sort={Sort}", search, status, sort);
        var tasks = await _taskService.GetAllTasksAsync(search, status, sort, userId);
        return Ok(tasks);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        if (task == null)
        {
            _logger.LogWarning("Task with ID {TaskId} not found", id);
            return NotFound();
        }
        return Ok(task);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTaskDto taskDto)
    {

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            _logger.LogWarning("Unable to extract user ID from token.");
            return Unauthorized();
        }

        var createdTask = await _taskService.CreateTaskAsync(taskDto, userId);

        _logger.LogInformation("Created new task with ID {TaskId}", createdTask.Id);
        return CreatedAtAction(nameof(GetById), new { id = createdTask.Id }, createdTask);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTaskDto taskDto)
    {
        var success = await _taskService.UpdateTaskAsync(id, taskDto);
        if (!success)
        {
            _logger.LogWarning("Failed to update task with ID {TaskId}", id);
            return NotFound();
        }

        _logger.LogInformation("Updated task with ID {TaskId}", id);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _taskService.DeleteTaskAsync(id);
        if (!success)
        {
            _logger.LogWarning("Failed to delete task with ID {TaskId}", id);
            return NotFound();
        }

        _logger.LogInformation("Deleted task with ID {TaskId}", id);
        return NoContent();
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportTasksToCsv([FromQuery] bool all = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        bool isAdmin = IsAdmin();

        if (all && !isAdmin)
            return Forbid();

        IEnumerable<TaskDto> tasks = isAdmin
            ? await _adminService.GetAllTasksAsync()
            : await _taskService.GetTasksByUserIdAsync(userId);

        var csvBuilder = new StringBuilder();

        if (isAdmin)
            csvBuilder.AppendLine("Title,Description,DueDate,IsCompleted,OwnerEmail");
        else
            csvBuilder.AppendLine("Title,Description,DueDate,IsCompleted");

        foreach (var task in tasks)
        {
            if (isAdmin)
            {
                csvBuilder.AppendLine(
                    $"\"{task.Title}\",\"{task.Description}\",{task.DueDate:yyyy-MM-dd},{task.IsCompleted},\"{task.OwnerEmail}\""
                );
            }
            else
            {
                csvBuilder.AppendLine(
                    $"\"{task.Title}\",\"{task.Description}\",{task.DueDate:yyyy-MM-dd},{task.IsCompleted}"
                );
            }
        }

        var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(csvBytes, "text/csv", "tasks.csv");
    }

    [HttpGet("export/pdf")]
    public async Task<IActionResult> ExportTasksToPdf(
    [FromServices] TaskPdfExporter pdfExporter,
    [FromQuery] bool all = false)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null) return Unauthorized();

        bool isAdmin = IsAdmin();

        if (all && !isAdmin)
            return Forbid();

        IEnumerable<TaskDto> tasks = isAdmin
            ? await _adminService.GetAllTasksAsync()
            : await _taskService.GetTasksByUserIdAsync(userId);

        var pdfBytes = pdfExporter.GenerateTasksPdf(tasks, isAdmin);
        return File(pdfBytes, "application/pdf", "tasks.pdf");
    }

    private bool IsAdmin() =>
    User.Claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
}
