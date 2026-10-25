using System;
using System.Collections.Generic;

namespace CoffeeMachineApp.Models
{
    public interface IPaymentMethod
    {
        string Name { get; }
        decimal Pay(decimal total);
    }
    public sealed class CashPayment : IPaymentMethod
    {
        public string Name => "Наличные";
        public decimal Inserted { get; private set; }
        public void Insert(int amount)
        {
            if (amount != 10 && amount != 50 && amount != 100 && amount != 500)
                throw new ArgumentException("Принимаются 10, 50, 100 и 500 ₽.");
            Inserted += amount;
        }
        public decimal Pay(decimal total)
        {
            if (total <= 0) throw new ArgumentOutOfRangeException(nameof(total));
            if (Inserted < total) throw new InvalidOperationException($"Не хватает {total - Inserted:0.##} ₽.");
            decimal change = Inserted - total; Inserted = 0; return change;
        }
        public decimal Refund() { decimal amount = Inserted; Inserted = 0; return amount; }
        public static IReadOnlyList<decimal> SplitChange(decimal amount)
        {
            if (amount < 0 || decimal.Round(amount, 2) != amount) throw new ArgumentException("Некорректная сумма сдачи.");
            var result = new List<decimal>();
            foreach (decimal value in new[] { 500m, 100m, 50m, 10m, 5m, 2m, 1m, .5m, .1m, .05m, .01m })
                while (amount >= value) { result.Add(value); amount -= value; }
            return result.AsReadOnly();
        }
    }
    /// <summary>Учебная карта с неограниченными средствами. Для оплаты её нужно вставить.</summary>
    public sealed class CardPayment : IPaymentMethod
    {
        public string Name => "Банковская карта";
        public bool IsInserted { get; private set; }
        public void Insert()
        {
            if (IsInserted) throw new InvalidOperationException("Карта уже в терминале.");
            IsInserted = true;
        }
        public void Eject()
        {
            if (!IsInserted) throw new InvalidOperationException("Карта не вставлена.");
            IsInserted = false;
        }
        public decimal Pay(decimal total)
        {
            if (total <= 0) throw new ArgumentOutOfRangeException(nameof(total));
            if (!IsInserted) throw new InvalidOperationException("Вставьте карту в терминал.");
            return 0;
        }
    }
}
