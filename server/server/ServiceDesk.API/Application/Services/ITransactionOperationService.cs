using ServiceDesk.API.DTOs.Operations;
using ServiceDesk.API.DTOs.Transactions;

namespace ServiceDesk.API.Application.Services;

public interface ITransactionOperationService
{
    Task<TransactionOperationResponse> CreateOperationAsync(TransactionOperationRequest request, string userId);
    Task<PagedResponse<TransactionOperationResponse>> GetPendingOperationsAsync(string operatorId, int page = 1, int pageSize = 10);
    Task<TransactionOperationResponse> ApproveOperationAsync(int operationId, string operatorId, ApproveOperationRequest? request = null);
    Task<TransactionOperationResponse> RejectOperationAsync(int operationId, string operatorId, string reason);
}