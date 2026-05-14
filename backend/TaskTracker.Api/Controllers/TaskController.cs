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

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
    {
        try
        {
            var tasks = await _context.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Error interno del servidor");
        }
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> CreateTask([FromBody] TaskItem task)
    {
        var newTask = new TaskItem
        {
            Title = task.Title,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
        };

        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();

        return Ok(newTask);
    }

    [HttpPut("{id}/complete")]
    public async Task<ActionResult<TaskItem>> CompleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        
        if (task == null)
        {
            return NotFound();
        }
        
        task.IsCompleted = true;
        
        await _context.SaveChangesAsync();
        
        return Ok();
    }

    [HttpDelete("{id}/complete")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
        
        if (task == null) return NotFound(new {Message ="Task Not Found"});
        
        _context.Remove(task);
        _context.SaveChanges();

        return Ok(new {message = "Task Deleted"});
    }
}

public record TaskItemDto(
    int Id,
    string Title,
    string? Description,
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
    string? Description,
    string Type,
    DateTime? DueDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string? Priority,
    int CategoryId);