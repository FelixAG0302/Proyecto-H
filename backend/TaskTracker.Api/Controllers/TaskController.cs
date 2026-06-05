using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using TaskTracker.Api.Repositories;
using TaskTracker.Api.Repositories.Interfaces;
using TaskTracker.Api.Services;

namespace TaskTracker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskItemRepository _taskRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly StreakService _streakService;

    public TaskController(ITaskItemRepository taskRepository, ICategoryRepository categoryRepository, StreakService streakService)
    {
        _taskRepository = taskRepository;
        _categoryRepository = categoryRepository;
        _streakService = streakService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("daily")]
    public async Task<IActionResult> GetDaily(int userId)
    {
        var tasks = await _taskRepository.GetDailyByUserAsync(GetUserId());
        
        return Ok(tasks.Select(ToDto));
    }

    [HttpGet("Scheduled")]
    public async Task<IActionResult> GetScheduled(
        [FromQuery] DateOnly from,
        [FromQuery] DateOnly to)
    {
        var tasks = await _taskRepository.GetScheduledByUserAsync(GetUserId(), from, to);

        return Ok(tasks.Select(ToDto));
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create([FromBody] CreateTaskItemDto dto)
    {
        var userId = GetUserId();
        
        var categoryExists = await _categoryRepository.ExistsAsync(dto.CategoryId, userId);
        if (!categoryExists) return NotFound(new {message = "Category Not Found"});

        if (dto.Type == "Scheduled" && dto.DueDate == null)
            return BadRequest(new { message = "A scheduled task needs a due date" });

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

        var created = await _taskRepository.CreateAsync(task);
        return CreatedAtAction(nameof(GetById),
            new {id = created.Id }, new {created.Id, created.Title, created.Type});
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult> GetById(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id, GetUserId());
        if (task == null) return NotFound(new {message = "Task Not Found"});
        
        return Ok(ToDto(task));
    }

    [HttpPatch("{id}/complete")]
    public async Task<ActionResult<TaskItem>> Complete(int id)
    {
        var userId = GetUserId();
        var task = await _taskRepository.CompleteAsync(id, userId);
        
        if (task == null) return NotFound(new {message = "Task Not Found"});

        await _streakService.UpdateStreakAsync(userId);

        return Ok(new {task.Id, task.IsCompleted, task.CompletedAt});
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _taskRepository.DeleteAsync(id, GetUserId());
        if (!deleted) return NotFound(new {message = "Task Not Found"});
        
        return NoContent();
    }

    //Helper method for mapp TaskItem to TaskItemDito
    private static TaskItemDto ToDto(TaskItem t) => new(
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
        t.Category?.Name ?? "Without Category",
        t.Category?.Color ?? "#4A90E2"
    );
}

// Dtos
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