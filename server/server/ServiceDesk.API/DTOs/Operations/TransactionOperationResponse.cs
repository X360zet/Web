namespace ServiceDesk.API.DTOs.Operations;

public sealed class TransactionOperationResponse
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public string TransactionTitle { get; set; } = default!;
    public decimal Amount { get; set; }
    public string OperationType { get; set; } = default!;
    public string Status { get; set; } = default!;
    public string UserName { get; set; } = default!;
    public string? RejectionReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
}