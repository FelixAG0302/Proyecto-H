using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;


namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/Controller")]

public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IConfiguration _configuration;
    
    public AuthController(AppDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }
    
    // Post apu/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        // First we verify if the user exits
        if (await _dbContext.Users.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = "Email already exists" });
        
        // Then we create the user with the hassed pass
        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        
        // Initial streak for the user
        var streak = new Streak { UserId = user.Id };
        _dbContext.Streaks.Add(streak);
        await _dbContext.SaveChangesAsync();
        
        return Ok(new {message = "User successfully registered"});
    }
    
    // Post api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        
        //search the user by its email
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new {message = "Invalid credentials"});

        var token = GenerateJwtToken(user);
        
        return Ok(new
        {
            token,
            user = new
            {
                UserId = user.Id,
                user.Name,
                user.Email,
            }
        });
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new []
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(double.Parse(_configuration["Jwt:ExpireInHours"]!)),
            signingCredentials: credentials
            );

        return new JwtSecurityTokenHandler().WriteToken(token);
    } 
}

public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);