// server/ServiceDesk.API/DTOs/Categories/SetActiveCategoryRequest.cs
namespace ServiceDesk.API.DTOs.Categories;

public sealed class SetActiveCategoryRequest
{
    public bool IsActive { get; set; }
}