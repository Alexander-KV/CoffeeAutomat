using System;
using System.Drawing;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Модуль 3: оплата. Две вкладки - наличные (купюры, сдача) и банковская карта
    /// (терминал с анимацией). Какой способ выбран, знает автомат, модуль только показывает.
    /// </summary>
    public class PaymentModule : MachineModule
    {
        private readonly Label _lblHint;
        private readonly TableLayoutPanel _content;
        private readonly Label _lblTotal;
        private readonly RoundedButton _btnCash;
        private readonly RoundedButton _btnCard;
        private readonly Panel _pageCash;
        private readonly Panel _pageCard;
        private readonly Label _lblInserted;
        private readonly Label _lblMissing;
        private readonly RoundedButton _btnPayCash;
        private readonly CardTerminalView _terminal;
        private readonly RoundedButton _btnTap;
        private readonly RoundedButton _btnBack;
        private readonly RoundedButton _btnCancel;
        private readonly Timer _timer = new Timer { Interval = 40 };
        private int _ticks;
        private bool _busy;   // идёт оплата картой: кнопки блокируются

        public PaymentModule()
        {
            Title = "3 · Оплата";

            _lblHint = UiStyle.CreateLabel("Оплата откроется\nпосле формирования заказа", UiStyle.Big, UiStyle.Gray, ContentAlignment.MiddleCenter);

            _lblTotal = UiStyle.CreateLabel("", UiStyle.Big, UiStyle.Brown, ContentAlignment.MiddleLeft);

            _btnCash = UiStyle.CreateButton("Наличные", false);
            _btnCard = UiStyle.CreateButton("Карта", false);
            TableLayoutPanel tabs = UiStyle.CreateColumns(0, 0);
            tabs.Controls.Add(_btnCash, 0, 0);
            tabs.Controls.Add(_btnCard, 1, 0);

            // --- страница "Наличные" ---
            _lblInserted = UiStyle.CreateLabel("", UiStyle.Regular, UiStyle.DarkBrown, ContentAlignment.MiddleLeft);
            _lblMissing = UiStyle.CreateLabel("", UiStyle.Big, UiStyle.Red, ContentAlignment.MiddleLeft);

            TableLayoutPanel notes = UiStyle.CreateColumns(0, 0, 0, 0);
            for (int i = 0; i < CashPayment.Banknotes.Count; i++)
            {
                int amount = CashPayment.Banknotes[i];
                var money = new MoneyButton { Amount = amount, Dock = DockStyle.Fill, Margin = new Padding(3) };
                money.Click += (s, e) => Run(() => Machine.InsertMoney(amount));
                notes.Controls.Add(money, i, 0);
            }

            _btnPayCash = UiStyle.CreateButton("Оплатить", true);
            TableLayoutPanel cash = UiStyle.CreateStack(24, 32, 58, 0, 46);
            cash.Controls.Add(_lblInserted, 0, 0);
            cash.Controls.Add(_lblMissing, 0, 1);
            cash.Controls.Add(notes, 0, 2);
            cash.Controls.Add(_btnPayCash, 0, 4);
            _pageCash = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _pageCash.Controls.Add(cash);

            // --- страница "Карта" ---
            _terminal = new CardTerminalView { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _btnTap = UiStyle.CreateButton("Приложить карту", true);
            TableLayoutPanel card = UiStyle.CreateStack(0, 46);
            card.Controls.Add(_terminal, 0, 0);
            card.Controls.Add(_btnTap, 0, 1);
            _pageCard = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _pageCard.Controls.Add(card);

            var pages = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            pages.Controls.Add(_pageCash);
            pages.Controls.Add(_pageCard);

            _btnBack = UiStyle.CreateButton("Назад", false);
            _btnCancel = UiStyle.CreateButton("Отменить заказ", false);
            TableLayoutPanel bottom = UiStyle.CreateColumns(90, 0);
            bottom.Controls.Add(_btnBack, 0, 0);
            bottom.Controls.Add(_btnCancel, 1, 0);

            _content = UiStyle.CreateStack(34, 40, 0, 44);
            _content.Controls.Add(_lblTotal, 0, 0);
            _content.Controls.Add(tabs, 0, 1);
            _content.Controls.Add(pages, 0, 2);
            _content.Controls.Add(bottom, 0, 3);

            Controls.Add(_content);
            Controls.Add(_lblHint);

            _btnCash.Click += (s, e) => SwitchTo(PaymentKind.Cash);
            _btnCard.Click += (s, e) => SwitchTo(PaymentKind.Card);
            _btnPayCash.Click += (s, e) => Run(() => Machine.Pay());
            _btnTap.Click += (s, e) => StartCardPayment();
            _btnBack.Click += (s, e) => GoBack();
            _btnCancel.Click += (s, e) => CancelOrder();
            _timer.Tick += OnTimerTick;
        }

        protected override bool IsActiveIn(MachineState state)
        {
            return state == MachineState.Payment;
        }

        // ---------- действия ----------

        private void SwitchTo(PaymentKind kind)
        {
            Run(() =>
            {
                decimal refund = Machine.SelectPaymentKind(kind);
                if (refund > 0)
                    Notify("Наличные возвращены: " + refund.ToString("0") + " ₽", false);
            });
        }

        private void GoBack()
        {
            Run(() =>
            {
                decimal inserted = Machine.Cash.Inserted;
                Machine.ReturnToOrder();
                if (inserted > 0)
                    Notify("Наличные возвращены: " + inserted.ToString("0") + " ₽", false);
            });
        }

        private void CancelOrder()
        {
            Run(() =>
            {
                decimal refund = Machine.Cancel();
                Notify(refund > 0 ? "Заказ отменён. Возвращено: " + refund.ToString("0") + " ₽" : "Заказ отменён", false);
            });
        }

        // ---------- оплата картой: связь с банком -> одобрено/отказ -> списание ----------

        private void StartCardPayment()
        {
            if (_busy)
                return;

            _busy = true;
            _ticks = 0;
            _terminal.Phase = CardPhase.Processing;
            SetControlsEnabled(false);
            if (!_timer.Enabled) _timer.Start();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (Machine == null)
                return;

            _ticks++;
            _terminal.Advance();

            switch (_terminal.Phase)
            {
                case CardPhase.Processing:
                    if (_ticks >= 35)   // ~1.4 с "связи с банком"
                    {
                        _ticks = 0;
                        if (Machine.CurrentMethod.CanPay(Machine.Order))
                        {
                            _terminal.Phase = CardPhase.Approved;
                        }
                        else
                        {
                            _terminal.Phase = CardPhase.Declined;
                            Notify("Недостаточно средств на карте", true);
                        }
                    }
                    break;

                case CardPhase.Approved:
                    if (_ticks >= 22)   // ~0.9 с показываем "Оплата одобрена"
                    {
                        _timer.Stop();
                        _busy = false;
                        _terminal.Phase = CardPhase.Idle;
                        Run(() => Machine.Pay());
                    }
                    break;

                case CardPhase.Declined:
                    if (_ticks >= 40)
                    {
                        _busy = false;
                        _terminal.Phase = CardPhase.Idle;
                        SetControlsEnabled(true);
                    }
                    break;
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            _btnCash.Enabled = enabled;
            _btnCard.Enabled = enabled;
            _btnBack.Enabled = enabled;
            _btnCancel.Enabled = enabled;
            _btnTap.Enabled = enabled;
        }

        // ---------- отображение ----------

        protected override void OnRefreshView()
        {
            bool paying = Machine.State == MachineState.Payment;
            _content.Visible = paying;
            _lblHint.Visible = !paying;

            if (!paying)
            {
                if (!_busy) _timer.Stop();
                return;
            }

            bool cash = Machine.PaymentKind == PaymentKind.Cash;
            CoffeeOrder order = Machine.Order;

            _lblTotal.Text = "К оплате: " + order.GetPrice().ToString("0") + " ₽";
            _btnCash.Selected = cash;
            _btnCard.Selected = !cash;
            _pageCash.Visible = cash;
            _pageCard.Visible = !cash;

            if (cash)
            {
                CashPayment payment = Machine.Cash;
                _lblInserted.Text = "Внесено: " + payment.Inserted.ToString("0") + " ₽";
                if (payment.CanPay(order))
                {
                    _lblMissing.Text = "Сдача: " + payment.Change(order).ToString("0") + " ₽";
                    _lblMissing.ForeColor = UiStyle.Green;
                }
                else
                {
                    _lblMissing.Text = "Не хватает: " + payment.Missing(order).ToString("0") + " ₽";
                    _lblMissing.ForeColor = UiStyle.Red;
                }
                if (!_busy) _timer.Stop();
            }
            else
            {
                BankCard bankCard = Machine.Card.Card;
                _terminal.CardNumber = bankCard.MaskedNumber;
                _terminal.Holder = bankCard.Holder;
                _terminal.Balance = bankCard.Balance;
                if (!_timer.Enabled) _timer.Start();   // анимация ожидания карты
            }
        }
    }
}
