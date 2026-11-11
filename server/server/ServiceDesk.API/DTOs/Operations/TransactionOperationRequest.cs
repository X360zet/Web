using System.ComponentModel.DataAnnotations;
using ServiceDesk.API.Domain;

namespace ServiceDesk.API.DTOs.Operations;

public sealed class TransactionOperationRequest
{
    [Required]
    public OperationType OperationType { get; set; }
    
    [Required]
    public int TransactionId { get; set; }
    
    [MaxLength(500)]
    public string? Comment { get; set; }  // Комментарий от пользователя
}

public sealed class ApproveOperationRequest
{
    [MaxLength(500)]
    public string? Comment { get; set; }
}