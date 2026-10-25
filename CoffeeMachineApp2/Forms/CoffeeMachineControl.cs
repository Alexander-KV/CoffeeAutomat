using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Один автомат: корпус, экран, терминал и выдача. Все состояния принадлежат модели.</summary>
    public sealed class CoffeeMachineControl : UserControl
    {
        public CoffeeMachine Machine { get; }
        public event EventHandler DuplicateRequested;
        public event EventHandler RemoveRequested;
        private readonly Action<MachineConfiguration> _save;
        private readonly Label _title, _step, _summary, _total, _wallet, _message, _terminalTotal, _sugarText, _bayTitle, _bayText;
        private readonly FlowLayoutPanel _menu;
        private readonly RoundedButton _minus, _plus, _milk, _syrup, _next, _cancel, _cash, _card, _pay, _collect, _admin, _remove;
        private readonly List<RoundedButton> _notes = new List<RoundedButton>();
        private readonly List<RoundedButton> _filters = new List<RoundedButton>();
        private readonly Dictionary<RoundedButton, Beverage> _cards = new Dictionary<RoundedButton, Beverage>();
        private readonly Timer _timer = new Timer { Interval = 40 };
        private readonly Stopwatch _brewing = new Stopwatch();
        private readonly ToolTip _tips = new ToolTip { InitialDelay = 300, ReshowDelay = 100, AutoPopDelay = 8000 };
        private readonly PreparationView _dispenser;
        private int _filter;
        private bool _error;

        public CoffeeMachineControl(CoffeeMachine machine, Action<MachineConfiguration> save)
        {
            Machine = machine; _save = save;
            AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            Size = new Size(640, 1000); Margin = new Padding(14, 8, 14, 24);
            BackColor = Color.FromArgb(232, 231, 220); Font = UiStyle.Regular;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            var brand = TextAt(this, "B E A N  &  C U P", new Rectangle(32, 19, 330, 18), UiStyle.Small, UiStyle.Gold);
            _title = TextAt(this, machine.Configuration.Name, new Rectangle(30, 39, 470, 38), UiStyle.Display, UiStyle.Ivory);
            var serial = TextAt(this, "CRAFTED COFFEE\n№ " + machine.Configuration.Id.Substring(0, 6).ToUpperInvariant(), new Rectangle(490, 27, 116, 42), UiStyle.Small, Color.FromArgb(171, 177, 159));
            serial.TextAlign = ContentAlignment.MiddleRight;

            var screen = new Panel { Bounds = new Rectangle(24, 100, 422, 640), BackColor = UiStyle.Cream, Padding = new Padding(12) };
            UiStyle.Round(screen, 19); Controls.Add(screen);
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6, Margin = Padding.Empty };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int h in new[] { 82, 36, 294, 58, 88, 54 }) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, h));
            screen.Controls.Add(layout);
            var intro = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            TextAt(intro, "СВЕЖИЕ ЗЁРНА. ВАШ МОМЕНТ.", new Rectangle(2, 0, 390, 18), UiStyle.Small, UiStyle.Forest);
            TextAt(intro, "Пора на кофе.", new Rectangle(0, 18, 390, 38), UiStyle.Display, UiStyle.Ink);
            _step = TextAt(intro, "01  ВЫБОР     ·     02  ОПЛАТА     ·     03  ГОТОВО", new Rectangle(2, 61, 390, 18), UiStyle.Small, UiStyle.Gray);
            layout.Controls.Add(intro, 0, 0);
            var filters = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false, Margin = Padding.Empty };
            string[] labels = { "Всё меню", "Без молока", "С молоком" };
            for (int i = 0; i < labels.Length; i++)
            {
                int index = i; var button = UiStyle.Button(labels[i], (s,e) => { _filter = index; BuildMenu(); RefreshView(); });
                button.Width = 124; button.Height = 30; button.Font = UiStyle.Small; button.Margin = new Padding(1, 0, 5, 4);
                filters.Controls.Add(button); _filters.Add(button);
            }
            layout.Controls.Add(filters, 0, 1);
            _menu = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, Margin = Padding.Empty, BackColor = UiStyle.Cream };
            layout.Controls.Add(_menu, 0, 2);
            var summary = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _summary = TextAt(summary, "Найдите свой любимый", new Rectangle(3, 6, 263, 23), UiStyle.Bold, UiStyle.Ink);
            TextAt(summary, "Свежая чашка, приготовленная для вас", new Rectangle(3, 30, 277, 18), UiStyle.Small, UiStyle.Gray);
            _total = TextAt(summary, "— ₽", new Rectangle(282, 5, 110, 42), UiStyle.Title, UiStyle.Forest); _total.TextAlign = ContentAlignment.MiddleRight;
            layout.Controls.Add(summary, 0, 3);
            var options = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _minus = Place(options, "−", new Rectangle(0, 0, 34, 34), (s,e) => Configure(-1));
            _sugarText = TextAt(options, "Сахар 0 / 4", new Rectangle(38, 0, 104, 34), UiStyle.Regular, UiStyle.Ink); _sugarText.TextAlign = ContentAlignment.MiddleCenter;
            _plus = Place(options, "+", new Rectangle(146, 0, 34, 34), (s,e) => Configure(1));
            _milk = Place(options, "Молоко +30 ₽", new Rectangle(190, 0, 203, 34), (s,e) => Configure(milk: true));
            _syrup = Place(options, "Сироп: без сиропа", new Rectangle(0, 41, 223, 34), (s,e) => Configure(syrup: true));
            TextAt(options, "0–2 сахара бесплатно\n3–4: +10 ₽ · сироп: +30 ₽", new Rectangle(232, 40, 164, 38), UiStyle.Small, UiStyle.Gray);
            layout.Controls.Add(options, 0, 4);
            var actions = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _cancel = Place(actions, "Сбросить", new Rectangle(0, 2, 98, 45), CancelOrBack);
            _next = Place(actions, "Выберите напиток", new Rectangle(105, 2, 288, 45), (s,e) => Act(() => { Machine.BeginPayment(); Notify("Выберите способ оплаты на терминале справа."); }), true);
            layout.Controls.Add(actions, 0, 5);

            var terminal = new Panel { Bounds = new Rectangle(464, 100, 152, 640), BackColor = Color.FromArgb(35, 40, 35) };
            UiStyle.Round(terminal, 17); Controls.Add(terminal);
            terminal.Paint += PaintTerminal;
            TextAt(terminal, "БЕСКОНТАКТНО", new Rectangle(8, 67, 136, 22), UiStyle.Small, Color.FromArgb(157, 169, 146)).TextAlign = ContentAlignment.MiddleCenter;
            var display = new Panel { Bounds = new Rectangle(10, 103, 132, 105), BackColor = Color.FromArgb(19, 27, 22) }; UiStyle.Round(display, 8); terminal.Controls.Add(display);
            TextAt(display, "К ОПЛАТЕ", new Rectangle(9, 6, 114, 18), UiStyle.Small, Color.FromArgb(138, 167, 131)).TextAlign = ContentAlignment.MiddleCenter;
            _terminalTotal = TextAt(display, "— ₽", new Rectangle(4, 27, 124, 37), UiStyle.Title, Color.FromArgb(183, 220, 162)); _terminalTotal.TextAlign = ContentAlignment.MiddleCenter;
            _wallet = TextAt(display, "Выберите кофе", new Rectangle(7, 68, 118, 32), UiStyle.Small, Color.FromArgb(138, 167, 131)); _wallet.TextAlign = ContentAlignment.MiddleCenter;
            _cash = Place(terminal, "Наличные", new Rectangle(10, 223, 132, 38), (s,e) => SwitchPayment(false)); _cash.Dark = true;
            _card = Place(terminal, "Демо-карта", new Rectangle(10, 267, 132, 38), (s,e) => SwitchPayment(true)); _card.Dark = true;
            TextAt(terminal, "ВНЕСИТЕ КУПЮРУ", new Rectangle(10, 321, 132, 19), UiStyle.Small, Color.FromArgb(157, 169, 146)).TextAlign = ContentAlignment.MiddleCenter;
            int[] amounts = { 10, 50, 100, 500 };
            for (int i = 0; i < amounts.Length; i++)
            {
                int amount = amounts[i];
                var note = Place(terminal, "+" + amount, new Rectangle(10 + (i % 2) * 69, 348 + (i / 2) * 41, 63, 35), (s,e) => Act(() => Machine.InsertMoney(amount)));
                note.Dark = true; _notes.Add(note);
            }
            _pay = Place(terminal, "Оплатить →", new Rectangle(10, 446, 132, 47), (s,e) => Act(() => { Machine.Pay(); _brewing.Restart(); _timer.Start(); Notify("Заказ оплачен. Магия уже началась."); }), true);
            TextAt(terminal, "Учебный терминал\nБез реального списания", new Rectangle(10, 509, 132, 40), UiStyle.Small, Color.FromArgb(137, 147, 129)).TextAlign = ContentAlignment.MiddleCenter;
            TextAt(terminal, "SECURE  /  COFFEE PAY", new Rectangle(10, 597, 132, 20), UiStyle.Small, Color.FromArgb(137, 147, 129)).TextAlign = ContentAlignment.MiddleCenter;
            _message = TextAt(this, "Готов к работе. Выберите свой кофе.", new Rectangle(33, 746, 574, 28), UiStyle.Regular, Color.FromArgb(170, 199, 147));
            _dispenser = new PreparationView { Bounds = new Rectangle(32, 790, 260, 142), BackColor = Color.FromArgb(21, 27, 23) }; Controls.Add(_dispenser);
            _bayTitle = TextAt(this, "Здесь появится ваш кофе", new Rectangle(318, 797, 280, 25), UiStyle.Bold, UiStyle.Ivory);
            _bayText = TextAt(this, "Свежий помол. Точный рецепт.\nМаленькая пауза для большого дня.", new Rectangle(318, 826, 280, 38), UiStyle.Small, Color.FromArgb(157, 173, 148));
            _collect = Place(this, "Забрать напиток", new Rectangle(316, 876, 282, 43), (s,e) => Act(() =>
            {
                decimal change = Machine.Collect(); _timer.Stop();
                Notify(change > 0 ? $"Приятного дня! Вместе с кофе вы получили {change:0.##} ₽ сдачи." : "Хороший кофе — хорошее начало. Приятного дня!");
            }), true); _collect.Dark = true;
            _admin = Place(this, "Настройки", new Rectangle(26, 952, 132, 30), OpenAdmin); _admin.Dark = true; _admin.Font = UiStyle.Small;
            var copy = Place(this, "+ Копировать автомат", new Rectangle(168, 952, 226, 30), (s,e) => DuplicateRequested?.Invoke(this, EventArgs.Empty)); copy.Dark = true; copy.Font = UiStyle.Small;
            _remove = Place(this, "Убрать автомат", new Rectangle(434, 952, 174, 30), (s,e) => RemoveRequested?.Invoke(this, EventArgs.Empty)); _remove.Dark = true; _remove.Font = UiStyle.Small;
            _timer.Tick += Tick; BuildMenu(); RefreshView();
        }
        private static Label TextAt(Control parent, string text, Rectangle bounds, Font font, Color color)
        {
            var label = new Label { Text = text, Bounds = bounds, Font = font, ForeColor = color, BackColor = Color.Transparent,
                AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, UseMnemonic = false };
            parent.Controls.Add(label); return label;
        }
        private static RoundedButton Place(Control parent, string text, Rectangle bounds, EventHandler click, bool primary = false)
        {
            var button = UiStyle.Button(text, click, primary); button.Bounds = bounds; parent.Controls.Add(button); return button;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save(); g.ScaleTransform(Width / 640f, Height / 1000f);
            UiStyle.FillRound(g, new Rectangle(9, 9, 622, 984), Color.FromArgb(198, 197, 183), 30);
            using (var body = UiStyle.RoundedRect(new Rectangle(3, 1, 629, 990), 28))
            using (var gradient = new LinearGradientBrush(new Rectangle(3, 1, 629, 990), Color.FromArgb(51, 58, 48), Color.FromArgb(25, 32, 27), 0f))
            using (var edge = new Pen(Color.FromArgb(92, 102, 82))) { g.FillPath(gradient, body); g.DrawPath(edge, body); }
            UiStyle.FillRound(g, new Rectangle(20, 91, 430, 655), Color.FromArgb(12, 20, 16), 23);
            using (var accent = new Pen(UiStyle.Gold, 2)) g.DrawLine(accent, 32, 83, 606, 83);
            UiStyle.FillRound(g, new Rectangle(24, 782, 592, 156), Color.FromArgb(21, 27, 23), 18);
            using (var glow = new Pen(Machine.State == MachineState.Ready ? Color.FromArgb(151, 212, 132) : UiStyle.Gold, 3)) g.DrawLine(glow, 46, 784, 594, 784);
            using (var led = new SolidBrush(Machine.State == MachineState.Ready ? UiStyle.Green : UiStyle.Gold)) g.FillEllipse(led, 601, 74, 6, 6);
            using (var grille = new Pen(Color.FromArgb(55, 67, 53))) for (int x = 48; x < 294; x += 10) g.DrawLine(grille, x, 934, x + 5, 929);
            g.Restore(saved);
        }
        private void PaintTerminal(object sender, PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save(); g.ScaleTransform(((Control)sender).Width / 152f, ((Control)sender).Height / 640f);
            using var pen = new Pen(UiStyle.Gold, 2.3f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            for (int i = 0; i < 3; i++) g.DrawArc(pen, 66 - i * 9, 22 - i * 7, 16 + i * 18, 26 + i * 14, -63, 126);
            UiStyle.FillRound(g, new Rectangle(23, 565, 106, 12), Color.FromArgb(14, 22, 17), 5);
            using var slot = new Pen(Color.FromArgb(104, 117, 94)); g.DrawLine(slot, 32, 571, 120, 571);
            g.Restore(saved);
        }
        private void BuildMenu()
        {
            _menu.SuspendLayout();
            foreach (var b in _cards.Keys) { _tips.SetToolTip(b, null); b.Image?.Dispose(); b.Dispose(); }
            _cards.Clear(); _menu.Controls.Clear();
            var drinks = Machine.Configuration.Menu.Where(x => _filter == 0 || (_filter == 1 ? !x.HasMilk : x.HasMilk)).ToArray();
            foreach (var drink in drinks)
            {
                var b = new RoundedButton { Width = 122, Height = 90, Margin = new Padding(4), Font = UiStyle.Regular,
                    Image = new Bitmap(ImageLoader.Load(drink), new Size(66, 44)), Text = drink.Name, Detail = drink.VolumeMl + " мл", Price = $"{drink.Price:0.##} ₽",
                    AccessibleName = $"{drink.Name}, {drink.VolumeMl} мл, {drink.Price:0.##} рублей" };
                string id = drink.Id; b.Click += (s,e) => Act(() => { Machine.Select(id); Notify(drink.Recipe); });
                _tips.SetToolTip(b, drink.Name + "\n" + drink.Recipe); _cards.Add(b, drink); _menu.Controls.Add(b);
            }
            if (drinks.Length == 0) _menu.Controls.Add(new Label { Text = "В этой категории пока нет напитков.\nВыберите «Всё меню» или добавьте рецепт в настройках.", Width = 350, Height = 90, ForeColor = UiStyle.Gray, Padding = new Padding(12) });
            _menu.ResumeLayout();
        }
        private void Configure(int sugarDelta = 0, bool milk = false, bool syrup = false)
        {
            if (Machine.Order == null) return;
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
            Notify(refund > 0 ? $"Внесённые наличные возвращены: {refund:0.##} ₽." : card ? "Демо-карта выбрана. Подтвердите оплату на терминале." : "Внесите купюры на терминале справа.");
        });
        private void OpenAdmin(object sender, EventArgs e)
        {
            using (var form = new AdminForm(Machine.Configuration, config => { _save(config); Machine.UpdateConfiguration(config); })) form.ShowDialog(this);
            _title.Text = Machine.Configuration.Name; BuildMenu(); RefreshView(); Notify("Настройки загружены. Автомат готов к работе.");
        }
        private void Tick(object sender, EventArgs e)
        {
            if (Machine.State == MachineState.Ready) { _dispenser.Advance(); return; }
            if (Machine.State != MachineState.Brewing) { _timer.Stop(); return; }
            _dispenser.Progress = _brewing.Elapsed.TotalSeconds / Machine.Order.Beverage.PrepareSeconds; _dispenser.Advance();
            var drink = Machine.Order.Beverage;
            string stage = _dispenser.Progress switch
            {
                < 0.25 => drink.IsCoffee ? "Мелем свежие зёрна" : "Подогреваем молоко",
                < 0.55 => drink.IsCoffee ? "Завариваем эспрессо" : "Растапливаем шоколад",
                < 0.8 => drink.HasMilk || Machine.Order.ExtraMilk ? "Добавляем молоко и пену" : "Наполняем чашку",
                _ => "Добавляем последний штрих"
            };
            _bayTitle.Text = stage + "…";
            int remaining = Math.Max(0, (int)Math.Ceiling(drink.PrepareSeconds - _brewing.Elapsed.TotalSeconds));
            _bayText.Text = $"{(int)(_dispenser.Progress * 100)}%  ·  осталось около {remaining} сек.\nВаш кофе почти у вас.";
            if (_dispenser.Progress >= 1) { Machine.FinishPreparing(); Notify("Готово! Заберите горячий напиток в нише выдачи."); RefreshView(); }
        }
        private void RefreshView()
        {
            bool choosing = Machine.State == MachineState.Choosing, paying = Machine.State == MachineState.Paying, ready = Machine.State == MachineState.Ready;
            var order = Machine.Order;
            _step.Text = Machine.State switch { MachineState.Choosing => "01  ВЫБОР     ·     02  ОПЛАТА     ·     03  ГОТОВО", MachineState.Paying => "01  ВЫБРАНО     →     02  ОПЛАТА     ·     03  ГОТОВО", MachineState.Brewing => "01  ВЫБРАНО     ·     02  ОПЛАЧЕНО     →     ГОТОВИМ", _ => "ВАШ КОФЕ ГОТОВ. ХОРОШЕГО ДНЯ!" };
            for (int i = 0; i < _filters.Count; i++) { _filters[i].Selected = _filter == i; _filters[i].Enabled = choosing; }
            foreach (var pair in _cards) { pair.Key.Enabled = choosing && pair.Value.IsEnabled; pair.Key.Selected = order?.Beverage.Id == pair.Value.Id; }
            _summary.Text = order?.Beverage.Name ?? "Найдите свой любимый";
            _total.Text = order == null ? "— ₽" : $"{order.Total:0.##} ₽";
            _total.Font = order?.Total >= 1000 ? UiStyle.CompactPrice : UiStyle.Title;
            _sugarText.Text = $"Сахар {order?.Sugar ?? 0} / 4";
            bool editable = choosing && order != null;
            _minus.Enabled = editable && order.Sugar > 0; _plus.Enabled = editable && order.Sugar < 4;
            _milk.Enabled = _syrup.Enabled = editable;
            _milk.Selected = order?.ExtraMilk ?? false; _milk.Text = order?.ExtraMilk == true ? "✓ Молоко +30 ₽" : "Молоко +30 ₽";
            _syrup.Text = "Сироп: " + (order?.Syrup.ToLower() ?? "без сиропа"); _syrup.Selected = order != null && order.Syrup != "Без сиропа";
            _next.Enabled = editable;
            _next.Text = Machine.State switch
            {
                MachineState.Paying => "Оплатите на терминале →",
                MachineState.Brewing => "Готовим ваш кофе…",
                MachineState.Ready => "Готово · заберите внизу ↓",
                _ => order == null ? "Выберите напиток" : $"К оплате · {order.Total:0.##} ₽ →"
            };
            _cancel.Enabled = (choosing && order != null) || paying; _cancel.Text = paying ? "← Назад" : "Сбросить";
            _cash.Enabled = _card.Enabled = paying; _cash.Selected = !Machine.UseCard; _card.Selected = Machine.UseCard;
            foreach (var note in _notes) note.Enabled = paying && !Machine.UseCard;
            _pay.Enabled = paying && (Machine.UseCard ? Machine.Card.Balance >= order.Total : Machine.Cash.Inserted >= order.Total);
            _terminalTotal.Text = order == null ? "— ₽" : $"{order.Total:0.##} ₽";
            _terminalTotal.Font = order?.Total >= 1000 ? UiStyle.CompactPrice : UiStyle.Title;
            _wallet.Text = paying ? (Machine.UseCard ? $"Баланс {Machine.Card.Balance:0.##} ₽" + (Machine.Card.Balance < order.Total ? "\nНедостаточно средств" : "\nПодтвердите оплату") : $"Внесено {Machine.Cash.Inserted:0.##} ₽\nЕщё {Math.Max(0, order.Total - Machine.Cash.Inserted):0.##} ₽") : ready ? "ОПЛАЧЕНО\nПриятного дня" : Machine.State == MachineState.Brewing ? "ОПЛАЧЕНО\nГотовим ваш кофе" : "Выберите кофе\nна экране слева";
            _admin.Enabled = _remove.Enabled = choosing && order == null; _collect.Enabled = ready;
            _collect.Text = ready && Machine.Change > 0 ? $"Забрать кофе и {Machine.Change:0.##} ₽" : ready ? "Забрать мой кофе →" : "Здесь будет ваш кофе";
            _dispenser.Mode = Machine.State == MachineState.Brewing ? PreparationMode.Brewing : ready ? PreparationMode.Ready : PreparationMode.Idle;
            if (order != null) { _dispenser.LiquidColor = ColorTranslator.FromHtml(order.Beverage.LiquidColor); _dispenser.HasMilk = order.Beverage.HasMilk || order.ExtraMilk; }
            if (ready)
            {
                _bayTitle.Text = order.Beverage.Name + " готов!";
                _bayText.Text = Machine.Change > 0 ? $"Ваша сдача: {Machine.Change:0.##} ₽\nПолучите её вместе с напитком." : "Осторожно, горячо.\nЗаберите чашку и наслаждайтесь.";
            }
            else if (Machine.State != MachineState.Brewing)
            {
                _dispenser.Progress = 0; _bayTitle.Text = "Здесь появится ваш кофе";
                _bayText.Text = "Свежий помол. Точный рецепт.\nМаленькая пауза для большого дня.";
            }
            _message.ForeColor = _error ? Color.FromArgb(246, 165, 143) : Color.FromArgb(170, 199, 147);
            Invalidate();
        }
        private void Notify(string text) { _error = false; _message.Text = text; }
        private void Act(Action action)
        {
            try { _error = false; action(); }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException) { _error = true; _message.Text = ex.Message; }
            RefreshView();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Dispose(); _tips.Dispose(); foreach (var b in _cards.Keys) b.Image?.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
