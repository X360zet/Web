using System.ComponentModel.DataAnnotations;
namespace ServiceDesk.API.DTOs.Categories;
public sealed class CreateCategoryRequest
{
    [Required, MaxLength(100)] public string Name { get; set; } = default!;
}