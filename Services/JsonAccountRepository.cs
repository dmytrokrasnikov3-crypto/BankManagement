using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BankManagementWpf.Models;

namespace BankManagementWpf.Services;

// Асинхронне збереження та завантаження рахунків у JSON
public class JsonAccountRepository : IAccountRepository
{
    private readonly string _filePath;

    public JsonAccountRepository(string filePath = "accounts_data.json")
    {
        _filePath = filePath;
    }

    public async Task<List<Account>> LoadAccountsAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return new List<Account>();

            using FileStream stream = File.OpenRead(_filePath);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            var accounts = await JsonSerializer.DeserializeAsync<List<SavingsAccount>>(stream, options);
            return accounts != null ? new List<Account>(accounts) : new List<Account>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ПОМИЛКА АСИНХРОННОГО ЗЧИТАННЯ JSON] {ex.Message}");
            return new List<Account>();
        }
    }

    public async Task<bool> SaveAccountsAsync(IEnumerable<Account> accounts)
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            using FileStream stream = File.Create(_filePath);

            // Серіалізуємо як конкретний тип SavingsAccount, щоб збереглися всі поля
            var savingsList = accounts.OfType<SavingsAccount>().ToList();
            await JsonSerializer.SerializeAsync(stream, savingsList, options);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ПОМИЛКА АСИНХРОННОГО ЗБЕРЕЖЕННЯ JSON] {ex.Message}");
            return false;
        }
    }
}