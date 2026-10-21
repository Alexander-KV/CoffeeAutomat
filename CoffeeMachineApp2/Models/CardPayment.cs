namespace CoffeeMachineApp.Models
{
    /// <summary>Оплата банковской картой: сдачи нет, деньги списываются с карты.</summary>
    public class CardPayment : IPaymentMethod
    {
        public CardPayment(BankCard card)
        {
            Card = card;
        }

        public BankCard Card { get; }

        public string Name => "Карта " + Card.MaskedNumber.Substring(Card.MaskedNumber.Length - 4);

        public bool CanPay(IPayable item) => Card.CanWithdraw(item.GetPrice());

        public decimal Pay(IPayable item)
        {
            Card.Withdraw(item.GetPrice());   // бросит исключение, если денег не хватает
            return 0m;
        }

        public decimal Refund() => 0m;
    }
}
