using System;
using System.Collections.Generic;
using System.Linq;
using BankManagementWpf.Models;

namespace BankManagementWpf.Services;

public class Bank
{
    public string BankName { get; }
    public string SwiftCode { get; }
    public List<Account> Accounts { get; } = new List<Account>();
    
    // Використання ЛЯМБДА-ВИРАЗІВ у властивостях
    public int TotalAccountsCount => Accounts.Count;
    public decimal TotalBankCapital => Accounts.Sum(a => a.Balance);
    public bool IsOperational { get; private set; } = true;

    // Індексатор із лямбда-виразом
    public Account? this[string accountNumber] => FindAccount(accountNumber);

    public Bank(string bankName, string swiftCode)
    {
        BankName = bankName ?? "Головний Банк";
        SwiftCode = swiftCode ?? "BANKUA2X";
    }

    public bool OpenAccount(Account account)
    {
        try
        {
            if (account == null) throw new ArgumentNullException(nameof(account));

            // Лямбда-вираз в LINQ (a => a.AccountNumber == account.AccountNumber)
            if (Accounts.Any(a => a.AccountNumber == account.AccountNumber))
                throw new InvalidOperationException($"Рахунок з номером {account.AccountNumber} вже існує.");

            Accounts.Add(account);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ПОМИЛКА ВІДКРИТТЯ РАХУНКУ] {ex.Message}");
            return false;
        }
    }

    // Метод пошуку VIP-рахунків з балансом більше суми (Лямбда-вираз та Predicate)
    public List<Account> GetAccountsByCondition(Func<Account, bool> predicate)
    {
        return Accounts.Where(predicate).ToList();
    }

    public bool TransferFunds(string sourceAccNum, string targetAccNum, decimal amount)
    {
        try
        {
            var source = this[sourceAccNum];
            var target = this[targetAccNum];

            if (source == null) throw new KeyNotFoundException($"Рахунок-відправник {sourceAccNum} не знайдено.");
            if (target == null) throw new KeyNotFoundException($"Рахунок-отримувач {targetAccNum} не знайдено.");

            if (source.Withdraw(amount, $"Переказ на рахунок {targetAccNum}"))
            {
                target.Deposit(amount, $"Переказ від рахунку {sourceAccNum}");
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

    // Пошук з лямбда-виразом
    public Account? FindAccount(string accountNumber) => 
        Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);
}