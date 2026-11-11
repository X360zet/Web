// server/ServiceDesk.API/Application/Services/ITransactionService.cs
using ServiceDesk.API.DTOs.Operations;
using ServiceDesk.API.DTOs.Transactions;

namespace ServiceDesk.API.Application.Services;

public interface ITransactionService
{
    // 🔹 Базовые операции (CRUD)
    Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, string currentUserId);
    Task<PagedResponse<TransactionResponse>> GetListAsync(TransactionsQuery query, string currentUserId, string currentRole);
    Task<TransactionResponse> GetByIdAsync(int id, string currentUserId, string currentRole);
    Task<TransactionResponse> UpdateAsync(int id, UpdateTransactionRequest request, string currentUserId, string currentRole);
    Task<TransactionResponse> DeleteAsync(int id, string currentUserId, string currentRole);
    
    // 🔹 Workflow оператора (Лаба 6)
    Task<TransactionResponse> AssignToMeAsync(int id, string currentUserId, string currentRole);
    Task<TransactionResponse> ChangeStatusAsync(int id, ChangeStatusRequest request, string currentUserId, string currentRole);
    Task<TransactionResponse> RejectAsync(int id, RejectRequest request, string currentUserId, string currentRole);
    
    // 🔹 Запрос пользователя на удаление (отправляется на одобрение оператору)
    Task<TransactionOperationResponse> RequestDeleteAsync(int id, string currentUserId);
}