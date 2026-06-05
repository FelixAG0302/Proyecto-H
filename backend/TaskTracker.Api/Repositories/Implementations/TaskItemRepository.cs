using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Models;
using TaskTracker.Api.Data;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public class TaskItemRepository : ITaskItemRepository
{
    private readonly AppDbContext _context;

    public TaskItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetDailyByUserAsync(int userId)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId && t.Type == TaskType.Daily)
            .Include(t => t.Category)
            .ToListAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetScheduledByUserAsync(int userId, DateOnly from, DateOnly to)
    {
        return await _context.Tasks
            .Where(t => t.UserId == userId && t.Type == TaskType.Scheduled)
            .Include(t => t.Category)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(int id, int userId)
    {
        return await _context.Tasks
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.UserId == userId && t.Id == id);
    }

    public async Task<TaskItem?> CreateAsync(TaskItem taskItem)
    {
        _context.Add(taskItem);
        await _context.SaveChangesAsync();
        return taskItem;
    }

    public async Task<TaskItem?> UpdateAsync(int id, int userId, TaskItem updated)
    {
        var task = await GetByIdAsync(id, userId);

        if (task == null) return null;

        task.Title = updated.Title;
        task.Type = updated.Type;
        task.DueDate = updated.DueDate;
        task.StartTime = updated.StartTime;
        task.EndTime = updated.EndTime;
        task.Priority = updated.Priority;
        task.CategoryId = updated.CategoryId;
        
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var task = await GetByIdAsync(id, userId);
        if (task == null) return false;
        
        _context.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<TaskItem?> CompleteAsync(int id, int userId)
    {
        var task = await GetByIdAsync(id, userId);
        if (task == null || task.IsCompleted ) return null;

        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return task;
    }
}