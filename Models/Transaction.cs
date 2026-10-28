using System;

namespace BankManagement.Models;

// Клас описує одну конкретну операцію (поповнення, зняття тощо)
public class Transaction
{
    // Унікальний ID транзакції
    public Guid Id { get; } = Guid.NewGuid();
    
    // Сума грошей
    public Money Amount { get; }
    
    // Тип операції з enum
    public TransactionType Type { get; }
    
    // Точний час, коли зробили транзакцію
    public DateTime Timestamp { get; }
    
    // Короткий опис, за що або куди переказ
    public string Description { get; }
    
    // Чи успішно пройшла операція
    public bool IsSuccessful { get; }

    // Конструктор для заповнення даних транзакції
    public Transaction(Money amount, TransactionType type, string description, bool isSuccessful = true)
    {
        Amount = amount;
        Type = type;
        Description = description ?? "Без опису";
        Timestamp = DateTime.Now;
        IsSuccessful = isSuccessful;
    }

    // Метод, який збирає всі дані про транзакцію в один рядок
    public string GetSummary()
    {
        return $"[{Timestamp:yyyy-MM-dd HH:mm}] {Type}: {Amount} | {Description} (Успішно: {IsSuccessful})";
    }
}