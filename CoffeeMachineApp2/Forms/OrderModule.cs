using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Модуль 2: "Ваш заказ". Один напиток за заказ, поэтому настройка и заказ - в одном месте:
    /// фото выбранного напитка, сахар, добавки, итоговая цена и кнопка "К оплате".
    /// Пока настраиваем - всё редактируется; после перехода к оплате модуль показывает
    /// заказ "только для чтения".
    /// </summary>
    public class OrderModule : MachineModule
    {
        private readonly TableLayoutPanel _content;
        private readonly Label _lblHint;
        private readonly PictureBox _picture;
        private readonly Label _lblName;
        private readonly Label _lblInfo;
        private readonly Label _lblSugar;
        private readonly Label _lblTotal;
        private readonly RoundedButton _btnMinus;
        private readonly RoundedButton _btnPlus;
        private readonly RoundedButton _btnCancel;
        private readonly RoundedButton _btnPay;
        private readonly TableLayoutPanel _buttonsRow;
        private readonly Panel _additivesPanel;
        private readonly List<CheckBox> _boxes = new List<CheckBox>();
        private bool _syncing;

        public OrderModule()
        {
            Title = "2 · Ваш заказ";

            // подсказка, пока напиток не выбран
            _lblHint = UiStyle.CreateLabel("Выберите напиток\nв меню слева", UiStyle.Big, UiStyle.Gray, ContentAlignment.MiddleCenter);

            // --- слева: фото, название, объём и цена ---
            _picture = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            _lblName = UiStyle.CreateLabel("", UiStyle.Big, UiStyle.DarkBrown, ContentAlignment.MiddleCenter);
            _lblInfo = UiStyle.CreateLabel("", UiStyle.Regular, UiStyle.Gray, ContentAlignment.TopCenter);
            TableLayoutPanel left = UiStyle.CreateStack(0, 30, 40);
            left.Controls.Add(_picture, 0, 0);
            left.Controls.Add(_lblName, 0, 1);
            left.Controls.Add(_lblInfo, 0, 2);

            // --- справа: сахар, добавки, итог, кнопки ---
            Label sugarTitle = UiStyle.CreateLabel("Сахар (до 2 ложек бесплатно)", UiStyle.Bold, UiStyle.DarkBrown, ContentAlignment.BottomLeft);
            _btnMinus = UiStyle.CreateButton("−", false);
            _btnPlus = UiStyle.CreateButton("+", false);
            _lblSugar = UiStyle.CreateLabel("", UiStyle.Bold, UiStyle.DarkBrown, ContentAlignment.MiddleCenter);
            TableLayoutPanel sugarRow = UiStyle.CreateColumns(48, 110, 48, 0);
            sugarRow.Controls.Add(_btnMinus, 0, 0);
            sugarRow.Controls.Add(_lblSugar, 1, 0);
            sugarRow.Controls.Add(_btnPlus, 2, 0);

            Label additivesTitle = UiStyle.CreateLabel("Добавки", UiStyle.Bold, UiStyle.DarkBrown, ContentAlignment.BottomLeft);
            _additivesPanel = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };

            _lblTotal = UiStyle.CreateLabel("", UiStyle.Big, UiStyle.Brown, ContentAlignment.MiddleLeft);

            _btnCancel = UiStyle.CreateButton("Отмена", false);
            _btnPay = UiStyle.CreateButton("К оплате", true);
            _buttonsRow = UiStyle.CreateColumns(120, 0);
            _buttonsRow.Controls.Add(_btnCancel, 0, 0);
            _buttonsRow.Controls.Add(_btnPay, 1, 0);

            TableLayoutPanel right = UiStyle.CreateStack(26, 38, 30, 0, 34, 46);
            right.Controls.Add(sugarTitle, 0, 0);
            right.Controls.Add(sugarRow, 0, 1);
            right.Controls.Add(additivesTitle, 0, 2);
            right.Controls.Add(_additivesPanel, 0, 3);
            right.Controls.Add(_lblTotal, 0, 4);
            right.Controls.Add(_buttonsRow, 0, 5);

            var rightHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 0, 0, 0), Margin = Padding.Empty };
            rightHost.Controls.Add(right);

            _content = UiStyle.CreateColumns(210, 0);
            _content.Controls.Add(left, 0, 0);
            _content.Controls.Add(rightHost, 1, 0);

            Controls.Add(_content);
            Controls.Add(_lblHint);

            _btnMinus.Click += (s, e) => Run(() => Machine.ChangeSugar(-1));
            _btnPlus.Click += (s, e) => Run(() => Machine.ChangeSugar(+1));
            _btnCancel.Click += (s, e) => Run(() => Machine.CancelSelection());
            _btnPay.Click += (s, e) => Run(() => Machine.GoToPayment());
        }

        protected override bool IsActiveIn(MachineState state)
        {
            return state == MachineState.Configuring;
        }

        protected override void OnMachineAttached()
        {
            _additivesPanel.Controls.Clear();
            _boxes.Clear();

            for (int i = 0; i < Machine.Additives.Count; i++)
            {
                Additive additive = Machine.Additives[i];
                var box = new CheckBox
                {
                    Text = additive.ToString(),
                    Font = UiStyle.Regular,
                    ForeColor = UiStyle.DarkBrown,
                    Cursor = Cursors.Hand,
                    Height = 28,
                    Dock = DockStyle.Top
                };
                box.CheckedChanged += (s, e) =>
                {
                    if (_syncing) return;
                    Run(() => Machine.SetAdditive(additive, box.Checked));
                };
                _boxes.Add(box);
            }

            // Dock=Top: последний добавленный элемент оказывается сверху, поэтому добавляем с конца
            for (int i = _boxes.Count - 1; i >= 0; i--)
                _additivesPanel.Controls.Add(_boxes[i]);
        }

        protected override void OnRefreshView()
        {
            MachineState state = Machine.State;
            bool hasDrink = state != MachineState.Waiting;
            _content.Visible = hasDrink;
            _lblHint.Visible = !hasDrink;
            if (!hasDrink)
                return;

            // пока настраиваем - показываем черновик, после оплаты - готовый заказ (только чтение)
            bool editable = state == MachineState.Configuring;
            OrderItem item = editable ? Machine.BuildDraftItem() : Machine.Order.Items[0];
            Beverage beverage = item.Beverage;

            _picture.Image = ImageLoader.Load(beverage);
            _lblName.Text = beverage.Name;
            _lblInfo.Text = beverage.VolumeMl + " мл\nбазовая цена " + beverage.GetPrice().ToString("0") + " ₽";

            _lblSugar.Text = item.Sugar.ToString();
            _btnMinus.Enabled = editable && item.Sugar.Spoons > 0;
            _btnPlus.Enabled = editable && item.Sugar.Spoons < Sugar.MaxSpoons;

            _syncing = true;
            for (int i = 0; i < _boxes.Count; i++)
            {
                _boxes[i].Checked = item.Additives.Contains(Machine.Additives[i]);
                _boxes[i].Enabled = editable;
            }
            _syncing = false;

            _lblTotal.Text = "Итого: " + item.GetPrice().ToString("0") + " ₽";   // цену считает сама позиция
            _buttonsRow.Visible = editable;
        }
    }
}
