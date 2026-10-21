using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Кнопка-купюра: раскрашена по номиналу, чтобы выглядеть как деньги.</summary>
    public class MoneyButton : Control
    {
        private bool _hover;
        private bool _down;
        private int _amount = 10;

        public MoneyButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Cursor = Cursors.Hand;
            Size = new Size(70, 48);
        }

        [Category("Money"), DefaultValue(10)]
        public int Amount
        {
            get { return _amount; }
            set { _amount = value; Invalidate(); }
        }

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }

        private static Color ColorFor(int amount)
        {
            if (amount <= 10) return Color.FromArgb(168, 204, 142);
            if (amount <= 50) return Color.FromArgb(142, 186, 224);
            if (amount <= 100) return Color.FromArgb(238, 194, 134);
            return Color.FromArgb(192, 166, 224);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(UiStyle.BackgroundOf(this));

            Color baseColor = Enabled ? ColorFor(_amount) : Color.FromArgb(225, 222, 218);
            if (_hover && Enabled) baseColor = ControlPaint.Light(baseColor, 0.25f);

            int offset = _down ? 2 : 0;
            Rectangle body = new Rectangle(1, 2 + offset, Width - 3, Height - 7);

            // тень под купюрой
            using (var shadow = new SolidBrush(Color.FromArgb(40, 80, 60, 40)))
            using (GraphicsPath sp = UiStyle.RoundedRect(new Rectangle(body.X + 1, body.Y + 3, body.Width, body.Height), 7))
                g.FillPath(shadow, sp);

            using (GraphicsPath path = UiStyle.RoundedRect(body, 7))
            using (var brush = new LinearGradientBrush(body, ControlPaint.Light(baseColor, 0.35f), baseColor, 20f))
            using (var pen = new Pen(ControlPaint.Dark(baseColor, 0.12f)))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            // внутренняя рамка и узоры
            Rectangle inner = Rectangle.Inflate(body, -5, -5);
            using (var pen = new Pen(Color.FromArgb(150, 255, 255, 255), 1.4f))
            {
                g.DrawRectangle(pen, inner);
                int d = Math.Max(8, body.Height / 3);
                g.DrawEllipse(pen, inner.X + 3, body.Y + (body.Height - d) / 2, d, d);
                g.DrawEllipse(pen, inner.Right - 3 - d, body.Y + (body.Height - d) / 2, d, d);
            }

            TextRenderer.DrawText(g, _amount + " ₽", UiStyle.Bold, body,
                Enabled ? UiStyle.DarkBrown : Color.FromArgb(170, 164, 158),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
