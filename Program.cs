using System;
using BankManagement.Models;
using BankManagement.Services;

namespace BankManagement;

class Program
{
    static void Main(string[] args)
    {
        Console.Title = "Bank Management System v1.0 — Менеджер Рахунків";
        Console.WriteLine("==================================================");
        Console.WriteLine("   СИСТЕМА УПРАВЛІННЯ БАНКІВСЬКИМИ РАХУНКАМИ");
        Console.WriteLine("==================================================\n");

        // Ініціалізація банку
        Bank myBank = new Bank("Monobank", "MONOUA2X");

        // Створення рахунків
        var acc1 = new SavingsAccount("UA111222333", "Дмитро Красніков", 1000m, 5.0m, 200m);
        var acc2 = new SavingsAccount("UA444555666", "Олена Краснікова", 500m, 3.5m, 100m);

        myBank.OpenAccount(acc1);
        myBank.OpenAccount(acc2);

        // Поповнення
        acc1.Deposit(500m);
            
        // Перевірка обробки try-catch при спробі зняти більше дозволеного залишку
        acc2.Withdraw(450m); // Спровокує виняток щодо мінімального залишку (100 UAH)

        // Переказ коштів між рахунками
        myBank.TransferFunds("UA111222333", "UA444555666", 300m);

        // Вивід підсумків
        Console.WriteLine($"\nЗагальний капітал банку: {myBank.TotalBankCapital} UAH");
        Console.WriteLine($"Всього рахунків: {myBank.TotalAccountsCount}");

        Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
        Console.ReadKey();
    }
}