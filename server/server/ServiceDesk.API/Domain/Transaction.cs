using ServiceDesk.API.Infrastructure.Identity;

namespace ServiceDesk.API.Domain;

public class Transaction
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    // 🔹 Поля workflow контролёра (Lab 6)
    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }
    public string? RejectionReason { get; set; }
    
    // 🔹 Поля для отображения и сортировки
    public DateTimeOffset? TransactionDate { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
}