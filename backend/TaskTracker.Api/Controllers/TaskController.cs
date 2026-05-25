using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly AppDbContext _context;

    public TaskController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily()
    {
        var userId = GetUserId();

        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId && t.Type == TaskType.Daily)
            .Include(t => t.Category)
            .Select(t => new TaskItemDto(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.CompletedAt,
                t.Type.ToString(),
                t.DueDate,
                t.StartTime,
                t.EndTime,
                t.Priority.ToString(),
                t.CategoryId,
                t.Category.Name,
                t.Category.Color
            )).ToListAsync();
        
        return Ok(tasks);
    }

    [HttpGet("Scheduled")]
    public async Task<IActionResult> GetScheduled(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var userId = GetUserId();
        
        var tasks = await _context.Tasks
            .Where(t => t.UserId == userId 
                            && t.Type == TaskType.Scheduled
                            && t.DueDate.HasValue
                            && DateOnly.FromDateTime(t.DueDate.Value) >= from 
                            && DateOnly.FromDateTime(t.DueDate.Value) <= to
                            ).Include(t => t.Category)
            .Select(t => new TaskItemDto(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.CompletedAt,
                t.Type.ToString(),
                t.DueDate,
                t.StartTime,
                t.EndTime,
                t.Priority.ToString(),
                t.CategoryId,
                t.Category.Name,
                t.Category.Color
            ))
            .ToListAsync();

        return Ok(tasks);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskItemDto dto)
    {
        var userId = GetUserId();
        
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.UserId == userId);

        if (!categoryExists) return BadRequest(new { message = "Category Not Valid" });

        if (dto.Type == "Scheduled" && dto.DueDate == null)
            return BadRequest(new { message = "A Scheduled Task Must Have a Due Date" });
        
        var task = new TaskItem
        {
            Title = dto.Title,
            Type = Enum.Parse<TaskType>(dto.Type),
            DueDate = dto.DueDate.HasValue ? DateTime.SpecifyKind(dto.DueDate.Value, DateTimeKind.Utc) : null,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Priority = Enum.Parse<TaskPriority>(dto.Priority ?? "Normal"),
            CategoryId = dto.CategoryId,
            UserId = userId
        };
        
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new {id = task.Id}, new {task.Id, task.Title, task.Type});
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var task = await _context.Tasks.Where(t => t.Id == id && t.UserId == userId)
            .Include(t => t.Category)
            .Select(t => new TaskItemDto(
                t.Id,
                t.Title,
                t.IsCompleted,
                t.CompletedAt,
                t.Type.ToString(),
                t.DueDate,
                t.StartTime,
                t.EndTime,
                t.Priority.ToString(),
                t.CategoryId,
                t.Category.Name,
                t.Category.Color))
            .FirstOrDefaultAsync();

        if (task == null) return NotFound(new { message = "Task Not Found" });

        return Ok(task);
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<TaskItem>> Complete(int id)
    {

        var userId = GetUserId();
        
        var task = await _context.Tasks.FirstOrDefaultAsync((t => t.Id == id && t.UserId == userId ));

        if (task == null) return NotFound(new { message = "The Task Was Not Found" });

        if (task.IsCompleted) return BadRequest(new { message = "The task is already complete" });

        task.IsCompleted = true;
        task.CompletedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();

        await UpdateStreak(userId);

        return Ok(new {task.Id, task.IsCompleted, task.CompletedAt});
    }

    [HttpDelete("{id}/complete")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        
        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id &&  t.UserId == userId);
        
        if (task == null) return NotFound(new {Message ="Task Not Found"});
        
        _context.Remove(task);
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // Update the user streak - is executed when aa task is completed for the first time in the day
    private async Task UpdateStreak(int userId)
    {
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null) return;
        
        var today = DateOnly.FromDateTime(DateTime.Now);

        if (streak.LastActivityDate == today) return;

        if (streak.LastActivityDate == today.AddDays(-1))
        {
            streak.CurrentStreak++;
        }
        else
        {
            streak.CurrentStreak = 1;
        }
        
        if (streak.CurrentStreak > streak.LongestStreak) streak.LongestStreak = streak.CurrentStreak;
        
        streak.LastActivityDate = today;

        await _context.SaveChangesAsync();
    }
}

public record TaskItemDto(
    int Id,
    string Title,
    bool IsCompleted,
    DateTime? CompletedAt,
    string Type,
    DateTime? DueDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string Priority,
    int CategoryId,
    string CategoryName,
    string CategoryColor
    );

public record CreateTaskItemDto(
    string Title,
    string Type,
    DateTime? DueDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Priority,
    int CategoryId);