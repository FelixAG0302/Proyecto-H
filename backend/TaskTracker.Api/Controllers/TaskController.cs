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

}