using System.Collections.Generic;

namespace BankManagementWpf.Models;

// Інтерфейс для рахунків, які підтримують грошові операції
public interface ITransactionable : INotifiable
{
    bool Deposit(decimal amount, string description = "Поповнення рахунку");
    bool Withdraw(decimal amount, string description = "Зняття коштів");
    IReadOnlyList<Transaction> GetTransactionHistory();
}