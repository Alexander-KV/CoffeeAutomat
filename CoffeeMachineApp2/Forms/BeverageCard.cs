using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Карточка напитка: фото, название, объём и цена, кнопка "Выбрать". Растягивается под любой размер.</summary>
    public class BeverageCard : UserControl
    {
        private readonly Beverage _beverage;
        private readonly PictureBox _picture;
        private readonly Label _lblName;
        private readonly Label _lblInfo;
        private readonly RoundedButton _button;
        private bool _chosen;

        public event EventHandler Selected;

        public BeverageCard(Beverage beverage)
        {
            _beverage = beverage;
            DoubleBuffered = true;
            BackColor = Color.White;
            Cursor = Cursors.Hand;
            Padding = new Padding(8);
            Size = new Size(180, 210);

            _picture = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = ImageLoader.Load(beverage),
                BackColor = Color.White,
                Margin = Padding.Empty
            };
            _lblName = UiStyle.CreateLabel(beverage.Name, UiStyle.Bold, UiStyle.DarkBrown, ContentAlignment.MiddleCenter);
            _lblInfo = UiStyle.CreateLabel("", UiStyle.Regular, UiStyle.Gray, ContentAlignment.MiddleCenter);
            _button = UiStyle.CreateButton("Выбрать", true);

            TableLayoutPanel stack = UiStyle.CreateStack(0, 26, 22, 40);
            stack.Controls.Add(_picture, 0, 0);
            stack.Controls.Add(_lblName, 0, 1);
            stack.Controls.Add(_lblInfo, 0, 2);
            stack.Controls.Add(_button, 0, 3);
            Controls.Add(stack);

            // клик по любой части карточки = выбор напитка
            foreach (Control control in new Control[] { this, _picture, _lblName, _lblInfo, _button })
                control.Click += OnAnyClick;

            RefreshView();
        }

        public Beverage Beverage
        {
            get { return _beverage; }
        }

        /// <summary>Подсветить карточку выбранного напитка.</summary>
        public bool Chosen
        {
            get { return _chosen; }
            set
            {
                if (_chosen == value) return;
                _chosen = value;
                Invalidate();
            }
        }

        /// <summary>Обновить цену и доступность из объекта напитка.</summary>
        public void RefreshView()
        {
            if (_beverage.IsEnabled)
            {
                _lblInfo.Text = _beverage.VolumeMl + " мл · " + _beverage.GetPrice().ToString("0") + " ₽";
                _lblInfo.ForeColor = UiStyle.Gray;
            }
            else
            {
                _lblInfo.Text = "Временно недоступен";
                _lblInfo.ForeColor = UiStyle.Red;
            }
            _button.Text = _beverage.IsEnabled ? "Выбрать" : "Недоступно";
            _button.Enabled = _beverage.IsEnabled;
        }

        private void OnAnyClick(object sender, EventArgs e)
        {
            EventHandler handler = Selected;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UiStyle.ApplyRoundRegion(this, 12);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = UiStyle.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 12))
            using (var pen = new Pen(_chosen ? UiStyle.Brown : UiStyle.Line, _chosen ? 3f : 1f))
                e.Graphics.DrawPath(pen, path);
        }
    }
}
