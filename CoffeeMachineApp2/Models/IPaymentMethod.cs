namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Способ оплаты. CoffeeMachine работает с этим интерфейсом и не знает,
    /// платит клиент наличными или картой - это и есть полиморфизм.
    /// </summary>
    public interface IPaymentMethod
    {
        string Name { get; }

        /// <summary>Хватит ли средств, чтобы оплатить.</summary>
        bool CanPay(IPayable item);

        /// <summary>Оплатить. Возвращает сдачу (для карты всегда 0).</summary>
        decimal Pay(IPayable item);

        /// <summary>Вернуть то, что клиент успел внести (для карты ничего).</summary>
        decimal Refund();
    }
}
