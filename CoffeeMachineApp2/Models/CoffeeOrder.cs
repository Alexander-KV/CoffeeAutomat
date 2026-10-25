using System;

namespace CoffeeMachineApp.Models
{
    public sealed record CoffeeOrder(Beverage Beverage, int Sugar = 0, bool ExtraMilk = false, string Syrup = "Без сиропа")
    {
        public decimal Total => Beverage.Price + (Sugar > 2 ? 10 : 0) + (ExtraMilk ? 30 : 0) + (Syrup != "Без сиропа" ? 30 : 0);
        public void Validate()
        {
            if (Beverage == null) throw new ArgumentException("Выберите напиток.");
            Beverage.Validate();
            if (Sugar < 0 || Sugar > 4) throw new ArgumentException("Сахар: от 0 до 4 ложек.");
            if (Syrup != "Без сиропа" && Syrup != "Ваниль" && Syrup != "Карамель")
                throw new ArgumentException("Неизвестный сироп.");
        }
    }
}
