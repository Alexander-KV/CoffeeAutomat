using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Одна позиция заказа: напиток + сахар + добавки + количество.
    /// Сама считает свою стоимость - в форме ничего вручную складывать не нужно.
    /// </summary>
    public class OrderItem : IPayable
    {
        public const int MaxQuantity = 10;

        private readonly List<Additive> _additives;
        private int _quantity;

        public OrderItem(Beverage beverage, Sugar sugar, IEnumerable<Additive> additives, int quantity = 1)
        {
            if (beverage == null)
                throw new ArgumentException("Не выбран напиток");

            Beverage = beverage;
            Sugar = new Sugar(sugar == null ? 0 : sugar.Spoons);   // своя копия, чтобы форма не меняла её "снаружи"
            _additives = additives == null ? new List<Additive>() : new List<Additive>(additives);
            Quantity = quantity;
        }

        public Beverage Beverage { get; }
        public Sugar Sugar { get; }
        public IReadOnlyList<Additive> Additives => _additives;

        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 1)
                    throw new ArgumentException("Количество должно быть не меньше 1");
                if (value > MaxQuantity)
                    throw new ArgumentException("Не больше " + MaxQuantity + " порций одной позиции");
                _quantity = value;
            }
        }

        /// <summary>Цена одной порции: напиток + сахар + добавки.</summary>
        public decimal UnitPrice
        {
            get
            {
                decimal total = Beverage.GetPrice() + Sugar.GetPrice();
                foreach (Additive additive in _additives)
                    total += additive.GetPrice();
                return total;
            }
        }

        /// <summary>Цена позиции с учётом количества.</summary>
        public decimal GetPrice() => UnitPrice * Quantity;

        /// <summary>Одинаковые ли настройки (тогда позиции можно объединить).</summary>
        public bool HasSameSettings(OrderItem other)
        {
            if (other.Beverage != Beverage) return false;
            if (other.Sugar.Spoons != Sugar.Spoons) return false;
            if (other._additives.Count != _additives.Count) return false;
            return _additives.All(a => other._additives.Contains(a));
        }

        /// <summary>Строка с настройками: "Сахар: 2 · Ванильный сироп".</summary>
        public string GetSettingsText()
        {
            string text = "Сахар: " + Sugar.Spoons;
            if (_additives.Count > 0)
                text += " · " + string.Join(", ", _additives.Select(a => a.Name));
            return text;
        }

        public override string ToString() =>
            Beverage.Name + " " + Beverage.VolumeMl + " мл ×" + Quantity + " = " + GetPrice().ToString("0") + " ₽";
    }
}
