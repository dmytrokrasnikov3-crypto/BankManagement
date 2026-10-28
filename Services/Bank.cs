using System;
using System.Collections.Generic;
using System.Linq;
using BankManagement.Models;

namespace BankManagement.Services;

// Клас банку, який зберігає список усіх рахунків та робить перекази між ними
public class Bank
{
    public string BankName { get; }
    public string SwiftCode { get; }
    
    // Список рахунків у банку (агрегація)
    public List<Account> Accounts { get; } = new List<Account>();
    
    // Загальна кількість рахунків
    public int TotalAccountsCount => Accounts.Count;
    
    // Сума всіх грошей на всіх рахунках банку
    public decimal TotalBankCapital => Accounts.Sum(a => a.Balance);
    
    // Чи працює система банку
    public bool IsOperational { get; private set; } = true;

    public Bank(string bankName, string swiftCode)
    {
        BankName = bankName ?? "Головний Банк";
        SwiftCode = swiftCode ?? "BANKUA2X";
    }

    // Додаємо новий рахунок у банк
    public bool OpenAccount(Account account)
    {
        try
        {
            if (account == null) throw new ArgumentNullException(nameof(account));
                
            // Перевіряємо, чи немає вже рахунку з таким номером
            if (Accounts.Any(a => a.AccountNumber == account.AccountNumber))
                throw new InvalidOperationException($"Рахунок з номером {account.AccountNumber} вже існує.");

            Accounts.Add(account);
            Console.WriteLine($"[БАНК] Відкрито новий рахунок {account.AccountNumber} для {account.OwnerName}.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ПОМИЛКА ВІДКРИТТЯ РАХУНКУ] {ex.Message}");
            return false;
        }
    }

    // Метод переказу грошей з одного рахунку на інший
    public bool TransferFunds(string sourceAccNum, string targetAccNum, decimal amount)
    {
        try
        {
            Console.WriteLine($"\n--- Спроба переказу {amount} UAH з {sourceAccNum} на {targetAccNum} ---");
            
            // Шукаємо обидва рахунки
            var source = FindAccount(sourceAccNum);
            var target = FindAccount(targetAccNum);

            if (source == null) throw new KeyNotFoundException($"Рахунок-відправник {sourceAccNum} не знайдено.");
            if (target == null) throw new KeyNotFoundException($"Рахунок-отримувач {targetAccNum} не знайдено.");

            // Спочатку знімаємо з першого, якщо вдалося — поповнюємо другий
            if (source.Withdraw(amount, $"Переказ на рахунок {targetAccNum}"))
            {
                target.Deposit(amount, $"Переказ від рахунку {sourceAccNum}");
                Console.WriteLine("[УСПІХ ПЕРЕКАЗУ] Транзакцію успішно завершено.");
                return true;
            }

            throw new InvalidOperationException("Не вдалося списати кошти з рахунку-відправника.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[КРИТИЧНА ПОМИЛКА ПЕРЕКАЗУ] {ex.Message}");
            return false;
        }
    }

    // Допоміжний метод для пошуку рахунку за номером
    public Account? FindAccount(string accountNumber)
    {
        return Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
    }
}