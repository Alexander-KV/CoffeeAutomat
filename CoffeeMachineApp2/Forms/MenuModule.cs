using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Модуль 1: сетка 3×3 с карточками напитков.</summary>
    public class MenuModule : MachineModule
    {
        private readonly TableLayoutPanel _grid;
        private readonly List<BeverageCard> _cards = new List<BeverageCard>();

        public MenuModule()
        {
            Title = "1 · Выберите напиток";

            _grid = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 3,
                Margin = Padding.Empty
            };
            for (int i = 0; i < 3; i++)
            {
                _grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
                _grid.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
            }
            Controls.Add(_grid);
        }

        protected override bool IsActiveIn(MachineState state)
        {
            return state == MachineState.Waiting || state == MachineState.Configuring;
        }

        protected override void OnMachineAttached()
        {
            _grid.Controls.Clear();
            _cards.Clear();

            for (int i = 0; i < Machine.Menu.Count; i++)
            {
                var card = new BeverageCard(Machine.Menu[i]);
                card.Dock = DockStyle.Fill;
                card.Margin = new Padding(4);
                card.Selected += OnCardSelected;
                _grid.Controls.Add(card, i % 3, i / 3);
                _cards.Add(card);
            }
        }

        private void OnCardSelected(object sender, EventArgs e)
        {
            Beverage beverage = ((BeverageCard)sender).Beverage;
            Run(() => Machine.SelectBeverage(beverage));
        }

        protected override void OnRefreshView()
        {
            foreach (BeverageCard card in _cards)
            {
                card.Chosen = card.Beverage == Machine.SelectedBeverage;
                card.RefreshView();
                card.Enabled = Machine.CanEditOrder;   // после оплаты меню блокируется
            }
        }
    }
}
