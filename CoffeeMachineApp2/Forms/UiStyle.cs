using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    internal static class UiStyle
    {
        public static readonly Color Ink = Color.FromArgb(36, 43, 36);
        public static readonly Color Forest = Color.FromArgb(41, 79, 57);
        public static readonly Color Gold = Color.FromArgb(188, 151, 91);
        public static readonly Color Ivory = Color.FromArgb(245, 241, 228);
        public static readonly Color Line = Color.FromArgb(226, 225, 215);
        public static readonly Color Brown = Color.FromArgb(143, 100, 62);
        public static readonly Color DarkBrown = Ink;
        public static readonly Color Cream = Color.FromArgb(248, 247, 240);
        public static readonly Color Gray = Color.FromArgb(120, 126, 113);
        public static readonly Color Green = Color.FromArgb(98, 168, 118);
        public static readonly Color Red = Color.FromArgb(185, 75, 56);
        public static readonly Font Regular = new Font("Segoe UI", 9.5F);
        public static readonly Font Bold = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        public static readonly Font Small = new Font("Segoe UI", 8F);
        public static readonly Font Title = new Font("Segoe UI", 22F, FontStyle.Bold);
        public static readonly Font CompactPrice = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font Display = new Font("Georgia", 23F);
        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            var p = new GraphicsPath();
            p.AddArc(r.X, r.Y, d, d, 180, 90); p.AddArc(r.Right-d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right-d, r.Bottom-d, d, d, 0, 90); p.AddArc(r.X, r.Bottom-d, d, d, 90, 90);
            p.CloseFigure(); return p;
        }
        public static void FillRound(Graphics g, Rectangle rect, Color color, int radius = 16)
        { using var path = RoundedRect(rect, radius); using var brush = new SolidBrush(color); g.FillPath(brush, path); }
        public static void Round(Control control, int radius = 16)
        {
            void Apply(object sender, EventArgs e)
            {
                if (control.Width <= 2 || control.Height <= 2) return;
                using var path = RoundedRect(control.ClientRectangle, radius * control.DeviceDpi / 96);
                var old = control.Region; control.Region = new Region(path); old?.Dispose();
            }
            control.Resize += Apply; Apply(control, EventArgs.Empty);
        }
        public static Color BackgroundOf(Control c)
        {
            for (var p = c.Parent; p != null; p = p.Parent) if (p.BackColor.A == 255) return p.BackColor;
            return Color.White;
        }
        public static Label Label(string text, int height = 26, bool bold = false)
            => new Label { Text = text, Height = height, Dock = DockStyle.Top, AutoEllipsis = true,
                TextAlign = ContentAlignment.MiddleLeft, ForeColor = Ink, Font = bold ? Bold : Regular };
        public static RoundedButton Button(string text, EventHandler click, bool primary = false)
        {
            var b = new RoundedButton { Text = text, Primary = primary, Height = 38, Width = 150, Margin = new Padding(3), TabStop = true };
            b.Click += click; return b;
        }
    }
}
