using System;

namespace CoffeeMachineApp.Models
{
    /// <summary>Банковская карта клиента (учебная имитация): номер и баланс.</summary>
    public class BankCard
    {
        private readonly string _number;
        private decimal _balance;

        public BankCard(string number, string holder, decimal balance)
        {
            if (string.IsNullOrWhiteSpace(number) || number.Length < 4)
                throw new ArgumentException("Некорректный номер карты");
            if (balance < 0)
                throw new ArgumentException("Баланс не может быть отрицательным");

            _number = number;
            Holder = holder;
            _balance = balance;
        }

        public string Holder { get; }
        public decimal Balance => _balance;

        /// <summary>Номер для показа: полный номер наружу не отдаём.</summary>
        public string MaskedNumber => "•••• •••• •••• " + _number.Substring(_number.Length - 4);

        public bool CanWithdraw(decimal amount) => amount <= _balance;

        public void Withdraw(decimal amount)
        {
            if (!CanWithdraw(amount))
                throw new InvalidOperationException("Недостаточно средств на карте");
            _balance -= amount;
        }
    }
}
