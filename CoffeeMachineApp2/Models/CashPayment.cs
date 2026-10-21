using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeMachineApp.Models
{
    /// <summary>Оплата наличными: внесение купюр, сдача, возврат денег.</summary>
    public class CashPayment : IPaymentMethod
    {
        private decimal _inserted;

        /// <summary>Купюры, которые принимает автомат.</summary>
        public static IReadOnlyList<int> Banknotes { get; } = new[] { 10, 50, 100, 500 };

        /// <summary>Номиналы, которыми автомат выдаёт сдачу.</summary>
        private static readonly int[] ChangeDenominations = { 500, 100, 50, 10, 5, 2, 1 };

        public string Name => "Наличные";

        public decimal Inserted => _inserted;

        public void Insert(int amount)
        {
            if (!Banknotes.Contains(amount))
                throw new ArgumentException("Автомат не принимает такую купюру");
            _inserted += amount;
        }

        public bool CanPay(IPayable item) => _inserted >= item.GetPrice();

        public decimal Missing(IPayable item) => Math.Max(0m, item.GetPrice() - _inserted);

        public decimal Change(IPayable item) => Math.Max(0m, _inserted - item.GetPrice());

        public decimal Pay(IPayable item)
        {
            if (!CanPay(item))
                throw new InvalidOperationException("Недостаточно средств. Не хватает: " + Missing(item).ToString("0") + " ₽");

            decimal change = Change(item);
            _inserted = 0m;
            return change;
        }

        public decimal Refund()
        {
            decimal refund = _inserted;
            _inserted = 0m;
            return refund;
        }

        /// <summary>Разложить сдачу на монеты и купюры (жадный алгоритм): 90 -> 50 + 10 + 10 + 10 + 10.</summary>
        public static IReadOnlyList<int> SplitChange(decimal amount)
        {
            var result = new List<int>();
            int rest = (int)Math.Round(amount);
            foreach (int denomination in ChangeDenominations)
            {
                while (rest >= denomination)
                {
                    result.Add(denomination);
                    rest -= denomination;
                }
            }
            return result;
        }
    }
}
