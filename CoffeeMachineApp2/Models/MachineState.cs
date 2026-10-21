namespace CoffeeMachineApp.Models
{
    /// <summary>Состояния кофейного автомата.</summary>
    public enum MachineState
    {
        Waiting,      // Ожидание: напиток не выбран
        Configuring,  // Настройка заказа: выбран напиток, настраиваем сахар и добавки
        Payment,      // Оплата
        Preparing,    // Приготовление
        Ready         // Заказ готов
    }
}
