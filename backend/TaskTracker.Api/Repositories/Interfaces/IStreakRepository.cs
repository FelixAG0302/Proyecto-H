using TaskTracker.Api.Models;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface IStreakRepository
{
    Task<Streak?> GetByUserAsync(int userId);
    Task UpdateAsync(Streak streak);
    Task ResetAsync(int userId);
}