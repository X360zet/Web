namespace ServiceDesk.API.Domain;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
}