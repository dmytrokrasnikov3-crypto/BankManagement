using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using BankManagementWpf.Models;
using BankManagementWpf.Services;

namespace BankManagementWpf;

public partial class MainWindow : Window
{
    private readonly Bank _bank;
    private readonly IAccountRepository _repository;

    public MainWindow()
    {
        InitializeComponent();

        _repository = new JsonAccountRepository();
        _bank = new Bank("Universal Bank", "UNIVUA2X");

        this.Title = "Bank System v2.0 — Система Управління Рахунками";

        // Підписуємось на подію завантаження вікна для асинхронного зчитування JSON
        Loaded += MainWindow_Loaded;
    }

    // --- 2. АСИНХРОННЕ ЗАВАНТАЖЕННЯ ДАНИХ ---
    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var savedAccounts = await _repository.LoadAccountsAsync();

        if (savedAccounts.Count == 0)
        {
            var acc1 = new SavingsAccount("UA111222333", "Дмитро Красніков", 1000m, 5.0m, 200m);
            var acc2 = new SavingsAccount("UA444555666", "Леола Бершанська", 500m, 3.5m, 100m);

            SubscribeToAccountEvents(acc1);
            SubscribeToAccountEvents(acc2);

            _bank.OpenAccount(acc1);
            _bank.OpenAccount(acc2);
            await _repository.SaveAccountsAsync(_bank.Accounts);
        }
        else
        {
            foreach (var acc in savedAccounts)
            {
                SubscribeToAccountEvents(acc);
                _bank.OpenAccount(acc);
            }
        }

        RefreshAccountsList();
    }

    // Підписка на події рахунку через лямбда-вираз
    private void SubscribeToAccountEvents(Account account)
    {
        account.NotificationOccurred += (sender, message) =>
            MessageBox.Show(message, "Сповіщення рахунку", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void RefreshAccountsList(IEnumerable<Account>? filteredList = null)
    {
        var listToShow = filteredList?.ToList() ?? _bank.Accounts;

        AccountsListBox.ItemsSource = null;
        AccountsListBox.ItemsSource = listToShow;

        TotalCapitalText.Text = $" | Капітал банку: {_bank.TotalBankCapital:F2} UAH";

        if (listToShow.Count > 0)
            AccountsListBox.SelectedIndex = 0;
    }

    // --- 1. ПОШУК ТА ФІЛЬТРАЦІЯ (LINQ + LAMBDA) ---
    private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        string query = SearchTextBox.Text.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(query))
        {
            RefreshAccountsList();
        }
        else
        {
            var filtered = _bank.Accounts
                .Where(a => a.OwnerName.ToLower().Contains(query) || a.AccountNumber.ToLower().Contains(query));

            RefreshAccountsList(filtered);
        }
    }

    // --- 5. ВАЛІДАЦІЯ ВВОДУ СУМИ (Блокування кнопок при некоректному вводі) ---
    private void AmountTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        bool isValidAmount = decimal.TryParse(AmountTextBox.Text, out decimal amount) && amount > 0;

        DepositButton.IsEnabled = isValidAmount;
        WithdrawButton.IsEnabled = isValidAmount;
        TransferButton.IsEnabled = isValidAmount;

        if (string.IsNullOrWhiteSpace(AmountTextBox.Text))
        {
            ValidationHintText.Text = "Введіть додатне число для активації кнопок";
            ValidationHintText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E67E22"));
        }
        else if (!isValidAmount)
        {
            ValidationHintText.Text = "❌ Некоректна сума! Введіть число більше 0";
            ValidationHintText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
        }
        else
        {
            ValidationHintText.Text = "✅ Сума коректна";
            ValidationHintText.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
        }
    }

    private void AccountsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateSelectedAccountDetails();
    }

    private void UpdateSelectedAccountDetails()
    {
        if (AccountsListBox.SelectedItem is Account selectedAccount)
        {
            AccountDetailsTitleText.Text = $"{selectedAccount.OwnerName} ({selectedAccount.AccountNumber})";
            BalanceText.Text = $"Баланс: {selectedAccount.Balance:F2} UAH";

            TransactionsListBox.ItemsSource = null;
            TransactionsListBox.ItemsSource = selectedAccount.GetTransactionHistory();
        }
        TotalCapitalText.Text = $" | Капітал банку: {_bank.TotalBankCapital:F2} UAH";
    }

    private async void DepositButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AccountsListBox.SelectedItem is Account selectedAccount && decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                string desc = string.IsNullOrWhiteSpace(TargetOrDescriptionTextBox.Text) ? "Поповнення через UI" : TargetOrDescriptionTextBox.Text;

                if (selectedAccount.Deposit(amount, desc))
                {
                    await _repository.SaveAccountsAsync(_bank.Accounts);
                    UpdateSelectedAccountDetails();
                    AmountTextBox.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void WithdrawButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AccountsListBox.SelectedItem is Account selectedAccount && decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                string desc = string.IsNullOrWhiteSpace(TargetOrDescriptionTextBox.Text) ? "Зняття через UI" : TargetOrDescriptionTextBox.Text;

                if (selectedAccount.Withdraw(amount, desc))
                {
                    await _repository.SaveAccountsAsync(_bank.Accounts);
                    UpdateSelectedAccountDetails();
                    AmountTextBox.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void TransferButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AccountsListBox.SelectedItem is Account sourceAccount && decimal.TryParse(AmountTextBox.Text, out decimal amount))
            {
                string targetAccNum = TargetOrDescriptionTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(targetAccNum))
                {
                    MessageBox.Show("Вкажіть номер рахунку отримувача!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (_bank.TransferFunds(sourceAccount.AccountNumber, targetAccNum, amount))
                {
                    await _repository.SaveAccountsAsync(_bank.Accounts);
                    UpdateSelectedAccountDetails();
                    AmountTextBox.Clear();
                    TargetOrDescriptionTextBox.Clear();
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка переказу: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- 3. СТВОРЕННЯ НОВОГО РАХУНКУ З UI ---
    private async void CreateAccountButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string owner = NewOwnerTextBox.Text.Trim();
            string accNumber = NewAccNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(accNumber))
            {
                MessageBox.Show("Заповніть ПІБ власника та номер рахунку!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(NewInitialBalanceTextBox.Text, out decimal initialBalance) || initialBalance < 0)
            {
                MessageBox.Show("Стартовий баланс має бути невід'ємним числом!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newAccount = new SavingsAccount(accNumber, owner, initialBalance, 4.5m, 100m);
            SubscribeToAccountEvents(newAccount);

            if (_bank.OpenAccount(newAccount))
            {
                await _repository.SaveAccountsAsync(_bank.Accounts);
                RefreshAccountsList();
                AccountsListBox.SelectedItem = newAccount;

                NewOwnerTextBox.Clear();
                NewAccNumberTextBox.Clear();
                NewInitialBalanceTextBox.Text = "500";
                MessageBox.Show($"Рахунок {accNumber} для {owner} успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Не вдалося створити рахунок. Можливо, такий номер вже існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка створення рахунку: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- 4. ЕКСПОРТ ІСТОРІЇ ТРАНЗАКЦІЙ У CSV / TXT ---
    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AccountsListBox.SelectedItem is not Account selectedAccount)
            {
                MessageBox.Show("Спочатку оберіть рахунок зі списку!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var dialog = new SaveFileDialog
            {
                FileName = $"Statement_{selectedAccount.AccountNumber}_{DateTime.Now:yyyyMMdd}",
                DefaultExt = ".csv",
                Filter = "CSV таблиця (*.csv)|*.csv|Текстовий файл (*.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                var lines = new List<string>
                {
                    $"Виписка за рахунком: {selectedAccount.AccountNumber} | Власник: {selectedAccount.OwnerName}",
                    $"Поточний баланс: {selectedAccount.Balance:F2} UAH",
                    "Дата та час;Тип операції;Сума;Опис;Статус"
                };

                foreach (var t in selectedAccount.GetTransactionHistory())
                {
                    lines.Add($"{t.Timestamp:yyyy-MM-dd HH:mm};{t.Type};{t.Amount};{t.Description};{(t.IsSuccessful ? "Успішно" : "Помилка")}");
                }

                await File.WriteAllLinesAsync(dialog.FileName, lines, Encoding.UTF8);
                MessageBox.Show("Виписку успішно збережено!", "Експорт завершено", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка експорту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // --- 5. ПЕРЕМИКАННЯ СВІТЛОЇ / ТЕМНОЇ ТЕМИ ---
    private void ThemeCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        bool isDark = DarkThemeCheckBox.IsChecked == true;

        var windowBg = (Color)ColorConverter.ConvertFromString(isDark ? "#1E1E2E" : "#F4F6F9");
        var cardBg = (Color)ColorConverter.ConvertFromString(isDark ? "#2A2A3C" : "#FFFFFF");
        var textColor = (Color)ColorConverter.ConvertFromString(isDark ? "#ECF0F1" : "#2C3E50");
        var subTextColor = (Color)ColorConverter.ConvertFromString(isDark ? "#BDC3C7" : "#34495E");

        RootWindow.Background = new SolidColorBrush(windowBg);
        LeftCardBorder.Background = new SolidColorBrush(cardBg);
        RightCardBorder.Background = new SolidColorBrush(cardBg);

        var textBrush = new SolidColorBrush(textColor);
        var subTextBrush = new SolidColorBrush(subTextColor);

        HeaderTitleText.Foreground = textBrush;
        DarkThemeCheckBox.Foreground = textBrush;
        AccountsLabel.Foreground = textBrush;
        OperationsLabel.Foreground = textBrush;
        NewAccountLabel.Foreground = textBrush;
        AccountDetailsTitleText.Foreground = textBrush;

        SearchLabel.Foreground = subTextBrush;
        AmountLabel.Foreground = subTextBrush;
        DescLabel.Foreground = subTextBrush;
        NewOwnerLabel.Foreground = subTextBrush;
        NewNumberLabel.Foreground = subTextBrush;
        NewBalanceLabel.Foreground = subTextBrush;
        HistoryLabel.Foreground = subTextBrush;
    }
}