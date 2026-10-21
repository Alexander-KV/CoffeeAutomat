using System;

namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Абстрактный товар автомата. Хранит название и цену.
    /// Поля private (инкапсуляция), доступ - через свойства.
    /// </summary>
    public abstract class Product : IPayable
    {
        private readonly string _name;
        private decimal _price;

        protected Product(string name, decimal price)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Название не может быть пустым");

            _name = name;
            Price = price; // проверка выполняется в set
        }

        public string Name => _name;

        public decimal Price
        {
            get => _price;
            set
            {
                if (value < 0)
                    throw new ArgumentException("Цена не может быть отрицательной");
                _price = value;
            }
        }

        // virtual: наследники могут изменить способ расчёта цены
        public virtual decimal GetPrice() => _price;

        public override string ToString() => Name;
    }
}
