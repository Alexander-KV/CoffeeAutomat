using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Кофейный автомат: меню, добавки, текущий заказ, оплата и состояние.
    /// Здесь находятся ВСЕ правила работы. Модули интерфейса только вызывают методы
    /// и показывают результат (или сообщение из исключения).
    /// </summary>
    public class CoffeeMachine
    {
        private readonly List<Beverage> _menu = new List<Beverage>();
        private readonly List<Additive> _additives = new List<Additive>();
        private readonly List<Additive> _draftAdditives = new List<Additive>();   // добавки напитка, который настраивается

        public CoffeeMachine()
        {
            FillMenu();
            FillAdditives();
            DraftSugar = new Sugar();
            Card = new CardPayment(new BankCard("4242424242424242", "COFFEE LOVER", 2000m));
        }

        public IReadOnlyList<Beverage> Menu => _menu;
        public IReadOnlyList<Additive> Additives => _additives;

        // Композиция: автомат содержит заказ и способы оплаты
        public CoffeeOrder Order { get; } = new CoffeeOrder();
        public CashPayment Cash { get; } = new CashPayment();
        public CardPayment Card { get; }

        public MachineState State { get; private set; } = MachineState.Waiting;
        public Beverage SelectedBeverage { get; private set; }

        // Черновик: настройки напитка, который выбран, но ещё не добавлен в заказ
        public Sugar DraftSugar { get; private set; }
        public IReadOnlyList<Additive> DraftAdditives => _draftAdditives;

        public PaymentKind PaymentKind { get; private set; } = PaymentKind.Cash;

        /// <summary>Текущий способ оплаты. Дальше с ним работаем только через интерфейс.</summary>
        public IPaymentMethod CurrentMethod
        {
            get
            {
                if (PaymentKind == PaymentKind.Card) return Card;
                return Cash;
            }
        }

        // Итоги последней оплаты
        public string PaidWith { get; private set; } = "";
        public decimal LastChange { get; private set; }
        public IReadOnlyList<int> ChangeCoins { get; private set; } = new List<int>();
        public bool ChangeTaken { get; private set; } = true;

        /// <summary>Можно ли сейчас выбирать напиток и менять его настройки.</summary>
        public bool CanEditOrder =>
            State == MachineState.Waiting ||
            State == MachineState.Configuring;

        // ---------- начальные данные ----------

        private void FillMenu()
        {
            _menu.Add(new Coffee("Эспрессо", 120, 60, "coffee1.png", "эспрессо", "#3A2212", false));
            _menu.Add(new Coffee("Американо", 140, 200, "coffee2.png", "эспрессо + горячая вода", "#4A2E1B", false));
            _menu.Add(new Coffee("Капучино", 180, 300, "coffee3.png", "эспрессо + молоко + молочная пена", "#B5875C", true));
            _menu.Add(new Coffee("Латте", 190, 300, "coffee4.png", "эспрессо + много молока", "#C9A27A", true));
            _menu.Add(new Coffee("Латте макиато", 200, 300, "coffee5.png", "молоко + эспрессо слоями", "#D9B98E", true));
            _menu.Add(new Coffee("Моккачино", 210, 300, "coffee6.png", "эспрессо + шоколад + молоко", "#7A4A2E", true));
            _menu.Add(new Coffee("Флэт уайт", 200, 250, "coffee7.png", "двойной эспрессо + бархатное молоко", "#B98558", true));
            _menu.Add(new Coffee("Раф", 220, 300, "coffee8.png", "эспрессо + сливки + ванильный сахар", "#E0C39A", true));
            _menu.Add(new HotChocolate(180, 250, "coffee9.png"));
        }

        private void FillAdditives()
        {
            _additives.Add(new Additive("Дополнительное молоко", 30));
            _additives.Add(new Additive("Ванильный сироп", 30));
            _additives.Add(new Additive("Карамельный сироп", 30));
        }

        // ---------- выбор и настройка напитка (в заказе всегда один напиток) ----------

        public void SelectBeverage(Beverage beverage)
        {
            EnsureOrderEditable();

            if (beverage == null)
                throw new InvalidOperationException("Сначала выберите напиток");
            if (!beverage.IsEnabled)
                throw new InvalidOperationException("Напиток временно недоступен");
            if (beverage == SelectedBeverage)
                return;

            SelectedBeverage = beverage;
            ResetDraft();
            State = MachineState.Configuring;
        }

        public void ChangeSugar(int delta)
        {
            EnsureConfiguring();
            if (delta > 0) DraftSugar.Increase();
            else DraftSugar.Decrease();
        }

        public void SetAdditive(Additive additive, bool selected)
        {
            EnsureConfiguring();
            bool has = _draftAdditives.Contains(additive);
            if (selected && !has) _draftAdditives.Add(additive);
            if (!selected && has) _draftAdditives.Remove(additive);
        }

        public bool IsAdditiveSelected(Additive additive) => _draftAdditives.Contains(additive);

        /// <summary>Позиция заказа, собранная из текущего черновика (для показа цены и оплаты).</summary>
        public OrderItem BuildDraftItem()
        {
            EnsureConfiguring();
            return new OrderItem(SelectedBeverage, DraftSugar, _draftAdditives);
        }

        /// <summary>Отказаться от выбранного напитка.</summary>
        public void CancelSelection()
        {
            EnsureOrderEditable();
            SelectedBeverage = null;
            ResetDraft();
            State = MachineState.Waiting;
        }

        // ---------- оплата ----------

        /// <summary>Перейти к оплате: выбранный напиток с настройками становится заказом.</summary>
        public void GoToPayment()
        {
            EnsureConfiguring();

            Order.Clear();
            Order.Add(BuildDraftItem());
            SelectedBeverage = null;
            ResetDraft();
            PaymentKind = PaymentKind.Cash;
            State = MachineState.Payment;
        }

        /// <summary>Вернуться к настройке напитка (внесённые наличные возвращаются).</summary>
        public void ReturnToOrder()
        {
            if (State != MachineState.Payment)
                return;

            Cash.Refund();

            OrderItem item = Order.Items[0];
            Order.Clear();
            SelectedBeverage = item.Beverage;
            DraftSugar = new Sugar(item.Sugar.Spoons);
            _draftAdditives.Clear();
            _draftAdditives.AddRange(item.Additives);
            State = MachineState.Configuring;
        }

        /// <summary>Выбрать способ оплаты. Возвращает сумму наличных, которые пришлось вернуть.</summary>
        public decimal SelectPaymentKind(PaymentKind kind)
        {
            if (State != MachineState.Payment)
                throw new InvalidOperationException("Сначала перейдите к оплате");
            if (kind == PaymentKind)
                return 0m;

            decimal refund = PaymentKind == PaymentKind.Cash ? Cash.Refund() : 0m;
            PaymentKind = kind;
            return refund;
        }

        public void InsertMoney(int amount)
        {
            if (State != MachineState.Payment)
                throw new InvalidOperationException("Сначала перейдите к оплате");
            if (PaymentKind != PaymentKind.Cash)
                throw new InvalidOperationException("Выбрана оплата картой");
            Cash.Insert(amount);
        }

        public void Pay()
        {
            if (State != MachineState.Payment)
                throw new InvalidOperationException("Сначала перейдите к оплате");
            if (Order.IsEmpty)
                throw new InvalidOperationException("Заказ пуст");

            IPaymentMethod method = CurrentMethod;   // наличные или карта - автомату всё равно
            decimal change = method.Pay(Order);      // метод сам бросит исключение, если денег не хватает

            PaidWith = method.Name;
            LastChange = change;
            ChangeCoins = change > 0 ? CashPayment.SplitChange(change) : new List<int>();
            ChangeTaken = change <= 0;
            State = MachineState.Preparing;
        }

        /// <summary>Отмена заказа: заказ очищается, наличные возвращаются. Возвращает сумму возврата.</summary>
        public decimal Cancel()
        {
            if (State == MachineState.Preparing || State == MachineState.Ready)
                throw new InvalidOperationException("Заказ уже оплачен, отменить его нельзя");

            SelectedBeverage = null;
            ResetDraft();
            Order.Clear();
            decimal refund = CurrentMethod.Refund();
            PaymentKind = PaymentKind.Cash;
            State = MachineState.Waiting;
            return refund;
        }

        // ---------- приготовление и выдача ----------

        /// <summary>Очередь напитков: позиция "Латте ×2" превращается в два напитка.</summary>
        public List<Beverage> GetPreparationQueue()
        {
            var queue = new List<Beverage>();
            foreach (OrderItem item in Order.Items)
                for (int i = 0; i < item.Quantity; i++)
                    queue.Add(item.Beverage);
            return queue;
        }

        public List<string> GetIssuedList()
        {
            return Order.Items
                .Select(i => i.Quantity > 1 ? i.Beverage.Name + " ×" + i.Quantity : i.Beverage.Name)
                .ToList();
        }

        public void FinishPreparing()
        {
            if (State == MachineState.Preparing)
                State = MachineState.Ready;
        }

        /// <summary>Клиент забирает сдачу из лотка. Возвращает её сумму.</summary>
        public decimal TakeChange()
        {
            if (State != MachineState.Ready)
                throw new InvalidOperationException("Сдача ещё не выдана");
            if (ChangeTaken)
                throw new InvalidOperationException("Сдачи нет или она уже получена");

            ChangeTaken = true;
            return LastChange;
        }

        public void StartNewOrder()
        {
            if (State != MachineState.Ready)
                throw new InvalidOperationException("Заказ ещё не готов");
            if (!ChangeTaken)
                throw new InvalidOperationException("Сначала заберите сдачу");

            Order.Clear();
            LastChange = 0m;
            ChangeCoins = new List<int>();
            PaidWith = "";
            SelectedBeverage = null;
            ResetDraft();
            PaymentKind = PaymentKind.Cash;
            State = MachineState.Waiting;
        }

        // ---------- вспомогательное ----------

        private void EnsureOrderEditable()
        {
            if (!CanEditOrder)
                throw new InvalidOperationException("Сейчас напиток изменить нельзя");
        }

        private void EnsureConfiguring()
        {
            if (State != MachineState.Configuring || SelectedBeverage == null)
                throw new InvalidOperationException("Сначала выберите напиток");
        }

        private void ResetDraft()
        {
            DraftSugar = new Sugar();
            _draftAdditives.Clear();
        }
    }
}
