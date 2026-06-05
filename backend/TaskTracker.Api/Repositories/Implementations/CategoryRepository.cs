using Microsoft.EntityFrameworkCore;
using TaskTracker.Api.Data;
using TaskTracker.Api.Models;
using TaskTracker.Api.Repositories.Interfaces;

namespace TaskTracker.Api.Repositories.Implementations;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Category>> GetAllByUserAsync(int userId)
    {
        return await _context.Categories.Where(c => c.UserId == userId).ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id, int userId)
    {
        return await _context.Categories.FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);
    }

    public async Task<Category?> CreateAsync(Category category)
    {
        _context.Categories.Add(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category?> UpdateAsync(int id, int userId, Category updated)
    {
        var category = await GetByIdAsync(id, userId);

        if (category == null) return null;

        category.Name = updated.Name;
        category.Color = updated.Color;
        
        await _context.SaveChangesAsync();
        
        return category;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var category = await GetByIdAsync(id, userId);
        
        if (category == null) return false;

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> ExistsAsync(int id, int userId)
    {
        return await _context.Categories.AnyAsync(c => c.Id == id && c.UserId == userId);
    }
}