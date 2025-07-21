// TaskTrackerApi/DTO/TaskDto.cs

public class UpdateTaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
}
