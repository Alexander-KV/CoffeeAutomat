using System;
using System.Linq;

namespace CoffeeMachineApp.Models
{
    public enum MachineState { Choosing, Paying, Brewing, Ready }

    /// <summary>Один независимый автомат. Никаких форм, файлов и общих static-заказов.</summary>
    public sealed class CoffeeMachine
    {
        public MachineConfiguration Configuration { get; private set; }
        public MachineState State { get; private set; } = MachineState.Choosing;
        public CoffeeOrder Order { get; private set; }
        public CashPayment Cash { get; } = new CashPayment();
        public CardPayment Card { get; } = new CardPayment();
        public bool UseCard { get; private set; }
        public decimal Change { get; private set; }
        public CoffeeMachine(MachineConfiguration configuration)
        {
            configuration.Validate();
            Configuration = configuration.Snapshot();
        }
        public void UpdateConfiguration(MachineConfiguration configuration)
        {
            Require(MachineState.Choosing);
            if (Order != null) throw new InvalidOperationException("Сначала отмените текущий выбор.");
            configuration.Validate();
            if (configuration.Id != Configuration.Id) throw new ArgumentException("Нельзя заменить ID автомата.");
            Configuration = configuration.Snapshot();
        }
        public void Select(string beverageId)
        {
            Require(MachineState.Choosing);
            var drink = Configuration.Menu.FirstOrDefault(x => x.Id == beverageId && x.IsEnabled);
            if (drink == null) throw new InvalidOperationException("Напиток недоступен.");
            Order = new CoffeeOrder(drink);
        }
        public void Configure(int sugar, bool extraMilk, string syrup)
        {
            Require(MachineState.Choosing);
            if (Order == null) throw new InvalidOperationException("Сначала выберите напиток.");
            var next = Order with { Sugar = sugar, ExtraMilk = extraMilk, Syrup = syrup };
            next.Validate();
            Order = next;
        }
        public void BeginPayment()
        {
            Require(MachineState.Choosing);
            if (Order == null) throw new InvalidOperationException("Сначала выберите напиток.");
            Order.Validate();
            State = MachineState.Paying;
        }
        public decimal SelectPayment(bool card)
        {
            Require(MachineState.Paying);
            decimal refund = card && !UseCard ? Cash.Refund() : 0;
            UseCard = card;
            return refund;
        }
        public decimal BackToSelection()
        {
            Require(MachineState.Paying);
            decimal refund = Cash.Refund();
            UseCard = false;
            State = MachineState.Choosing;
            return refund;
        }
        public void InsertMoney(int amount)
        {
            Require(MachineState.Paying);
            if (UseCard) throw new InvalidOperationException("Выбрана карта.");
            Cash.Insert(amount);
        }
        public void Pay()
        {
            Require(MachineState.Paying);
            IPaymentMethod payment = UseCard ? (IPaymentMethod)Card : Cash;
            Change = payment.Pay(Order.Total);
            State = MachineState.Brewing;
        }
        public decimal Cancel()
        {
            if (State != MachineState.Choosing && State != MachineState.Paying)
                throw new InvalidOperationException("Оплаченный заказ отменить нельзя.");
            decimal refund = Cash.Refund();
            Reset();
            return refund;
        }
        public void FinishPreparing() { Require(MachineState.Brewing); State = MachineState.Ready; }
        public decimal Collect()
        {
            Require(MachineState.Ready);
            decimal change = Change;
            Reset();
            return change;
        }
        private void Reset() { Order = null; Change = 0; UseCard = false; State = MachineState.Choosing; }
        private void Require(MachineState expected)
        {
            if (State != expected) throw new InvalidOperationException("Действие недоступно на текущем этапе.");
        }
    }
}
