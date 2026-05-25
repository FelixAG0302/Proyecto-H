using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreakController : Controller
{
    private readonly AppDbContext _context;

    public StreakController(AppDbContext context)
    {
        _context = context;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();
        
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);

        if (streak == null) return NotFound(new {message = "The User Streak Was Not Found"});
        
        return Ok(new StreakDto(streak.CurrentStreak, streak.LongestStreak, streak.LastActivityDate));
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        var userId = GetUserId();
        
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId);
        
        if (streak == null) return NotFound(new {message = "The User Streak Was Not Found"});
        
        streak.CurrentStreak = 0;
        streak.LastActivityDate = DateOnly.FromDateTime(DateTime.UtcNow);
        
        await _context.SaveChangesAsync();

        return Ok(new {message = "The User Streak Was Successfully Reset"});
    }
}

public record StreakDto(
    int CurrentStreak,
    int LongestStreak,
    DateOnly LastActivityDate);