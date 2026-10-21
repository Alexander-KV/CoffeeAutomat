using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Кнопка со скруглёнными углами, наведением и "нажатым" видом.</summary>
    public class RoundedButton : Button
    {
        private bool _hover;
        private bool _down;
        private bool _primary;
        private bool _selected;

        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat;
            Font = UiStyle.Bold;
            Cursor = Cursors.Hand;
            Size = new Size(110, 40);
        }

        /// <summary>Главная (коричневая) кнопка.</summary>
        [Category("Appearance"), DefaultValue(false)]
        public bool Primary
        {
            get { return _primary; }
            set { _primary = value; Invalidate(); }
        }

        /// <summary>Выбранная вкладка (переключатель).</summary>
        [Category("Appearance"), DefaultValue(false)]
        public bool Selected
        {
            get { return _selected; }
            set { _selected = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(UiStyle.BackgroundOf(this));

            Color fill, border, text;
            if (!Enabled)
            {
                fill = Color.FromArgb(238, 234, 230);
                border = Color.FromArgb(228, 223, 218);
                text = Color.FromArgb(170, 164, 158);
            }
            else if (_primary || _selected)
            {
                fill = _down ? Color.FromArgb(84, 58, 40) : (_hover ? Color.FromArgb(132, 94, 68) : UiStyle.Brown);
                border = fill;
                text = Color.White;
            }
            else
            {
                fill = _down ? Color.FromArgb(232, 222, 211) : (_hover ? Color.FromArgb(243, 235, 226) : Color.FromArgb(251, 248, 244));
                border = _hover ? UiStyle.Brown : Color.FromArgb(214, 204, 193);
                text = UiStyle.DarkBrown;
            }

            Rectangle r = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiStyle.RoundedRect(r, Math.Min(12, Height / 2)))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            TextRenderer.DrawText(g, Text, Font, ClientRectangle, text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
