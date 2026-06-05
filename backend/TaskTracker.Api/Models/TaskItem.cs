namespace TaskTracker.Api.Models;

public enum TaskType
{
    Daily,
    Scheduled
}

public enum TaskPriority
{
    Low,
    Normal,
    High
}

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; } = null;
    public TaskType Type { get; set; } = TaskType.Daily;
    public DateTime? DueDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Low;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign Keys
    public int UserId { get; set; }
    public User User { get; set; }
    
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}