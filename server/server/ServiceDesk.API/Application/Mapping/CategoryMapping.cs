using ServiceDesk.API.Domain;
using ServiceDesk.API.DTOs.Categories;

namespace ServiceDesk.API.Application.Mapping;

public static class CategoryMapping
{
    public static CategoryResponse ToResponse(this Category c) => new()
    {
        Id = c.Id, Name = c.Name, IsActive = c.IsActive
    };
}