namespace ServiceDesk.API.DTOs.Categories;
public sealed class CategoryResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; }
}