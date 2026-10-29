using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms.Modules
{
    /// <summary>Ниша выдачи: отображает чашку и сдачу, передаёт действия владельцу автомата.</summary>
    [DefaultEvent(nameof(DrinkCollected))]
    public partial class DispensingControl : UserControl
    {
        private bool _ready;

        [Category("Действия")]
        [Description("Покупатель нажал кнопку получения напитка.")]
        public event EventHandler DrinkCollected;

        [Category("Действия")]
        [Description("Покупатель нажал на лоток сдачи.")]
        public event EventHandler ChangeCollected;

        public DispensingControl()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            InitializeComponent();
            UpdateRoundedRegion();
        }

        public void ShowState(CoffeeMachine machine, bool administrator)
        {
            ArgumentNullException.ThrowIfNull(machine);
            bool customer = !administrator;
            bool ready = customer && machine.State == MachineState.Ready;
            var order = machine.Order;
            _ready = machine.State == MachineState.Ready;

            collectButton.Enabled = ready && !machine.DrinkCollected;
            collectButton.Text = ready && machine.DrinkCollected ? "Кофе получен" : "Забрать кофе";
            changeTrayView.Amount = machine.Change;
            changeTrayView.Enabled = customer && machine.Change > 0;
            preparationView.Visible = !machine.DrinkCollected;
            preparationView.Mode = machine.State == MachineState.Brewing
                ? PreparationMode.Brewing : ready ? PreparationMode.Ready : PreparationMode.Idle;
            if (order != null)
            {
                preparationView.LiquidColor = ColorTranslator.FromHtml(order.Beverage.LiquidColor);
                preparationView.HasMilk = order.Beverage.HasMilk || order.ExtraMilk;
            }
            if (ready)
            {
                bayTitleLabel.Text = machine.DrinkCollected ? "Приятного дня!" : order.Beverage.Name;
                bayDescriptionLabel.Text = machine.DrinkCollected
                    ? (machine.Change > 0 ? "Заберите сдачу →" : "Заберите карту ↑")
                    : "Ваш кофе готов.\nОсторожно, горячо.";
            }
            else if (machine.State != MachineState.Brewing)
            {
                preparationView.Progress = 0;
                bayTitleLabel.Text = "Ваш кофе";
                bayDescriptionLabel.Text = "Свежий помол.\nТочный рецепт.";
            }
            Invalidate();
        }

        public void ShowProgress(CoffeeOrder order, double progress, int remainingSeconds, string stage)
        {
            ArgumentNullException.ThrowIfNull(order);
            preparationView.Progress = progress;
            bayTitleLabel.Text = stage + "…";
            bayDescriptionLabel.Text = $"{(int)(preparationView.Progress * 100)}%  ·  {Math.Max(0, remainingSeconds)} сек.\nГотовим ваш напиток";
        }

        public void AdvanceAnimation()
        {
            preparationView.Advance();
        }

        private void CollectButton_Click(object sender, EventArgs e)
        {
            DrinkCollected?.Invoke(this, e);
        }

        private void ChangeTrayView_Click(object sender, EventArgs e)
        {
            ChangeCollected?.Invoke(this, e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float scale = DeviceDpi / 96f;
            UiStyle.FillRound(g, ClientRectangle, Color.FromArgb(21, 27, 23), LogicalToDeviceUnits(18));
            using (var glow = new Pen(_ready ? Color.FromArgb(151, 212, 132) : UiStyle.Gold, 3 * scale))
                g.DrawLine(glow, 22 * scale, 2 * scale, Width - 22 * scale, 2 * scale);
            if (preparationView == null) return;
            using (var grille = new Pen(Color.FromArgb(55, 67, 53), scale))
            {
                for (float x = preparationView.Left + 16 * scale; x < preparationView.Right + 22 * scale; x += 10 * scale)
                    g.DrawLine(grille, x, Height - 4 * scale, x + 5 * scale, Height - 9 * scale);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateRoundedRegion();
        }

        private void UpdateRoundedRegion()
        {
            if (ClientSize.Width <= 2 || ClientSize.Height <= 2) return;
            using var outline = UiStyle.RoundedRect(ClientRectangle, LogicalToDeviceUnits(18));
            var previous = Region;
            Region = new Region(outline);
            previous?.Dispose();
        }
    }
}
