using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;  // ← Добавьте этот using
using ServiceDesk.API.Domain;

namespace ServiceDesk.API.DTOs.Transactions;

public sealed class CreateTransactionRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string Title { get; set; } = default!;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required, Range(0.01, 1000000)]
    public decimal Amount { get; set; }
    
    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]  // ← ДОБАВЬТЕ ЭТУ СТРОКУ
    public TransactionType Type { get; set; }
    
    [Required]
    public int CategoryId { get; set; }
    
    public DateTimeOffset? TransactionDate { get; set; }
}