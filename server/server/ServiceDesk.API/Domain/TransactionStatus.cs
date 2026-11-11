namespace ServiceDesk.API.Domain;

public enum TransactionStatus
{
    Pending,    // Ожидает проверки
    InProgress, // Взято контролёром на проверку
    Approved,   // Одобрено
    Rejected    // Отклонено
}

public enum TransactionType
{
    Expense,
    Income
}