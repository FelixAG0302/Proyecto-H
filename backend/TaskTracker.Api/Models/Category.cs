namespace TaskTracker.Api.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; } = "#4A90E2";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Foreign Key
    public int UserId { get; set; }
    public User User { get; set; }
    
    // Navegation properties
    public ICollection<TaskItem> TaskItems { get; set; } = new List<TaskItem>();
}