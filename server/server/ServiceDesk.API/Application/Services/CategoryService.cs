using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceDesk.API.Application.Mapping;
using ServiceDesk.API.Domain;
using ServiceDesk.API.DTOs.Categories;
using ServiceDesk.API.Infrastructure.Data;

namespace ServiceDesk.API.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext db, ILogger<CategoryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    // 🔹 GetAllAsync с двумя параметрами
    public async Task<IReadOnlyList<CategoryResponse>> GetAllAsync(bool includeInactive, string role)
    {
        var query = _db.Categories.AsQueryable();
        
        // Студенты видят только активные категории
        if (role == "User" && !includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }
        
        return await query.Select(c => c.ToResponse()).ToListAsync();
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var exists = await _db.Categories.AnyAsync(c => c.Name == request.Name);
        if (exists)
        {
            throw new InvalidOperationException("Категория с таким именем уже существует.");
        }

        var category = new Category { Name = request.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Создана категория: {Name}", category.Name);
        return category.ToResponse();
    }

    public async Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id) 
            ?? throw new KeyNotFoundException("Категория не найдена.");

        var nameExists = await _db.Categories.AnyAsync(c => c.Name == request.Name && c.Id != id);
        if (nameExists)
        {
            throw new InvalidOperationException("Категория с таким именем уже существует.");
        }

        category.Name = request.Name;
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Категория {Id} успешно обновлена", id);
        return category.ToResponse();
    }

    // 🔹 SetActiveAsync принимает DTO, а не bool
    public async Task<CategoryResponse> SetActiveAsync(int id, SetActiveCategoryRequest request)
    {
        var category = await _db.Categories.FindAsync(id) 
            ?? throw new KeyNotFoundException("Категория не найдена.");
        
        category.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("Статус категории {Id} изменён на {IsActive}", id, category.IsActive);
        return category.ToResponse();
    }

   public async Task DeleteAsync(int id)
{
    var category = await _db.Categories.FindAsync(id) 
        ?? throw new KeyNotFoundException("Категория не найдена.");
    
    // 🔹 ИСПРАВЛЕНО: было _db.Tickets, стало _db.Transactions
    var hasTransactions = await _db.Transactions.AnyAsync(t => t.CategoryId == id);
    if (hasTransactions)
    {
        throw new InvalidOperationException("Невозможно удалить категорию: есть связанные финансовые записи");
    }
    
    _db.Categories.Remove(category);
    await _db.SaveChangesAsync();
    
    _logger.LogInformation("Категория {Id} удалена", id);
}
}