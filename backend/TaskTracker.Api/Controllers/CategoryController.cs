using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using System.Linq;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoryController : ControllerBase
{
    private readonly ICategoryRepository _repository;

    public CategoryController(ICategoryRepository repository)
    {
        _repository = repository;
    }
    
    // Helper method
    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    
    // GET api/category
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _repository.GetAllByUserAsync(GetUserId());
        return Ok(categories.Select(c => new CategoryDto(c.Id, c.Name, c.Color)));
    }

    // GET api/category
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        
        var category = await _repository.GetByIdAsync(id, userId);
        if (category == null) return NotFound(new {message = "Category not found."});
        
        return Ok(new CategoryDto(category.Id, category.Name, category.Color));
    }
    
    // POST api/category
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {

        var category = new Category
        {
            Name = dto.Name,
            Color = dto.Color ?? "#4A90E2",
            UserId = GetUserId()
        };
        
        var created = await _repository.CreateAsync(category);
        if (created == null) return StatusCode(500, new {message = "Category not found."});
        
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new CategoryDto(created.Id, category.Name, category.Color));
    }
    
    // PUT api/category/
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateCategoryDto dto)
    {
        var updated = new Category
        {
            Name = dto.Name,
            Color = dto.Color ?? "#4A90E2"
        };

        var result = await _repository.UpdateAsync(id, GetUserId(), updated);
        if (result == null) return NotFound(new {message = "Category not found."});
        
        return Ok(new CategoryDto(result.Id, result.Name, result.Color));
    }
    
    // DELETE api/category/
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _repository.DeleteAsync(id, GetUserId());
        
        if (!deleted) return NotFound(new {message = "Category not found."});
        
        return Ok(new {message = "Category deleted."});
    }
}

public record CategoryDto(int Id, string Name, string? Color);
public record CreateCategoryDto(string Name, string? Color);