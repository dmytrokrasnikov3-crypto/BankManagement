using System;

namespace BankManagementWpf.Models;

// Значимий тип (struct) для збереження суми
public struct Money
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public DateTime FormattedDate { get; private set; }
    public bool IsZero => Amount == 0;
    public string DisplayValue => $"{Amount:F2} {Currency}";

    public Money(decimal amount, string currency = "UAH")
    {
        if (amount < 0)
            throw new ArgumentException("Сума грошей не може бути від'ємною.");

        Amount = amount;
        Currency = string.IsNullOrWhiteSpace(currency) ? "UAH" : currency;
        FormattedDate = DateTime.Now;
    }

    // --- ПЕРЕВАНТАЖЕННЯ ОПЕРАТОРІВ ---
    public static Money operator +(Money left, Money right)
    {
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;
    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public override string ToString() => DisplayValue;
}