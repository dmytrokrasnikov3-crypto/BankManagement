using System;

namespace BankManagement.Models;

// Ощадний рахунок (нащадок базового класу Account)
public class SavingsAccount : Account
{
    // Процентна ставка
    public decimal InterestRate { get; private set; }
    
    // Дата останнього нарахування відсотків
    public DateTime LastInterestApplied { get; private set; }
    
    // Мінімальний за залишок, який має залишатися на картці
    public decimal MinimumBalance { get; private set; }
    
    // Періодичність виплати (наприклад, 30 днів)
    public int AutoCapitalizationDays { get; private set; } = 30;
    
    // Чи заблоковане зняття коштів
    public bool IsLocked { get; private set; }

    // Конструктор передає базові дані в батьківський клас через base()
    public SavingsAccount(string accountNumber, string ownerName, decimal initialBalance, decimal interestRate, decimal minimumBalance = 100m)
        : base(accountNumber, ownerName, initialBalance)
    {
        InterestRate = interestRate > 0 ? interestRate : 1.0m;
        MinimumBalance = minimumBalance;
        LastInterestApplied = DateTime.Now;
        IsLocked = false;
    }

    // Перевизначений метод зняття коштів із додатковими перевірками
    public override bool Withdraw(decimal amount, string description = "Зняття з ощадного рахунку")
    {
        try
        {
            if (IsLocked)
                throw new InvalidOperationException("Рахунок заблоковано для зняття.");

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Сума зняття повинна бути більшою за нуль.");

            // Перевіряємо, щоб після зняття залишився мінімальний баланс
            if (Balance - amount < MinimumBalance)
                throw new InvalidOperationException($"Неможливо зняти кошти. Необхідний мінімальний залишок: {MinimumBalance} UAH.");

            Balance -= amount;
            var money = new Money(amount);
            
            // Записуємо транзакцію зняття
            Transactions.Add(new Transaction(money, TransactionType.Withdrawal, description, true));
            Console.WriteLine($"[УСПІХ] Знято {amount} UAH з {AccountNumber}. Поточний баланс: {Balance} UAH.");
            return true;
        }
        catch (Exception ex)
        {
            // Перехоплюємо помилки зняття (наприклад, коли бракує коштів)
            Transactions.Add(new Transaction(new Money(0), TransactionType.Withdrawal, $"Невдале зняття: {ex.Message}", false));
            Console.WriteLine($"[ПОМИЛКА ЗНЯТТЯ] {ex.Message}");
            return false;
        }
    }

    // Метод для виплати відсотків за депозитом
    public void ApplyInterest()
    {
        try
        {
            decimal interest = Balance * (InterestRate / 100m);
            Deposit(interest, $"Нарахування відсотків ({InterestRate}%)");
            LastInterestApplied = DateTime.Now;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ПОМИЛКА НАРАХУВАННЯ ВІДСОТКІВ] {ex.Message}");
        }
    }
}