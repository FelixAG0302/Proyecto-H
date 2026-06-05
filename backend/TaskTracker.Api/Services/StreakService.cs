using TaskTracker.Api.Models;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Services;

public class StreakService
{
    private readonly IStreakRepository _repository;

    public StreakService(IStreakRepository repository)
    {
        _repository = repository;
    }

    public async Task UpdateStreakAsync(int userId)
    {
        var streak = await _repository.GetByUserAsync(userId);
        if (streak == null) return;
        
        var today = DateOnly.FromDateTime(DateTime.Now);
        streak.CurrentStreak = streak.LastActivityDate == today.AddDays(-1) ? streak.CurrentStreak + 1 : 1;
        
        if (streak.CurrentStreak > streak.LongestStreak) streak.LongestStreak = streak.CurrentStreak;
        
        streak.LastActivityDate = today;
        
        await _repository.UpdateAsync(streak);
    }
}