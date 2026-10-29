using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Переставляемые модули задаются в Designer; этот класс связывает их с моделью.</summary>
    [ToolboxItem(true)]
    public partial class CoffeeMachineControl : UserControl
    {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CoffeeMachine Machine { get; private set; }
        public event EventHandler DuplicateRequested;
        public event EventHandler RemoveRequested;
        private Action<MachineConfiguration> _save;
        private AdminAccess _access;
        private readonly Stopwatch _brewing = new Stopwatch();
        private bool _error;

        // Дизайнеру не нужны ни JSON, ни пароль, ни работающая модель.
        public CoffeeMachineControl()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            selectionModule.BeverageSelected += (s, id) => Act(() => { Machine.Select(id); Notify(Machine.Order.Beverage.Recipe); });
            selectionModule.SugarChanged += (s, delta) => Configure(delta);
            selectionModule.MilkToggled += (s, e) => Configure(milk: true);
            selectionModule.SyrupChanged += (s, e) => Configure(syrup: true);
            selectionModule.PaymentRequested += (s, e) => Act(() => { Machine.BeginPayment(); Notify("Выберите способ оплаты на терминале справа."); });
            selectionModule.CancelRequested += CancelOrBack;
            paymentModule.PaymentMethodSelected += (s, card) => SwitchPayment(card);
            paymentModule.CashInserted += (s, amount) => Act(() => Machine.InsertMoney(amount));
            paymentModule.CardClicked += CardClicked;
            paymentModule.CardAnimationFinished += (s, e) => RefreshView();
            paymentModule.PayRequested += (s, e) => Act(() =>
            {
                Machine.Pay(); _brewing.Restart(); brewingTimer.Start(); Notify("Заказ оплачен. Магия уже началась.");
            });
            dispensingModule.DrinkCollected += (s, e) => Act(() =>
            {
                Machine.CollectDrink(); brewingTimer.Stop();
                Notify(Machine.Change > 0 ? "Напиток получен. Заберите сдачу из лотка справа." : Machine.Card.IsInserted ? "Напиток получен. Заберите карту из терминала." : "Хороший кофе — хорошее начало. Приятного дня!");
            });
            dispensingModule.ChangeCollected += (s, e) => Act(() =>
            {
                decimal amount = Machine.CollectChange();
                Notify($"Сдача {amount:0.##} ₽ получена." + (Machine.Order != null && !Machine.DrinkCollected ? " Не забудьте напиток." : " Приятного дня!"));
            });
            selectionModule.LocationChanged += ModuleBoundsChanged;
            selectionModule.SizeChanged += ModuleBoundsChanged;
        }

        public CoffeeMachineControl(CoffeeMachine machine, Action<MachineConfiguration> save, AdminAccess access) : this()
        {
            Bind(machine, save, access);
        }

        public void Bind(CoffeeMachine machine, Action<MachineConfiguration> save, AdminAccess access)
        {
            ArgumentNullException.ThrowIfNull(machine);
            ArgumentNullException.ThrowIfNull(save);
            ArgumentNullException.ThrowIfNull(access);
            if (Machine != null) throw new InvalidOperationException("Этот элемент уже подключён к автомату.");
            Machine = machine; _save = save; _access = access;
            titleLabel.Text = machine.Configuration.Name;
            serialLabel.Text = "CRAFTED COFFEE\n№ " + machine.Configuration.Id.Substring(0, 6).ToUpperInvariant();
            selectionModule.SetMenu(machine.Configuration.Menu);
            RefreshAccess();
        }

        public void RefreshAccess()
        {
            if (Machine == null) return;
            adminButton.Visible = copyButton.Visible = removeButton.Visible = _access.IsAdmin;
            Notify(_access.IsAdmin ? "Режим администратора. Покупки приостановлены." : "Готов к работе. Выберите свой кофе.");
            RefreshView();
        }
        private void ModuleBoundsChanged(object sender, EventArgs e) => Invalidate();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save(); g.ScaleTransform(Width / 640f, Height / 1000f);
            UiStyle.FillRound(g, new Rectangle(9, 9, 622, 984), Color.FromArgb(198, 197, 183), 30);
            using (var body = UiStyle.RoundedRect(new Rectangle(3, 1, 629, 990), 28))
            using (var gradient = new LinearGradientBrush(new Rectangle(3, 1, 629, 990), Color.FromArgb(51, 58, 48), Color.FromArgb(25, 32, 27), 0f))
            using (var edge = new Pen(Color.FromArgb(92, 102, 82))) { g.FillPath(gradient, body); g.DrawPath(edge, body); }
            using (var accent = new Pen(UiStyle.Gold, 2)) g.DrawLine(accent, 32, 83, 606, 83);
            using (var led = new SolidBrush(Machine?.State == MachineState.Ready ? UiStyle.Green : UiStyle.Gold)) g.FillEllipse(led, 601, 74, 6, 6);
            g.Restore(saved);
            if (selectionModule != null)
            {
                int x = LogicalToDeviceUnits(4), top = LogicalToDeviceUnits(9), bottom = LogicalToDeviceUnits(6);
                Rectangle frame = selectionModule.Bounds;
                frame = new Rectangle(frame.X - x, frame.Y - top, frame.Width + 2 * x, frame.Height + top + bottom);
                UiStyle.FillRound(g, frame, Color.FromArgb(12, 20, 16), LogicalToDeviceUnits(23));
            }
        }
        private void Configure(int sugarDelta = 0, bool milk = false, bool syrup = false)
        {
            if (Machine?.Order == null) return;
            Act(() =>
            {
                var order = Machine.Order;
                string nextSyrup = syrup ? order.Syrup switch { "Без сиропа" => "Ваниль", "Ваниль" => "Карамель", _ => "Без сиропа" } : order.Syrup;
                Machine.Configure(Math.Clamp(order.Sugar + sugarDelta, 0, 4), milk ? !order.ExtraMilk : order.ExtraMilk, nextSyrup);
            });
        }
        private void CancelOrBack(object sender, EventArgs e) => Act(() =>
        {
            bool back = Machine.State == MachineState.Paying;
            decimal refund = back ? Machine.BackToSelection() : Machine.Cancel();
            Notify(refund > 0 ? $"Возвращено {refund:0.##} ₽. Можно изменить напиток." : back ? "Измените напиток или настройки по вкусу." : "Выберите что-нибудь новое.");
        });
        private void SwitchPayment(bool card) => Act(() =>
        {
            decimal refund = Machine.SelectPayment(card);
            Notify(refund > 0 ? $"Внесённые наличные возвращены: {refund:0.##} ₽." : card ? (Machine.Card.IsInserted ? "Карта принята. Подтвердите оплату на терминале." : "Нажмите на карту над терминалом, чтобы вставить её.") : "Внесите купюры на терминале справа.");
        });
        private void CardClicked(object sender, EventArgs e) => Act(() =>
        {
            if (Machine.Card.IsInserted) { Machine.EjectCard(); Notify("Карта возвращена. Спасибо!"); }
            else
            {
                decimal refund = Machine.SelectPayment(true); Machine.InsertCard();
                Notify(refund > 0 ? $"Карта вставлена. Внесённые наличные возвращены: {refund:0.##} ₽." : "Карта вставлена. Нажмите «Оплатить».");
            }
        });
        private void OpenAdmin(object sender, EventArgs e)
        {
            if (Machine == null || !_access.IsAdmin) return;
            using (var form = new AdminForm(Machine.Configuration, config => { _save(config); Machine.UpdateConfiguration(config); })) form.ShowDialog(this);
            titleLabel.Text = Machine.Configuration.Name;
            selectionModule.SetMenu(Machine.Configuration.Menu);
            RefreshView(); Notify("Настройки загружены. Автомат готов к работе.");
        }
        private void CopyMachine(object sender, EventArgs e)
        {
            if (Machine != null && _access.IsAdmin) DuplicateRequested?.Invoke(this, EventArgs.Empty);
        }
        private void RemoveMachine(object sender, EventArgs e)
        {
            if (Machine != null && _access.IsAdmin) RemoveRequested?.Invoke(this, EventArgs.Empty);
        }
        private void Tick(object sender, EventArgs e)
        {
            if (Machine == null) { brewingTimer.Stop(); return; }
            if (Machine.State == MachineState.Ready)
            {
                if (!Machine.DrinkCollected) dispensingModule.AdvanceAnimation(); else brewingTimer.Stop();
                return;
            }
            if (Machine.State != MachineState.Brewing) { brewingTimer.Stop(); return; }
            var drink = Machine.Order.Beverage;
            double progress = Math.Clamp(_brewing.Elapsed.TotalSeconds / drink.PrepareSeconds, 0, 1);
            string stage = progress switch
            {
                < 0.25 => drink.IsCoffee ? "Мелем свежие зёрна" : "Подогреваем молоко",
                < 0.55 => drink.IsCoffee ? "Завариваем эспрессо" : "Растапливаем шоколад",
                < 0.8 => drink.HasMilk || Machine.Order.ExtraMilk ? "Добавляем молоко и пену" : "Наполняем чашку",
                _ => "Добавляем последний штрих"
            };
            int remaining = Math.Max(0, (int)Math.Ceiling(drink.PrepareSeconds - _brewing.Elapsed.TotalSeconds));
            dispensingModule.ShowProgress(Machine.Order, progress, remaining, stage);
            dispensingModule.AdvanceAnimation();
            if (progress >= 1) { Machine.FinishPreparing(); Notify("Готово! Заберите горячий напиток в нише выдачи."); RefreshView(); }
        }
        private void RefreshView()
        {
            if (Machine == null) return;
            selectionModule.ShowState(Machine.Order, Machine.State, _access.IsAdmin);
            paymentModule.ShowState(Machine, _access.IsAdmin);
            dispensingModule.ShowState(Machine, _access.IsAdmin);
            adminButton.Enabled = copyButton.Enabled = removeButton.Enabled = _access.IsAdmin && Machine.Order == null;
            messageLabel.ForeColor = _error ? Color.FromArgb(246, 165, 143) : Color.FromArgb(170, 199, 147);
            Invalidate();
        }
        private void Notify(string text) { _error = false; messageLabel.Text = text; }
        private void Act(Action action)
        {
            if (Machine == null) return;
            if (_access.IsAdmin) { Notify("Для покупки выйдите из режима администратора."); RefreshView(); return; }
            try { _error = false; action(); }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { _error = true; messageLabel.Text = ex.Message; }
            RefreshView();
        }
    }
}
