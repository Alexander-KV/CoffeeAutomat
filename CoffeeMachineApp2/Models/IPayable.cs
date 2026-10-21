namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Всё, за что можно заплатить: напиток, сахар, добавка, позиция заказа, весь заказ.
    /// Класс Payment работает именно с этим интерфейсом и не знает, что именно оплачивается.
    /// </summary>
    public interface IPayable
    {
        decimal GetPrice();
    }
}
