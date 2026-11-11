using Microsoft.EntityFrameworkCore;
using ServiceDesk.API.Domain;
using ServiceDesk.API.DTOs.Operations;
using ServiceDesk.API.DTOs.Transactions;
using ServiceDesk.API.Infrastructure.Data;

namespace ServiceDesk.API.Application.Services;

public class TransactionOperationService : ITransactionOperationService
{
    private readonly AppDbContext _db;

    public TransactionOperationService(AppDbContext db) => _db = db;

    public async Task<TransactionOperationResponse> CreateOperationAsync(TransactionOperationRequest request, string userId)
    {
        var tx = await _db.Transactions.FindAsync(request.TransactionId) 
            ?? throw new KeyNotFoundException("Транзакция не найдена");
            
        var operation = new TransactionOperation
        {
            TransactionId = tx.Id,
            UserId = userId,
            OperationType = request.OperationType,
            Status = OperationStatus.Pending
        };
        _db.TransactionOperations.Add(operation);
        await _db.SaveChangesAsync();
        return MapToResponse(operation, tx);
    }

    public async Task<PagedResponse<TransactionOperationResponse>> GetPendingOperationsAsync(string operatorId, int page, int pageSize)
    {
        var query = _db.TransactionOperations
            .Include(o => o.Transaction)
            .Include(o => o.User)
            .Where(o => o.Status == OperationStatus.Pending)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResponse<TransactionOperationResponse>
        {
            Items = items.Select(op => MapToResponse(op, op.Transaction)).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<TransactionOperationResponse> ApproveOperationAsync(int operationId, string operatorId, ApproveOperationRequest? request)
    {
        var operation = await _db.TransactionOperations
            .Include(o => o.Transaction)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == operationId)
            ?? throw new KeyNotFoundException("Операция не найдена");

        if (operation.Status != OperationStatus.Pending)
            throw new InvalidOperationException("Операция уже обработана");

        // 🔹 Сохраняем данные ДО любых изменений, чтобы вернуть их в ответе
        var txId = operation.TransactionId;
        var tx = operation.Transaction;
        var opType = operation.OperationType;
        var title = tx?.Title ?? "Неизвестно";
        var amount = tx?.Amount ?? 0;

        if (opType == OperationType.Create)
        {
            // 🔹 ОДОБРЕНИЕ СОЗДАНИЯ
            if (tx != null)
            {
                tx.Status = TransactionStatus.Approved;
                tx.ApprovedAt = DateTimeOffset.UtcNow;
            }
        }
        else if (opType == OperationType.Delete)
        {
            // 🔹 ОДОБРЕНИЕ УДАЛЕНИЯ
            // Используем "ядерный" вариант: обнуляем ВСЕ ссылки на эту транзакцию
            
            if (txId.HasValue)
            {
                // 1. УДАЛЯЕМ ВСЕ ССЫЛКИ НА ЭТУ ТРАНЗАКЦИЮ В ТАБЛИЦЕ ОПЕРАЦИЙ
                // Это гарантирует, что FK constraint не заблокирует удаление, 
                // даже если есть другие операции, указывающие на ту же транзакцию.
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE [TransactionOperations] SET [TransactionId] = NULL WHERE [TransactionId] = {0}", txId.Value);

                // 2. ТЕПЕРЬ БЕЗОПАСНО УДАЛЯЕМ САМУ ТРАНЗАКЦИЮ
                await _db.Database.ExecuteSqlRawAsync(
                    "DELETE FROM [Transactions] WHERE [Id] = {0}", txId.Value);
            }
            
            // 3. Обновляем текущую операцию
            operation.Status = OperationStatus.Approved;
            operation.AssigneeId = operatorId;
            operation.ProcessedAt = DateTimeOffset.UtcNow;
            
            // ВАЖНО: Обнуляем ссылку в памяти, чтобы при сохранении EF Core 
            // не попытался вернуть старое значение TransactionId в базу данных.
            operation.TransactionId = null;
            operation.Transaction = null; // Отсоединяем объект
            
            // 4. Сохраняем статус операции
            await _db.SaveChangesAsync();

            // Возвращаем результат
            return new TransactionOperationResponse
            {
                Id = operation.Id,
                TransactionId = txId ?? 0, // Возвращаем ID для истории (хотя в БД он уже null)
                TransactionTitle = title,  // Возвращаем название (чтобы в UI не было нулей)
                Amount = amount,           // Возвращаем сумму
                OperationType = opType.ToString(),
                Status = "Approved",
                UserName = operation.User?.DisplayName ?? "User",
                CreatedAt = operation.CreatedAt,
                ProcessedAt = operation.ProcessedAt
            };
        }

        // Для Create сохраняем стандартным способом
        operation.Status = OperationStatus.Approved;
        operation.AssigneeId = operatorId;
        operation.ProcessedAt = DateTimeOffset.UtcNow;
        
        await _db.SaveChangesAsync();

        return new TransactionOperationResponse
        {
            Id = operation.Id,
            TransactionId = txId ?? 0,
            TransactionTitle = title,
            Amount = amount,
            OperationType = opType.ToString(),
            Status = "Approved",
            UserName = operation.User?.DisplayName ?? "User",
            CreatedAt = operation.CreatedAt,
            ProcessedAt = operation.ProcessedAt
        };
    }

    public async Task<TransactionOperationResponse> RejectOperationAsync(int operationId, string operatorId, string reason)
    {
        var operation = await _db.TransactionOperations
            .Include(o => o.Transaction)
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == operationId)
            ?? throw new KeyNotFoundException("Операция не найдена");

        if (operation.Status != OperationStatus.Pending)
            throw new InvalidOperationException("Операция уже обработана");

        var txId = operation.TransactionId;
        var tx = operation.Transaction;
        var opType = operation.OperationType;
        var title = tx?.Title ?? "Неизвестно";
        var amount = tx?.Amount ?? 0;

        // Если отклоняем создание — удаляем черновик транзакции
        if (opType == OperationType.Create && txId.HasValue)
        {
            // Разрываем связь
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE [TransactionOperations] SET [TransactionId] = NULL WHERE [TransactionId] = {0}", txId.Value);
            
            _db.Entry(operation).State = EntityState.Detached;

            // Удаляем черновик
            await _db.Database.ExecuteSqlRawAsync(
                "DELETE FROM [Transactions] WHERE [Id] = {0}", txId.Value);
        }

        operation.Status = OperationStatus.Rejected;
        operation.AssigneeId = operatorId;
        operation.RejectionReason = reason;
        operation.ProcessedAt = DateTimeOffset.UtcNow;
        operation.TransactionId = null;
        
        await _db.SaveChangesAsync();

        return new TransactionOperationResponse
        {
            Id = operation.Id,
            TransactionId = txId ?? 0,
            TransactionTitle = title,
            Amount = amount,
            OperationType = opType.ToString(),
            Status = "Rejected",
            UserName = operation.User?.DisplayName ?? "User",
            RejectionReason = reason,
            CreatedAt = operation.CreatedAt,
            ProcessedAt = operation.ProcessedAt
        };
    }

    private static TransactionOperationResponse MapToResponse(TransactionOperation op, Transaction? tx)
    {
        return new TransactionOperationResponse
        {
            Id = op.Id,
            TransactionId = op.TransactionId ?? tx?.Id ?? 0,
            TransactionTitle = tx?.Title ?? "Новая транзакция",
            Amount = tx?.Amount ?? 0,
            OperationType = op.OperationType.ToString(),
            Status = op.Status.ToString(),
            UserName = op.User?.DisplayName ?? "User",
            RejectionReason = op.RejectionReason,
            CreatedAt = op.CreatedAt,
            ProcessedAt = op.ProcessedAt
        };
    }
}