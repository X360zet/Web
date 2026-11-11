using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.API.DTOs.Transactions;

public sealed class UpdateTransactionRequest
{
    [StringLength(200)]
    public string? Title { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Range(0.01, 1000000)]
    public decimal? Amount { get; set; }
    
    public int? CategoryId { get; set; }
    
    public DateTimeOffset? TransactionDate { get; set; }
}