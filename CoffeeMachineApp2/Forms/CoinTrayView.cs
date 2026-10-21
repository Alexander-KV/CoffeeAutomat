using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Лоток сдачи: показывает монеты и купюры, которые выдал автомат.</summary>
    public class CoinTrayView : Control
    {
        private IReadOnlyList<int> _coins = new List<int>();

        public CoinTrayView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(260, 56);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<int> Coins
        {
            get { return _coins; }
            set { _coins = value ?? new List<int>(); Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(UiStyle.BackgroundOf(this));

            Rectangle tray = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = UiStyle.RoundedRect(tray, 12))
            using (var brush = new LinearGradientBrush(tray, Color.FromArgb(104, 98, 92), Color.FromArgb(66, 62, 58), 90f))
                g.FillPath(brush, path);

            Rectangle slot = Rectangle.Inflate(tray, -6, -6);
            using (GraphicsPath slotPath = UiStyle.RoundedRect(slot, 9))
            using (var slotBrush = new SolidBrush(Color.FromArgb(40, 38, 36)))
                g.FillPath(slotBrush, slotPath);

            if (_coins.Count == 0)
                return;

            int d = Math.Min(Height - 16, 44);
            float step = d + 4;
            if (_coins.Count > 1)
                step = Math.Min(step, (slot.Width - 12 - d) / (float)(_coins.Count - 1));

            float total = d + step * (_coins.Count - 1);
            float x = slot.X + (slot.Width - total) / 2f;
            float y = slot.Y + (slot.Height - d) / 2f;

            for (int i = 0; i < _coins.Count; i++)
                DrawCoin(g, x + step * i, y, d, _coins[i]);
        }

        private static void DrawCoin(Graphics g, float x, float y, float d, int value)
        {
            Color light, dark;
            if (value >= 50) { light = Color.FromArgb(250, 222, 120); dark = Color.FromArgb(196, 148, 44); }
            else if (value >= 10) { light = Color.FromArgb(242, 208, 112); dark = Color.FromArgb(184, 138, 48); }
            else { light = Color.FromArgb(236, 236, 240); dark = Color.FromArgb(150, 150, 160); }

            RectangleF rect = new RectangleF(x, y, d, d);
            using (var shadow = new SolidBrush(Color.FromArgb(70, 0, 0, 0)))
                g.FillEllipse(shadow, x + 1.5f, y + 2.5f, d, d);
            using (var brush = new LinearGradientBrush(rect, light, dark, 45f))
                g.FillEllipse(brush, rect);
            using (var rim = new Pen(Color.FromArgb(120, 255, 255, 255), 1.5f))
                g.DrawEllipse(rim, x + d * 0.1f, y + d * 0.1f, d * 0.8f, d * 0.8f);

            using (var font = new Font("Segoe UI", Math.Max(8f, d * 0.36f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (var text = new SolidBrush(Color.FromArgb(90, 62, 24)))
            using (var fmt = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(value.ToString(), font, text, rect, fmt);
        }
    }
}
