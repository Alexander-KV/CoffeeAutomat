using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms.Modules
{
    /// <summary>Терминал оплаты: отображает модель и передаёт действия покупателя владельцу автомата.</summary>
    public partial class PaymentControl : UserControl
    {
        public event EventHandler<bool> PaymentMethodSelected;
        public event EventHandler<int> CashInserted;
        public event EventHandler CardClicked;
        public event EventHandler PayRequested;
        public event EventHandler CardAnimationFinished;

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsCardAnimating => _cardSlot.IsAnimating;

        public PaymentControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            UiStyle.Round(this, 17);
            UiStyle.Round(_display, 8);
        }

        public void ShowState(CoffeeMachine machine, bool administrator)
        {
            bool customer = !administrator;
            bool paying = customer && machine.State == MachineState.Paying;
            bool ready = customer && machine.State == MachineState.Ready;
            var order = machine.Order;
            _cash.Enabled = _card.Enabled = paying;
            _cash.Selected = !machine.UseCard;
            _card.Selected = machine.UseCard;
            _note10.Enabled = _note50.Enabled = _note100.Enabled = _note500.Enabled = paying && !machine.UseCard;
            _cardSlot.Inserted = machine.Card.IsInserted;
            _cardSlot.Enabled = customer && !_cardSlot.IsAnimating && (paying || machine.Card.IsInserted);
            _pay.Enabled = paying && (machine.UseCard ? machine.Card.IsInserted && !_cardSlot.IsAnimating : machine.Cash.Inserted >= order.Total);
            _terminalTotal.Text = order == null ? "— ₽" : $"{order.Total:0.##} ₽";
            _terminalTotal.Font = order?.Total >= 1000 ? UiStyle.CompactPrice : UiStyle.Title;
            _wallet.Text = paying ? (machine.UseCard ? (machine.Card.IsInserted ? "Карта принята\nПодтвердите оплату" : "Вставьте карту\nв слот выше") : $"Внесено {machine.Cash.Inserted:0.##} ₽\nЕщё {Math.Max(0, order.Total - machine.Cash.Inserted):0.##} ₽") : machine.Card.IsInserted ? "ОПЛАЧЕНО\nЗаберите карту" : ready ? "ОПЛАЧЕНО\nПриятного дня" : machine.State == MachineState.Brewing ? "ОПЛАЧЕНО\nГотовим ваш кофе" : administrator ? "ОБСЛУЖИВАНИЕ" : "Выберите кофе\nна экране слева";
        }

        private void CashClick(object sender, EventArgs e) => PaymentMethodSelected?.Invoke(this, false);
        private void CardClick(object sender, EventArgs e) => PaymentMethodSelected?.Invoke(this, true);
        private void Note10Click(object sender, EventArgs e) => CashInserted?.Invoke(this, 10);
        private void Note50Click(object sender, EventArgs e) => CashInserted?.Invoke(this, 50);
        private void Note100Click(object sender, EventArgs e) => CashInserted?.Invoke(this, 100);
        private void Note500Click(object sender, EventArgs e) => CashInserted?.Invoke(this, 500);
        private void CardSlotClick(object sender, EventArgs e) => CardClicked?.Invoke(this, EventArgs.Empty);
        private void PayClick(object sender, EventArgs e) => PayRequested?.Invoke(this, EventArgs.Empty);
        private void CardSlotAnimationFinished(object sender, EventArgs e) => CardAnimationFinished?.Invoke(this, EventArgs.Empty);

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save();
            g.ScaleTransform(Width / 152f, Height / 640f);
            UiStyle.FillRound(g, new Rectangle(23, 565, 106, 12), Color.FromArgb(14, 22, 17), 5);
            using var slot = new Pen(Color.FromArgb(104, 117, 94));
            g.DrawLine(slot, 32, 571, 120, 571);
            g.Restore(saved);
        }
    }
}
