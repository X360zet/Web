using ServiceDesk.API.Domain;

namespace ServiceDesk.API.DTOs.Transactions;

public sealed class TransactionsQuery
{
    public TransactionStatus? Status { get; set; }
    public int? CategoryId { get; set; }
    
    // 🔹 ДОБАВЬТЕ ЭТИ СВОЙСТВА (если их нет):
    public bool? AssignedToMe { get; set; }
    public bool? UnassignedOnly { get; set; }
    
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool Descending { get; set; } = true;
}

public sealed class PagedResponse<T>
{
    public IReadOnlyList<T> Items { get; set; } = default!;
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}