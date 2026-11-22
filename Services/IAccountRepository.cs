using System.Collections.Generic;
using System.Threading.Tasks;
using BankManagementWpf.Models;

namespace BankManagementWpf.Services;

// Асинхронний інтерфейс для роботи зі сховищем рахунків (DIP)
public interface IAccountRepository
{
    Task<List<Account>> LoadAccountsAsync();
    Task<bool> SaveAccountsAsync(IEnumerable<Account> accounts);
}