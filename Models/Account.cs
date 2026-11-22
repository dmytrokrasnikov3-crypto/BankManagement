using System;
using System.Collections.Generic;

namespace BankManagementWpf.Models;

// Реалізація інтерфейсу ITransactionable
public abstract class Account : ITransactionable
{
    // Подія (events) із використанням делегата EventHandler<string>
    public event EventHandler<string>? NotificationOccurred;

    public Guid Id { get; } = Guid.NewGuid();
    public string AccountNumber { get; protected set; }
    public string OwnerName { get; protected set; }
    public decimal Balance { get; protected set; }
    public List<Transaction> Transactions { get; } = new List<Transaction>();
    public DateTime CreatedAt { get; } = DateTime.Now;
    public bool IsActive { get; protected set; } = true;

    protected Account(string accountNumber, string ownerName, decimal initialBalance)
    {
        if (string.IsNullOrWhiteSpace(accountNumber))
            throw new ArgumentException("Номер рахунку не може бути порожнім.");
            
        if (string.IsNullOrWhiteSpace(ownerName))
            throw new ArgumentException("Ім'я власника не може бути порожнім.");

        AccountNumber = accountNumber;
        OwnerName = ownerName;
        Balance = initialBalance >= 0 ? initialBalance : throw new ArgumentException("Початковий баланс не може бути від'ємним.");
    }

    // Метод для виклику події
    protected void RaiseNotification(string message)
    {
        NotificationOccurred?.Invoke(this, message);
    }

    public virtual bool Deposit(decimal amount, string description = "Поповнення рахунку")
    {
        try
        {
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Сума поповнення повинна бути більшою за нуль.");

            Balance += amount;
            var money = new Money(amount);
            Transactions.Add(new Transaction(money, TransactionType.Deposit, description, true));

            // Тригер події через метод
            RaiseNotification($"[УСПІХ] Рахунок {AccountNumber} поповнено на {amount} UAH.");
            return true;
        }
        catch (Exception ex)
        {
            Transactions.Add(new Transaction(new Money(0), TransactionType.Deposit, $"Невдале поповнення: {ex.Message}", false));
            RaiseNotification($"[ПОМИЛКА] {ex.Message}");
            return false;
        }
    }

    public abstract bool Withdraw(decimal amount, string description = "Зняття коштів");

    public IReadOnlyList<Transaction> GetTransactionHistory() => Transactions.AsReadOnly();
}