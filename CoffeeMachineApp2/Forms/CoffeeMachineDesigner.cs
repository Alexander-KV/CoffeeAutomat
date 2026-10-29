using System.ComponentModel;
using Microsoft.DotNet.DesignTools.Designers;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Открывает три вложенных модуля для редактирования прямо на MainForm.</summary>
    public class CoffeeMachineDesigner : ParentControlDesigner
    {
        public override void Initialize(IComponent component)
        {
            base.Initialize(component);
            var machine = (CoffeeMachineControl)component;
            EnableDesignMode(machine.SelectionModule, nameof(machine.SelectionModule));
            EnableDesignMode(machine.PaymentModule, nameof(machine.PaymentModule));
            EnableDesignMode(machine.DispensingModule, nameof(machine.DispensingModule));
        }
    }
}
