// TaskTrackerApi/Models/TaskItem.cs

namespace TaskTrackerApi.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public string UserId { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;
    }
}
