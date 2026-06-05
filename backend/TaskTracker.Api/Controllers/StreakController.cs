using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreakController : Controller
{
    private readonly IStreakRepository _repository;

    public StreakController(IStreakRepository repository)
    {
        _repository = repository;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Get()
    {
       var streak = await _repository.GetByUserAsync(GetUserId());
       if (streak == null) return NotFound(new {message = "The User Streak Was Not Found"});
        
       return Ok(new StreakDto(streak.CurrentStreak, streak.LongestStreak, streak.LastActivityDate));
    }

    [HttpPost("reset")]
    public async Task<IActionResult> Reset()
    {
        await _repository.ResetAsync(GetUserId());
        return Ok(new {message = "The User Streak Was Successfully Reset"});
    }
}

public record StreakDto(
    int CurrentStreak,
    int LongestStreak,
    DateOnly LastActivityDate);