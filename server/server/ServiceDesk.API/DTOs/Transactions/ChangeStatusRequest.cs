using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.API.DTOs.Transactions;

public sealed class ChangeStatusRequest
{
    [Required]
    public string Status { get; set; } = default!;
}