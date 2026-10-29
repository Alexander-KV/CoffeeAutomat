using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CoffeeMachineApp.Models;
namespace CoffeeMachineApp.Forms.Modules
{
    public partial class CoffeeSelectionControl : ScalableControl
    {
        public event EventHandler<string> BeverageSelected;
        public event EventHandler<int> SugarChanged;
        public event EventHandler MilkToggled, SyrupChanged, PaymentRequested, CancelRequested;
        public CoffeeSelectionControl()
        {
            InitializeComponent();
            UiStyle.Round(this, 19);
            menuModule.BeverageSelected += (s,e) => BeverageSelected?.Invoke(this,e);
            orderModule.SugarChanged += (s,e) => SugarChanged?.Invoke(this,e);
            orderModule.MilkToggled += (s,e) => MilkToggled?.Invoke(this,e);
            orderModule.SyrupChanged += (s,e) => SyrupChanged?.Invoke(this,e);
            orderModule.PaymentRequested += (s,e) => PaymentRequested?.Invoke(this,e);
            orderModule.CancelRequested += (s,e) => CancelRequested?.Invoke(this,e);
            CompleteLayout();
        }
        public void SetMenu(IReadOnlyList<Beverage> menu) => menuModule.SetMenu(menu);
        public void ShowState(CoffeeOrder order, MachineState state, bool administrator)
        {
            menuModule.ShowState(order,state,administrator);
            orderModule.ShowState(order,state,administrator);
        }
    }
}
