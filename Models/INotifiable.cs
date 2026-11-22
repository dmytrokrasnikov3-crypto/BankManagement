namespace BankManagementWpf.Models;

// Інтерфейс для сутностей, які можуть надсилати сповіщення через події
public interface INotifiable
{
    // Подія з використанням стандартного делегата EventHandler<T>
    event EventHandler<string>? NotificationOccurred;
}