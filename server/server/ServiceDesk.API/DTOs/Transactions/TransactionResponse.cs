using ServiceDesk.API.Domain;
using ServiceDesk.API.DTOs.Users;

namespace ServiceDesk.API.DTOs.Transactions;

public sealed class TransactionResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = default!;
    
    public UserInfoDto User { get; set; } = default!;
    public UserInfoDto? Approver { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? TransactionDate { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}