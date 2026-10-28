using System;

namespace BankManagement.Models;

// Структура для збереження суми та валюти, щоб не плутатися з числами
public struct Money
{
    // Кількість грошей
    public decimal Amount { get; private set; }
    
    // Валюта (за замовчуванням гривня)
    public string Currency { get; private set; }
    
    // Дата створення об'єкта
    public DateTime FormattedDate { get; private set; }
    
    // Перевірка, чи сума дорівнює нулю
    public bool IsZero => Amount == 0;
    
    // Красивий вивід суми з валютою
    public string DisplayValue => $"{Amount:F2} {Currency}";

    // Конструктор: задаємо суму та перевіряємо, щоб вона не була від'ємною
    public Money(decimal amount, string currency = "UAH")
    {
        if (amount < 0)
            throw new ArgumentException("Сума грошей не може бути від'ємною.");

        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "UAH" : currency;
        FormattedDate = DateTime.Now;
    }

    public override string ToString() => DisplayValue;
}