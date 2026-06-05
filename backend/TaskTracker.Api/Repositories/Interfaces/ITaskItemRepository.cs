using TaskTracker.Api.Models;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface ITaskItemRepository
{
    Task<IEnumerable<TaskItem>> GetDailyByUserAsync(int userId);
    Task<IEnumerable<TaskItem>> GetScheduledByUserAsync(int userId, DateOnly from, DateOnly to);
    Task<TaskItem?> GetByIdAsync(int id, int userId);
    Task<TaskItem?> CreateAsync(TaskItem taskItem);
    Task<TaskItem?> UpdateAsync(int id, int userId, TaskItem taskItem);
    Task<TaskItem?> CompleteAsync(int id, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}