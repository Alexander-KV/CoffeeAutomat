using System;

namespace CoffeeMachineApp.Models
{
    // Полиморфизм: автомату известен контракт, а не алгоритм списания денег.
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
            decimal change = Inserted - total;
            Inserted = 0;
            return change;
        }
        public decimal Refund() { decimal amount = Inserted; Inserted = 0; return amount; }
    }

    // Учебный терминал: баланс одного клиента на этом автомате, без реального эквайринга.
    public sealed class CardPayment : IPaymentMethod
    {
        public string Name => "Демо-карта";
        public decimal Balance { get; private set; } = 2000;
        public decimal Pay(decimal total)
        {
            if (total <= 0) throw new ArgumentOutOfRangeException(nameof(total));
            if (total > Balance) throw new InvalidOperationException("Недостаточно средств на демо-карте. Выберите наличные.");
            Balance -= total;
            return 0;
        }
    }
}
