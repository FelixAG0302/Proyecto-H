using TaskTracker.Api.Models;

namespace TaskTracker.Api.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<IEnumerable<Category>> GetAllByUserAsync(int userId);
    Task<Category?> GetByIdAsync(int id, int userId);
    Task<Category?> CreateAsync(Category category);
    Task<Category?> UpdateAsync(int id, int userId, Category category);
    Task<bool> DeleteAsync(int id, int userId);
    Task<bool> ExistsAsync(int id, int userId);
}