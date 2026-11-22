using System;

namespace BankManagementWpf.Models;

// Клас описує одну конкретну операцію (поповнення, зняття тощо)
public class Transaction
{
    public Guid Id { get; } = Guid.NewGuid();
    public Money Amount { get; }
    public TransactionType Type { get; }
    public DateTime Timestamp { get; }
    public string Description { get; }
    public bool IsSuccessful { get; }

    // Рядок для зручного виводу у список інтерфейсу
    public string DisplayValue => GetSummary();

    // Колір тексту для UI: зелений для поповнення, червоний для зняття або помилки
    public string DisplayColor
    {
        get
        {
            if (!IsSuccessful) return "#E74C3C";
            return Type == TransactionType.Deposit ? "#27AE60" : "#C0392B";
        }
    }

    public Transaction(Money amount, TransactionType type, string description, bool isSuccessful = true)
    {
        Amount = amount;
        Type = type;
        Description = description ?? "Без опису";
        Timestamp = DateTime.Now;
        IsSuccessful = isSuccessful;
    }

    public string GetSummary()
    {
        string status = IsSuccessful ? "OK" : "ПОМИЛКА";
        return $"[{Timestamp:dd.MM.yyyy HH:mm}] {Type}: {Amount} | {Description} ({status})";
    }
}