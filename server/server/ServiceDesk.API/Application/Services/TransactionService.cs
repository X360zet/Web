// server/ServiceDesk.API/Application/Services/TransactionService.cs
using Microsoft.EntityFrameworkCore;
using ServiceDesk.API.Application.Mapping;
using ServiceDesk.API.Domain;
using ServiceDesk.API.DTOs.Operations;
using ServiceDesk.API.DTOs.Transactions;
using ServiceDesk.API.Infrastructure.Data;

namespace ServiceDesk.API.Application.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _db;

    public TransactionService(AppDbContext db) => _db = db;

    // ==========================================
    // ГЛАВНОЕ ИСПРАВЛЕНИЕ: Фильтрация списка
    // ==========================================
    public async Task<PagedResponse<TransactionResponse>> GetListAsync(TransactionsQuery query, string currentUserId, string currentRole)
    {
        var transactions = _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.User)
            .Include(t => t.Assignee)
            .AsQueryable();

        // 🔹 ФИЛЬТРАЦИЯ ПО РОЛЯМ
        if (currentRole == "Operator")
        {
            // Оператор видит заявки на проверку (Pending)  ВОПРОС!!!
            if (query.UnassignedOnly == true)
                transactions = transactions.Where(t => t.Status == TransactionStatus.Pending && t.AssigneeId == null);
            else if (query.AssignedToMe == true)
                transactions = transactions.Where(t => t.AssigneeId == currentUserId);
            else
                transactions = transactions.Where(t => t.AssigneeId == currentUserId);
        }
        else if (currentRole == "User")
        {
            // 🔹 ПОЛЬЗОВАТЕЛЬ ВИДИТ ТОЛЬКО ОДОБРЕННЫЕ ТРАНЗАКЦИИ (или Rejected, если нужно)
            // Pending (черновики) скрыты
            transactions = transactions.Where(t => t.UserId == currentUserId && t.Status != TransactionStatus.Pending);
        }

        if (query.Status.HasValue) transactions = transactions.Where(t => t.Status == query.Status);
        if (query.CategoryId.HasValue) transactions = transactions.Where(t => t.CategoryId == query.CategoryId);

        var totalCount = await transactions.CountAsync();
        var items = await transactions
            .OrderByDescending(t => t.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(t => t.ToResponse())
            .ToListAsync();

        return new PagedResponse<TransactionResponse> 
        { 
            Items = items, 
            TotalCount = totalCount, 
            Page = query.Page, 
            PageSize = query.PageSize 
        };
    }

    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, string currentUserId)
    {
        var category = await _db.Categories.FindAsync(request.CategoryId)
            ?? throw new KeyNotFoundException("Категория не найдена.");

        // 1. Создаем транзакцию в статусе Pending (Черновик)
        var transaction = new Transaction
        {
            Title = request.Title,
            Description = request.Description,
            Amount = request.Amount,
            Type = request.Type,
            Status = TransactionStatus.Pending, // Скрыта от пользователя (см. GetListAsync)
            CategoryId = request.CategoryId,
            UserId = currentUserId,
            TransactionDate = request.TransactionDate ?? DateTimeOffset.UtcNow
        };
        _db.Transactions.Add(transaction);
        await _db.SaveChangesAsync(); 

        // 2. Создаем операцию на одобрение
        var operation = new TransactionOperation
        {
            TransactionId = transaction.Id,
            UserId = currentUserId,
            OperationType = OperationType.Create,
            Status = OperationStatus.Pending
        };
        _db.TransactionOperations.Add(operation);
        await _db.SaveChangesAsync();
        
        return transaction.ToResponse();
    }

    public async Task<TransactionResponse> GetByIdAsync(int id, string currentUserId, string currentRole)
    {
        var tx = await _db.Transactions
            .Include(t => t.Category)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Транзакция не найдена.");

        if (currentRole == "User" && tx.UserId != currentUserId) throw new UnauthorizedAccessException();
        return tx.ToResponse();
    }

    public async Task<TransactionResponse> UpdateAsync(int id, UpdateTransactionRequest request, string currentUserId, string currentRole)
    {
        var tx = await _db.Transactions.FindAsync(id) ?? throw new KeyNotFoundException("Транзакция не найдена.");
        if (currentRole == "User" && tx.UserId != currentUserId) throw new UnauthorizedAccessException();

        if (!string.IsNullOrWhiteSpace(request.Title)) tx.Title = request.Title;
        if (request.Description != null) tx.Description = request.Description;
        if (request.Amount.HasValue) tx.Amount = request.Amount.Value;
        if (request.CategoryId.HasValue) tx.CategoryId = request.CategoryId.Value;
        
        tx.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return tx.ToResponse();
    }

    public async Task<TransactionResponse> DeleteAsync(int id, string currentUserId, string currentRole)
    {
        var tx = await _db.Transactions.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.Transactions.Remove(tx);
        await _db.SaveChangesAsync();
        return tx.ToResponse();
    }

    public async Task<TransactionOperationResponse> RequestDeleteAsync(int id, string currentUserId)
    {
        var tx = await _db.Transactions.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new KeyNotFoundException("Транзакция не найдена.");

        if (tx.UserId != currentUserId) throw new UnauthorizedAccessException();

        var operation = new TransactionOperation
        {
            TransactionId = tx.Id,
            UserId = currentUserId,
            OperationType = OperationType.Delete,
            Status = OperationStatus.Pending
        };

        _db.TransactionOperations.Add(operation);
        await _db.SaveChangesAsync();

        return new TransactionOperationResponse
        {
            Id = operation.Id,
            TransactionId = tx.Id,
            TransactionTitle = tx.Title,
            Amount = tx.Amount,
            OperationType = "Delete",
            Status = "Pending",
            UserName = tx.User?.DisplayName,
            CreatedAt = operation.CreatedAt
        };
    }

    public async Task<TransactionResponse> AssignToMeAsync(int id, string currentUserId, string currentRole)
    {
        var tx = await _db.Transactions.FindAsync(id) ?? throw new KeyNotFoundException();
        if (tx.Status != TransactionStatus.Pending) throw new InvalidOperationException("Только новые.");
        if (tx.AssigneeId != null && tx.AssigneeId != currentUserId) throw new InvalidOperationException("Уже назначено.");

        tx.AssigneeId = currentUserId;
        tx.Status = TransactionStatus.InProgress;
        await _db.SaveChangesAsync();
        return tx.ToResponse();
    }

    public async Task<TransactionResponse> ChangeStatusAsync(int id, ChangeStatusRequest request, string currentUserId, string currentRole)
    {
        var tx = await _db.Transactions.FindAsync(id) ?? throw new KeyNotFoundException();
        if (tx.AssigneeId != currentUserId) throw new UnauthorizedAccessException();
        
        tx.Status = Enum.Parse<TransactionStatus>(request.Status, true);
        if (tx.Status == TransactionStatus.Approved) tx.ApprovedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return tx.ToResponse();
    }

    public async Task<TransactionResponse> RejectAsync(int id, RejectRequest request, string currentUserId, string currentRole)
    {
        var tx = await _db.Transactions.FindAsync(id) ?? throw new KeyNotFoundException();
        if (tx.AssigneeId != currentUserId) throw new UnauthorizedAccessException();
        
        tx.Status = TransactionStatus.Rejected;
        tx.RejectionReason = request.Reason;
        await _db.SaveChangesAsync();
        return tx.ToResponse();
    }
}
//Form.useForm()!!!
//Это хук, предоставляемый библиотекой Ant Design, который создаёт и возвращает экземпляр формы.
//Он позволяет управлять формой программно 
//(из JavaScript/TypeScript кода), а не только через интерфейс.
//Form (с большой буквы) — это Компонент
//useForm — это Статический метод (Хук)
//form (с маленькой буквы) — это Переменная (экземпляр)





//transactions.Where(t => t.Status == TransactionStatus.Pending && t.AssigneeId == null)!!!
//transactions → исходный запрос к таблице Transactions (данные ещё не загружены из БД)
//.Where(...) → добавляет условие фильтрации
//t => ... → лямбда-выражение, где t это каждая строка таблицы
//t.Status == TransactionStatus.Pending → оставить только транзакции со статусом Pending 
//(ожидает одобрения)
//&& t.AssigneeId == null → И только те, у которых поле AssigneeId пустое 
//(никто из операторов ещё не взял заявку)