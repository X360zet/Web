using ServiceDesk.API.Infrastructure.Identity;

namespace ServiceDesk.API.Domain;

public class Ticket
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    
    // Связь с категорией
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    // Связь с автором (Студент)
    public string AuthorId { get; set; } = default!;
    public ApplicationUser Author { get; set; } = default!;

    // Связь с исполнителем (Оператор) - может быть null
    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}