using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Окно администратора: цены и включение/выключение напитков.</summary>
    public class AdminForm : Form
    {
        private const int FirstRowY = 84;
        private const int RowHeight = 42;

        public AdminForm(IReadOnlyList<Beverage> menu)
        {
            Text = "Режим администратора";
            Font = UiStyle.Regular;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(520, FirstRowY + menu.Count * RowHeight + 70);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = Color.White;

            Controls.Add(UiStyle.CreateLabelAt("Администратор", 20, 12, 400, 30, UiStyle.Big, UiStyle.Brown));
            Controls.Add(UiStyle.CreateLabelAt("Напиток", 20, 54, 200, 22, UiStyle.Bold, UiStyle.Gray));
            Controls.Add(UiStyle.CreateLabelAt("Цена, ₽", 250, 54, 100, 22, UiStyle.Bold, UiStyle.Gray));
            Controls.Add(UiStyle.CreateLabelAt("В продаже", 390, 54, 110, 22, UiStyle.Bold, UiStyle.Gray));

            for (int i = 0; i < menu.Count; i++)
                AddRow(menu[i], FirstRowY + i * RowHeight);

            RoundedButton close = UiStyle.CreateButton("Закрыть", true);
            close.Dock = DockStyle.None;
            close.Bounds = new Rectangle(360, FirstRowY + menu.Count * RowHeight + 12, 140, 42);
            close.Click += (s, e) => Close();
            Controls.Add(close);
        }

        private void AddRow(Beverage beverage, int y)
        {
            Label name = UiStyle.CreateLabelAt(beverage.Name, 20, y + 6, 220, 24, UiStyle.Regular, UiStyle.DarkBrown);

            var price = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 9999,
                Value = beverage.Price < 1 ? 1 : beverage.Price,
                Bounds = new Rectangle(250, y + 4, 100, 28)
            };
            // цена меняется прямо в объекте напитка - карточки и заказ подхватят её сами
            price.ValueChanged += (s, e) => beverage.Price = price.Value;

            var enabled = new CheckBox
            {
                Text = "Включён",
                Checked = beverage.IsEnabled,
                AutoSize = true,
                Location = new Point(390, y + 6),
                ForeColor = UiStyle.DarkBrown
            };
            enabled.CheckedChanged += (s, e) => beverage.IsEnabled = enabled.Checked;

            Controls.AddRange(new Control[] { name, price, enabled });
        }
    }
}
