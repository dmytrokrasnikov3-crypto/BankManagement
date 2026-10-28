using System;
using System.Collections.Generic;

namespace BankManagement.Models;

// Базовий абстрактний клас для будь-якого рахунку в банку
public abstract class Account
{
    public Guid Id { get; } = Guid.NewGuid();
    public string AccountNumber { get; protected set; }
    public string OwnerName { get; protected set; }
    public decimal Balance { get; protected set; }
    
    // Список усіх транзакцій за цим рахунком (агрегація)
    public List<Transaction> Transactions { get; } = new List<Transaction>();
    
    public DateTime CreatedAt { get; } = DateTime.Now;
    public bool IsActive { get; protected set; } = true;

    // Конструктор перевіряє вхідні дані при створенні рахунку
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

    // Поповнення рахунку з обробкою помилок (try-catch)
    public virtual bool Deposit(decimal amount, string description = "Поповнення рахунку")
    {
        try
        {
            // Не дозволяємо поповнювати на 0 або від'ємні суми
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Сума поповнення повинна бути більшою за нуль.");
            }

            Balance += amount;
            var money = new Money(amount);
            
            // Записуємо успішну транзакцію
            Transactions.Add(new Transaction(money, TransactionType.Deposit, description, true));
            Console.WriteLine($"[УСПІХ] Рахунок {AccountNumber} поповнено на {amount} UAH. Поточний баланс: {Balance} UAH.");
            return true;
        }
        catch (Exception ex)
        {
            // Якщо сталася помилка, фіксуємо невдалу транзакцію й виводимо повідомлення
            Transactions.Add(new Transaction(new Money(0), TransactionType.Deposit, $"Невдале поповнення: {ex.Message}", false));
            Console.WriteLine($"[ПОМИЛКА РОБОТИ РАХУНКУ] Не вдалося поповнити: {ex.Message}");
            return false;
        }
    }

    // Абстрактний метод для зняття (кожен тип рахунку реалізує його по-своєму)
    public abstract bool Withdraw(decimal amount, string description = "Зняття коштів");

    // Повертаємо історію операцій для перегляду
    public IReadOnlyList<Transaction> GetTransactionHistory() => Transactions.AsReadOnly();
}