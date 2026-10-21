using System;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Весь интерфейс автомата как ОДИН элемент (UserControl), а не форма.
    /// Его можно положить в любое окно. Расположение модулей (меню, заказ,
    /// оплата, приготовление) задаётся в дизайнере: файл CoffeeMachineControl.Designer.cs.
    ///
    /// Этот класс - "хозяин": он создаёт автомат (модель), передаёт его всем модулям,
    /// слушает их события и просит всех обновиться. Логики автомата здесь нет.
    /// </summary>
    public partial class CoffeeMachineControl : UserControl
    {
        private readonly CoffeeMachine _machine = new CoffeeMachine();
        private readonly MachineModule[] _modules;
        private readonly Timer _notifyTimer = new Timer { Interval = 4000 };

        public CoffeeMachineControl()
        {
            InitializeComponent();

            // Любой модуль, который лежит на форме, добавляется сюда.
            // Если в дизайнере добавить новый модуль - допишите его в этот список.
            _modules = new MachineModule[]
            {
                menuModule,
                orderModule,
                paymentModule,
                preparationModule
            };

            foreach (MachineModule module in _modules)
            {
                module.Machine = _machine;
                module.Changed += OnModuleChanged;
                module.Notification += OnModuleNotification;
            }

            _notifyTimer.Tick += (s, e) => { _notifyTimer.Stop(); lblNotify.Text = ""; };
            RefreshAll();
        }

        private void OnModuleChanged(object sender, EventArgs e)
        {
            RefreshAll();
        }

        private void OnModuleNotification(object sender, NotificationEventArgs e)
        {
            lblNotify.ForeColor = e.IsError ? UiStyle.Red : UiStyle.Green;
            lblNotify.Text = e.Message;
            _notifyTimer.Stop();
            _notifyTimer.Start();
        }

        /// <summary>Попросить все модули перерисоваться по текущему состоянию автомата.</summary>
        private void RefreshAll()
        {
            foreach (MachineModule module in _modules)
                module.RefreshView();   // полиморфный вызов: каждый модуль обновляется по-своему

            lblState.Text = "Состояние: " + StateToText(_machine.State);
            btnAdmin.Enabled = _machine.State == MachineState.Waiting;
        }

        private void btnAdmin_Click(object sender, EventArgs e)
        {
            using (var admin = new AdminForm(_machine.Menu))
                admin.ShowDialog(this);

            RefreshAll();   // цены и доступность могли измениться
        }

        private static string StateToText(MachineState state)
        {
            switch (state)
            {
                case MachineState.Waiting: return "Ожидание";
                case MachineState.Configuring: return "Настройка заказа";
                case MachineState.Payment: return "Оплата";
                case MachineState.Preparing: return "Приготовление";
                case MachineState.Ready: return "Заказ готов";
                default: return "";
            }
        }
    }
}
