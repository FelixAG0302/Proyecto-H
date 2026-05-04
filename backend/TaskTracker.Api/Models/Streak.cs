namespace TaskTracker.Api.Models;

public class Streak
{
    public int Id { get; set; }
    public int CurrentStreak { get; set; } = 0;
    public int LongestStreak { get; set; } = 0;
    public DateOnly LastActivityDate { get; set; } = new DateOnly();
    
    // Foreign Key
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}