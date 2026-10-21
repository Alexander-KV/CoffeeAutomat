using System.Collections.Generic;
using System.Linq;

namespace CoffeeMachineApp.Models
{
    /// <summary>Текущий заказ: список позиций (композиция).</summary>
    public class CoffeeOrder : IPayable
    {
        private readonly List<OrderItem> _items = new List<OrderItem>();

        public IReadOnlyList<OrderItem> Items => _items;
        public bool IsEmpty => _items.Count == 0;

        public void Add(OrderItem item)
        {
            // одинаковые позиции объединяем: количество увеличивается
            OrderItem existing = _items.FirstOrDefault(i => i.HasSameSettings(item));
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                _items.Add(item);
        }

        public void Remove(OrderItem item) => _items.Remove(item);

        public void Clear() => _items.Clear();

        /// <summary>Итого = сумма всех позиций.</summary>
        public decimal GetPrice() => _items.Sum(i => i.GetPrice());
    }
}
