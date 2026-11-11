using ServiceDesk.API.DTOs.Categories;

namespace ServiceDesk.API.Application.Services;

public interface ICategoryService
{
    // 🔹 Два параметра: includeInactive и role (для фильтрации по роли)
    Task<IReadOnlyList<CategoryResponse>> GetAllAsync(bool includeInactive, string role);
    
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    
    Task<CategoryResponse> UpdateAsync(int id, UpdateCategoryRequest request);
    
    // 🔹 Принимает DTO, а не bool
    Task<CategoryResponse> SetActiveAsync(int id, SetActiveCategoryRequest request);
    
    // 🔹 Метод удаления (для Лабы 4+)
    Task DeleteAsync(int id);
}