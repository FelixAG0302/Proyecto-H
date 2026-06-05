using TaskTracker.Api.Repositories;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Models;
using TaskTracker.Api.Data;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public class StreakRepository : IStreakRepository
{
    private readonly AppDbContext _dbContext;
    
    public StreakRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Streak?> GetByUserAsync(int userId)
    {
        return await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task UpdateAsync(Streak streak)
    {
        var existing = await _dbContext.Streaks.FindAsync(streak.Id);
        if (existing == null) return;
        
        existing.CurrentStreak = streak.CurrentStreak;
        existing.LongestStreak = streak.LongestStreak;
        existing.LastActivityDate = streak.LastActivityDate;
        await _dbContext.SaveChangesAsync();
    }

    public async Task ResetAsync(int userId)
    {
        var streak = await _dbContext.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        if (streak == null) return;

        streak.CurrentStreak = 0;
        streak.LastActivityDate = DateOnly.FromDateTime(DateTime.Now);
        
        await _dbContext.SaveChangesAsync();
    }
}