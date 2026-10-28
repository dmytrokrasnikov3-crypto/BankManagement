namespace BankManagement.Models;

// Типи операцій, які можна робити з рахунком
public enum TransactionType
{
    Deposit,    // Поповнення
    Withdrawal, // Зняття
    Transfer    // Переказ
}