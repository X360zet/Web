// server/ServiceDesk.API/Domain/TransactionOperation.cs
using ServiceDesk.API.Infrastructure.Identity;

namespace ServiceDesk.API.Domain;

public class TransactionOperation
{
    public int Id { get; set; }
    
    // 🔹 Nullable: для операций типа Create транзакции ещё нет
    public int? TransactionId { get; set; }
    public Transaction? Transaction { get; set; }
    
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    
    public OperationType OperationType { get; set; }
    public OperationStatus Status { get; set; } = OperationStatus.Pending;
    
    public string? RejectionReason { get; set; }
    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAt { get; set; }
}

public enum OperationType { Create, Delete }
public enum OperationStatus { Pending, Approved, Rejected }