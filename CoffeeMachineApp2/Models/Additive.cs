using System;

namespace CoffeeMachineApp.Models
{
    /// <summary>Дополнительная добавка (молоко, сироп) с ценой.</summary>
    public class Additive : IPayable
    {
        public Additive(string name, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название добавки не может быть пустым");
            if (price < 0)
                throw new ArgumentException("Цена добавки не может быть отрицательной");

            Name = name;
            Price = price;
        }

        public string Name { get; }
        public decimal Price { get; }

        public decimal GetPrice() => Price;

        public override string ToString() => Name + " +" + Price.ToString("0") + " ₽";
    }
}
