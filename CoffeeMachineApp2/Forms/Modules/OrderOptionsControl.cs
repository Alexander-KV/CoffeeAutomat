using System;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms.Modules
{
    /// <summary>Параметры выбранной порции. Только отображение и события, без изменения модели.</summary>
    public partial class OrderOptionsControl : ScalableControl
    {
        private CoffeeOrder _order;
        private MachineState _state = MachineState.Choosing;
        private bool _administrator;

        public event EventHandler<int> SugarChanged;
        public event EventHandler MilkToggled;
        public event EventHandler SyrupChanged;
        public event EventHandler PaymentRequested;
        public event EventHandler CancelRequested;

        public OrderOptionsControl()
        {
            InitializeComponent();
            ShowState(null, MachineState.Choosing, false);
            CompleteLayout();
        }

        public void ShowState(CoffeeOrder order, MachineState state, bool administrator)
        {
            _order = order;
            _state = state;
            _administrator = administrator;
            bool choosing = !administrator && state == MachineState.Choosing;
            bool paying = !administrator && state == MachineState.Paying;
            bool editable = choosing && order != null;
            summaryLabel.Text = order?.Beverage.Name ?? "Найдите свой любимый";
            totalLabel.Text = order == null ? "— ₽" : $"{order.Total:0.##} ₽";
            SetDisplayFont(totalLabel, order?.Total >= 1000 ? UiStyle.CompactPrice : UiStyle.Title);
            sugarLabel.Text = $"Сахар {order?.Sugar ?? 0} / 4";
            minusButton.Enabled = editable && order.Sugar > 0;
            plusButton.Enabled = editable && order.Sugar < 4;
            milkButton.Enabled = syrupButton.Enabled = editable;
            milkButton.Selected = order?.ExtraMilk ?? false;
            milkButton.Text = order?.ExtraMilk == true ? "✓ Молоко +30 ₽" : "Молоко +30 ₽";
            syrupButton.Text = "Сироп: " + (order?.Syrup.ToLower() ?? "без сиропа");
            syrupButton.Selected = order != null && order.Syrup != "Без сиропа";
            paymentButton.Enabled = editable;
            paymentButton.Text = state switch
            {
                MachineState.Paying => "Оплатите на терминале →",
                MachineState.Brewing => "Готовим ваш кофе…",
                MachineState.Ready => "Готово · заберите внизу ↓",
                _ => order == null ? "Выберите напиток" : $"К оплате · {order.Total:0.##} ₽ →"
            };
            cancelButton.Enabled = editable || paying;
            cancelButton.Text = paying ? "← Назад" : "Сбросить";
        }

        private bool CanEditOrder() => !_administrator && _state == MachineState.Choosing && _order != null;

        private void MinusButton_Click(object sender, EventArgs e)
        {
            if (CanEditOrder() && _order.Sugar > 0) SugarChanged?.Invoke(this, -1);
        }
        private void PlusButton_Click(object sender, EventArgs e)
        {
            if (CanEditOrder() && _order.Sugar < 4) SugarChanged?.Invoke(this, 1);
        }
        private void MilkButton_Click(object sender, EventArgs e)
        {
            if (CanEditOrder()) MilkToggled?.Invoke(this, EventArgs.Empty);
        }
        private void SyrupButton_Click(object sender, EventArgs e)
        {
            if (CanEditOrder()) SyrupChanged?.Invoke(this, EventArgs.Empty);
        }
        private void PaymentButton_Click(object sender, EventArgs e)
        {
            if (CanEditOrder()) PaymentRequested?.Invoke(this, EventArgs.Empty);
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (!_administrator && (CanEditOrder() || _state == MachineState.Paying))
                CancelRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
