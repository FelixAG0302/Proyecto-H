using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using System.Linq;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoryController(AppDbContext context)
    {
        _context = context;
    }
    
    // Helper method
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
    // GET api/category
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var category = await _context.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var result = category.Select(c => new CategoryDto(
            c.UserId,
            c.Name,
            c.Color
            ));

        return Ok(category);
    }

    // GET api/category
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        
        var category = await _context.Categories
            .Where(c => c.Id == id && c.UserId == userId)
            .Select( c => new CategoryDto(c.UserId, c.Name, c.Color))
            .FirstOrDefaultAsync();
        
        if (category == null) return NotFound(new {message = "Category not found."});

        return Ok(category);
    }
    
    // POST api/category
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var userId = GetUserId();

        var category = new Category
        {
            Name = dto.Name,
            Color = dto.Color,
            UserId = userId
        };
        
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = category.Id },
            new CategoryDto(category.Id, category.Name, category.Color)
            );
    }
    
    // PUT api/category/
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var userId = GetUserId();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        
        if (category == null) return NotFound( new {message = "Category not found."});
        
        category.Name = dto.Name;
        category.Color = dto.Color ?? category.Color;

        await _context.SaveChangesAsync();
        
        return Ok(new CategoryDto(category.Id, category.Name, category.Color));
    }
    
    // DELETE api/category/
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        
        if (category == null) return NotFound(new {message = "Category not found."});

        _context.Categories.Remove(category);
        _context.SaveChanges();

        return Ok(category);
    }
    
}

public record CategoryDto(int UserId, string Name, string? Color);
public record CreateCategoryDto(string Name, string? Color);