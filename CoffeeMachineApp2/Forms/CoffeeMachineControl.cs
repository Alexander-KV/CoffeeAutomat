using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Один автомат: корпус, экран, терминал и выдача. Все состояния принадлежат модели.</summary>
    public sealed class CoffeeMachineControl : UserControl
    {
        public CoffeeMachine Machine { get; }
        public event EventHandler DuplicateRequested;
        public event EventHandler RemoveRequested;
        private readonly Action<MachineConfiguration> _save;
        private readonly AdminAccess _access;
        private readonly CardSlotView _cardSlot;
        private readonly ChangeTrayView _changeTray;
        private readonly RoundedButton _copy;
        private readonly Label _title, _step, _summary, _total, _wallet, _message, _terminalTotal, _sugarText, _bayTitle, _bayText;
        private const int MenuColumns = 3, MenuRows = 2, MenuPageSize = MenuColumns * MenuRows;
        private readonly TableLayoutPanel _menu;
        private readonly RoundedButton _previousPage, _nextPage;
        private readonly Label _pageLabel;
        private readonly RoundedButton _minus, _plus, _milk, _syrup, _next, _cancel, _cash, _card, _pay, _collect, _admin, _remove;
        private readonly List<RoundedButton> _notes = new List<RoundedButton>();
        private readonly List<RoundedButton> _filters = new List<RoundedButton>();
        private readonly Dictionary<RoundedButton, Beverage> _cards = new Dictionary<RoundedButton, Beverage>();
        private readonly Timer _timer = new Timer { Interval = 40 };
        private readonly Stopwatch _brewing = new Stopwatch();
        private readonly ToolTip _tips = new ToolTip { InitialDelay = 300, ReshowDelay = 100, AutoPopDelay = 8000 };
        private readonly PreparationView _dispenser;
        private int _filter, _page, _pageCount = 1;
        private bool _error;

        public CoffeeMachineControl(CoffeeMachine machine, Action<MachineConfiguration> save, AdminAccess access)
        {
            Machine = machine; _save = save; _access = access;
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
            var filters = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = MenuColumns, RowCount = 1,
                AutoSize = false, AutoScroll = false, GrowStyle = TableLayoutPanelGrowStyle.FixedSize, Margin = Padding.Empty };
            filters.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            string[] labels = { "Всё меню", "Без молока", "С молоком" };
            for (int i = 0; i < labels.Length; i++)
            {
                filters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / MenuColumns));
                int index = i; var button = UiStyle.Button(labels[i], (s,e) => { _filter = index; _page = 0; BuildMenu(); RefreshView(); });
                button.Dock = DockStyle.Fill; button.Font = UiStyle.Small; button.Margin = new Padding(4, 0, 4, 4);
                filters.Controls.Add(button, i, 0); _filters.Add(button);
            }
            layout.Controls.Add(filters, 0, 1);
            var catalog = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                AutoSize = false, AutoScroll = false, GrowStyle = TableLayoutPanelGrowStyle.FixedSize, Margin = Padding.Empty };
            catalog.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            catalog.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            catalog.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            _menu = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = MenuColumns, RowCount = MenuRows,
                AutoSize = false, AutoScroll = false, GrowStyle = TableLayoutPanelGrowStyle.FixedSize,
                Margin = Padding.Empty, BackColor = UiStyle.Cream };
            for (int i = 0; i < MenuColumns; i++) _menu.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / MenuColumns));
            for (int i = 0; i < MenuRows; i++) _menu.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / MenuRows));
            catalog.Controls.Add(_menu, 0, 0);
            var pager = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1,
                AutoSize = false, AutoScroll = false, GrowStyle = TableLayoutPanelGrowStyle.FixedSize, Margin = Padding.Empty };
            for (int i = 0; i < 3; i++) pager.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3));
            pager.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            _previousPage = UiStyle.Button("← Назад", (s,e) => ChangePage(-1));
            _nextPage = UiStyle.Button("Далее →", (s,e) => ChangePage(1));
            foreach (var button in new[] { _previousPage, _nextPage })
            {
                button.Dock = DockStyle.Fill; button.Font = UiStyle.Small; button.Margin = new Padding(4, 3, 4, 1);
            }
            _previousPage.AccessibleName = "Предыдущая страница напитков";
            _nextPage.AccessibleName = "Следующая страница напитков";
            _pageLabel = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter,
                Font = UiStyle.Small, ForeColor = UiStyle.Gray, AutoEllipsis = true, Margin = Padding.Empty };
            pager.Controls.Add(_previousPage, 0, 0); pager.Controls.Add(_pageLabel, 1, 0); pager.Controls.Add(_nextPage, 2, 0);
            catalog.Controls.Add(pager, 0, 1);
            layout.Controls.Add(catalog, 0, 2);
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
            _cardSlot = new CardSlotView { Bounds = new Rectangle(8, 5, 136, 132) };
            _cardSlot.Click += (s,e) => Act(() =>
            {
                if (Machine.Card.IsInserted) { Machine.EjectCard(); Notify("Карта возвращена. Спасибо!"); }
                else
                {
                    decimal refund = Machine.SelectPayment(true); Machine.InsertCard();
                    Notify(refund > 0 ? $"Карта вставлена. Внесённые наличные возвращены: {refund:0.##} ₽." : "Карта вставлена. Нажмите «Оплатить».");
                }
            });
            _cardSlot.AnimationFinished += (s,e) => RefreshView(); terminal.Controls.Add(_cardSlot);
            var display = new Panel { Bounds = new Rectangle(10, 141, 132, 105), BackColor = Color.FromArgb(19, 27, 22) }; UiStyle.Round(display, 8); terminal.Controls.Add(display);
            TextAt(display, "К ОПЛАТЕ", new Rectangle(9, 6, 114, 18), UiStyle.Small, Color.FromArgb(138, 167, 131)).TextAlign = ContentAlignment.MiddleCenter;
            _terminalTotal = TextAt(display, "— ₽", new Rectangle(4, 27, 124, 37), UiStyle.Title, Color.FromArgb(183, 220, 162)); _terminalTotal.TextAlign = ContentAlignment.MiddleCenter;
            _wallet = TextAt(display, "Выберите кофе", new Rectangle(7, 68, 118, 32), UiStyle.Small, Color.FromArgb(138, 167, 131)); _wallet.TextAlign = ContentAlignment.MiddleCenter;
            _cash = Place(terminal, "Наличные", new Rectangle(10, 252, 132, 32), (s,e) => SwitchPayment(false)); _cash.Dark = true;
            _card = Place(terminal, "Банковская карта", new Rectangle(10, 290, 132, 32), (s,e) => SwitchPayment(true)); _card.Dark = true;
            TextAt(terminal, "ВНЕСИТЕ КУПЮРУ", new Rectangle(10, 327, 132, 19), UiStyle.Small, Color.FromArgb(157, 169, 146)).TextAlign = ContentAlignment.MiddleCenter;
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
            _dispenser = new PreparationView { Bounds = new Rectangle(32, 790, 240, 142), BackColor = Color.FromArgb(21, 27, 23) }; Controls.Add(_dispenser);
            _bayTitle = TextAt(this, "Ваш кофе", new Rectangle(282, 797, 156, 25), UiStyle.Bold, UiStyle.Ivory);
            _bayText = TextAt(this, "Свежий помол.\nТочный рецепт.", new Rectangle(282, 826, 156, 40), UiStyle.Small, Color.FromArgb(157, 173, 148));
            _collect = Place(this, "Забрать кофе", new Rectangle(280, 876, 160, 43), (s,e) => Act(() =>
            {
                Machine.CollectDrink(); _timer.Stop();
                Notify(Machine.Change > 0 ? "Напиток получен. Заберите сдачу из лотка справа." : Machine.Card.IsInserted ? "Напиток получен. Заберите карту из терминала." : "Хороший кофе — хорошее начало. Приятного дня!");
            }), true); _collect.Dark = true;
            _changeTray = new ChangeTrayView { Bounds = new Rectangle(450, 792, 154, 134) };
            _changeTray.Click += (s,e) => Act(() => { decimal amount = Machine.CollectChange(); Notify($"Сдача {amount:0.##} ₽ получена." + (Machine.Order != null && !Machine.DrinkCollected ? " Не забудьте напиток." : " Приятного дня!")); });
            Controls.Add(_changeTray);
            _admin = Place(this, "Настройки", new Rectangle(26, 952, 132, 30), OpenAdmin); _admin.Dark = true; _admin.Font = UiStyle.Small;
            _copy = Place(this, "+ Копировать автомат", new Rectangle(168, 952, 226, 30), (s,e) => { if (_access.IsAdmin) DuplicateRequested?.Invoke(this, EventArgs.Empty); }); _copy.Dark = true; _copy.Font = UiStyle.Small;
            _remove = Place(this, "Убрать автомат", new Rectangle(434, 952, 174, 30), (s,e) => RemoveRequested?.Invoke(this, EventArgs.Empty)); _remove.Dark = true; _remove.Font = UiStyle.Small;
            _timer.Tick += Tick; BuildMenu(); RefreshAccess();
        }
        public void RefreshAccess()
        {
            _admin.Visible = _copy.Visible = _remove.Visible = _access.IsAdmin;
            Notify(_access.IsAdmin ? "Режим администратора. Покупки приостановлены." : "Готов к работе. Выберите свой кофе.");
            RefreshView();
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
            UiStyle.FillRound(g, new Rectangle(23, 565, 106, 12), Color.FromArgb(14, 22, 17), 5);
            using var slot = new Pen(Color.FromArgb(104, 117, 94)); g.DrawLine(slot, 32, 571, 120, 571);
            g.Restore(saved);
        }
        private void BuildMenu()
        {
            _menu.SuspendLayout();
            try
            {
                foreach (var b in _cards.Keys) { _tips.SetToolTip(b, null); b.Image?.Dispose(); }
                foreach (Control control in _menu.Controls.Cast<Control>().ToArray()) control.Dispose();
                _cards.Clear(); _menu.Controls.Clear();
                var drinks = Machine.Configuration.Menu.Where(x => _filter == 0 || (_filter == 1 ? !x.HasMilk : x.HasMilk)).ToArray();
                _pageCount = Math.Max(1, (drinks.Length + MenuPageSize - 1) / MenuPageSize);
                _page = Math.Clamp(_page, 0, _pageCount - 1);
                var page = drinks.Skip(_page * MenuPageSize).Take(MenuPageSize).ToArray();
                for (int i = 0; i < page.Length; i++)
                {
                    var drink = page[i];
                    var b = new RoundedButton { Dock = DockStyle.Fill, Margin = new Padding(LogicalToDeviceUnits(4)), Font = UiStyle.Regular,
                        Image = new Bitmap(ImageLoader.Load(drink), new Size(132, 88)), Text = drink.Name, Detail = drink.VolumeMl + " мл", Price = $"{drink.Price:0.##} ₽",
                        TabIndex = i, AccessibleName = $"{drink.Name}, {drink.VolumeMl} мл, {drink.Price:0.##} рублей" };
                    string id = drink.Id; b.Click += (s,e) => Act(() => { Machine.Select(id); Notify(drink.Recipe); });
                    _tips.SetToolTip(b, drink.Name + "\n" + drink.Recipe); _cards.Add(b, drink); _menu.Controls.Add(b, i % MenuColumns, i / MenuColumns);
                }
                if (drinks.Length == 0)
                {
                    var empty = new Label { Text = "В этой категории пока нет напитков.\nПопробуйте другую категорию.", Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter, Font = UiStyle.Regular, ForeColor = UiStyle.Gray,
                        AutoEllipsis = true, Margin = Padding.Empty, Padding = new Padding(LogicalToDeviceUnits(12)) };
                    _menu.Controls.Add(empty, 0, 0); _menu.SetColumnSpan(empty, MenuColumns); _menu.SetRowSpan(empty, MenuRows);
                }
                _pageLabel.Text = _pageCount > 1 ? $"{_page + 1} / {_pageCount}" : $"Напитков: {drinks.Length}";
                _pageLabel.AccessibleName = $"Страница {_page + 1} из {_pageCount}. Всего напитков: {drinks.Length}";
                _previousPage.Visible = _nextPage.Visible = _pageCount > 1;
            }
            finally { _menu.ResumeLayout(true); }
        }
        private void ChangePage(int direction)
        {
            if (_access.IsAdmin || Machine.State != MachineState.Choosing) return;
            int page = Math.Clamp(_page + direction, 0, _pageCount - 1);
            if (page == _page) return;
            _page = page; BuildMenu(); RefreshView();
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
            Notify(refund > 0 ? $"Внесённые наличные возвращены: {refund:0.##} ₽." : card ? (Machine.Card.IsInserted ? "Карта принята. Подтвердите оплату на терминале." : "Нажмите на карту над терминалом, чтобы вставить её.") : "Внесите купюры на терминале справа.");
        });
        private void OpenAdmin(object sender, EventArgs e)
        {
            if (!_access.IsAdmin) return;
            using (var form = new AdminForm(Machine.Configuration, config => { _save(config); Machine.UpdateConfiguration(config); })) form.ShowDialog(this);
            _title.Text = Machine.Configuration.Name; BuildMenu(); RefreshView(); Notify("Настройки загружены. Автомат готов к работе.");
        }
        private void Tick(object sender, EventArgs e)
        {
            if (Machine.State == MachineState.Ready) { if (!Machine.DrinkCollected) _dispenser.Advance(); else _timer.Stop(); return; }
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
            _bayText.Text = $"{(int)(_dispenser.Progress * 100)}%  ·  {remaining} сек.\nГотовим ваш напиток";
            if (_dispenser.Progress >= 1) { Machine.FinishPreparing(); Notify("Готово! Заберите горячий напиток в нише выдачи."); RefreshView(); }
        }
        private void RefreshView()
        {
            bool customer = !_access.IsAdmin;
            bool choosing = customer && Machine.State == MachineState.Choosing, paying = customer && Machine.State == MachineState.Paying, ready = customer && Machine.State == MachineState.Ready;
            var order = Machine.Order;
            _step.Text = !customer ? "ОБСЛУЖИВАНИЕ · РЕЖИМ АДМИНИСТРАТОРА" : Machine.State switch { MachineState.Choosing => "01  ВЫБОР     ·     02  ОПЛАТА     ·     03  ГОТОВО", MachineState.Paying => "01  ВЫБРАНО     →     02  ОПЛАТА     ·     03  ГОТОВО", MachineState.Brewing => "01  ВЫБРАНО     ·     02  ОПЛАЧЕНО     →     ГОТОВИМ", _ => "ВАШ КОФЕ ГОТОВ. ХОРОШЕГО ДНЯ!" };
            for (int i = 0; i < _filters.Count; i++) { _filters[i].Selected = _filter == i; _filters[i].Enabled = choosing; }
            _previousPage.Enabled = choosing && _page > 0;
            _nextPage.Enabled = choosing && _page < _pageCount - 1;
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
            _cardSlot.Inserted = Machine.Card.IsInserted;
            _cardSlot.Enabled = customer && !_cardSlot.IsAnimating && (paying || Machine.Card.IsInserted);
            _pay.Enabled = paying && (Machine.UseCard ? Machine.Card.IsInserted && !_cardSlot.IsAnimating : Machine.Cash.Inserted >= order.Total);
            _terminalTotal.Text = order == null ? "— ₽" : $"{order.Total:0.##} ₽";
            _terminalTotal.Font = order?.Total >= 1000 ? UiStyle.CompactPrice : UiStyle.Title;
            _wallet.Text = paying ? (Machine.UseCard ? (Machine.Card.IsInserted ? "Карта принята\nПодтвердите оплату" : "Вставьте карту\nв слот выше") : $"Внесено {Machine.Cash.Inserted:0.##} ₽\nЕщё {Math.Max(0, order.Total - Machine.Cash.Inserted):0.##} ₽") : Machine.Card.IsInserted ? "ОПЛАЧЕНО\nЗаберите карту" : ready ? "ОПЛАЧЕНО\nПриятного дня" : Machine.State == MachineState.Brewing ? "ОПЛАЧЕНО\nГотовим ваш кофе" : _access.IsAdmin ? "ОБСЛУЖИВАНИЕ" : "Выберите кофе\nна экране слева";
            _admin.Enabled = _remove.Enabled = _copy.Enabled = _access.IsAdmin && order == null;
            _collect.Enabled = ready && !Machine.DrinkCollected;
            _collect.Text = ready && Machine.DrinkCollected ? "Кофе получен" : "Забрать кофе";
            _changeTray.Amount = Machine.Change; _changeTray.Enabled = customer && Machine.Change > 0;
            _dispenser.Visible = !Machine.DrinkCollected;
            _dispenser.Mode = Machine.State == MachineState.Brewing ? PreparationMode.Brewing : ready ? PreparationMode.Ready : PreparationMode.Idle;
            if (order != null) { _dispenser.LiquidColor = ColorTranslator.FromHtml(order.Beverage.LiquidColor); _dispenser.HasMilk = order.Beverage.HasMilk || order.ExtraMilk; }
            if (ready)
            {
                _bayTitle.Text = Machine.DrinkCollected ? "Приятного дня!" : order.Beverage.Name;
                _bayText.Text = Machine.DrinkCollected ? (Machine.Change > 0 ? "Заберите сдачу →" : "Заберите карту ↑") : "Ваш кофе готов.\nОсторожно, горячо.";
            }
            else if (Machine.State != MachineState.Brewing)
            {
                _dispenser.Progress = 0; _bayTitle.Text = "Ваш кофе";
                _bayText.Text = "Свежий помол.\nТочный рецепт.";
            }
            _message.ForeColor = _error ? Color.FromArgb(246, 165, 143) : Color.FromArgb(170, 199, 147);
            Invalidate();
        }
        private void Notify(string text) { _error = false; _message.Text = text; }
        private void Act(Action action)
        {
            if (_access.IsAdmin) { Notify("Для покупки выйдите из режима администратора."); RefreshView(); return; }
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
