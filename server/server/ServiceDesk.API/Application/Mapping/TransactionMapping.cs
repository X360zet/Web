using ServiceDesk.API.Domain;
using ServiceDesk.API.DTOs.Transactions;
using ServiceDesk.API.DTOs.Users;

namespace ServiceDesk.API.Application.Mapping;

public static class TransactionMapping
{
    public static TransactionResponse ToResponse(this Transaction transaction) => new()
    {
        Id = transaction.Id,
        Title = transaction.Title,
        Description = transaction.Description,
        Amount = transaction.Amount,
        Type = transaction.Type,
        // 🔹 ИСПРАВЛЕНО: присваиваем enum напрямую, без .ToString()
        Status = transaction.Status, 
        CategoryId = transaction.CategoryId,
        CategoryName = transaction.Category.Name,
        User = new UserInfoDto
        {
            Id = transaction.User.Id,
            DisplayName = transaction.User.DisplayName,
            Email = transaction.User.Email!,
            Role = "User"
        },
        Approver = transaction.Assignee == null ? null : new UserInfoDto
        {
            Id = transaction.Assignee.Id,
            DisplayName = transaction.Assignee.DisplayName,
            Email = transaction.Assignee.Email!,
            Role = "Operator"
        },
        CreatedAt = transaction.CreatedAt,
        TransactionDate = transaction.TransactionDate,
        ApprovedAt = transaction.ApprovedAt
    };
}