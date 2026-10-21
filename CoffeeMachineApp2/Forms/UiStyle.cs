using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Общие цвета, шрифты и мелкие фабрики, чтобы не повторять код оформления.</summary>
    internal static class UiStyle
    {
        public static readonly Color Brown = Color.FromArgb(111, 78, 55);
        public static readonly Color DarkBrown = Color.FromArgb(59, 42, 32);
        public static readonly Color Cream = Color.FromArgb(245, 240, 234);
        public static readonly Color Line = Color.FromArgb(226, 218, 209);
        public static readonly Color Gray = Color.FromArgb(125, 117, 110);
        public static readonly Color Green = Color.FromArgb(46, 125, 50);
        public static readonly Color Red = Color.FromArgb(198, 40, 40);

        public static readonly Font Regular = new Font("Segoe UI", 10F);
        public static readonly Font Bold = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font Small = new Font("Segoe UI", 9F);
        public static readonly Font Big = new Font("Segoe UI", 13F, FontStyle.Bold);
        public static readonly Font Title = new Font("Segoe UI", 22F, FontStyle.Bold);

        public static Color ParseColor(string hex)
        {
            return ColorTranslator.FromHtml(hex);
        }

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = Math.Max(1, Math.Min(radius * 2, Math.Min(r.Width, r.Height)));
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Скруглить углы элемента.</summary>
        public static void ApplyRoundRegion(Control control, int radius)
        {
            if (control.Width < 4 || control.Height < 4)
                return;

            using (GraphicsPath path = RoundedRect(new Rectangle(0, 0, control.Width, control.Height), radius))
            {
                Region old = control.Region;
                control.Region = new Region(path);
                if (old != null) old.Dispose();
            }
        }

        /// <summary>Первый непрозрачный цвет фона у родителей (для сглаживания углов).</summary>
        public static Color BackgroundOf(Control control)
        {
            Control c = control.Parent;
            while (c != null)
            {
                if (c.BackColor.A == 255)
                    return c.BackColor;
                c = c.Parent;
            }
            return Color.White;
        }

        /// <summary>Перевести пиксели из "96 dpi" в реальные (для высоты строк списка).</summary>
        public static int Dpi(Control control, int pixels)
        {
            using (Graphics g = control.CreateGraphics())
                return (int)Math.Round(pixels * g.DpiY / 96f);
        }

        // ---------- фабрики элементов ----------

        public static Label CreateLabel(string text, Font font, Color color, ContentAlignment align)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                TextAlign = align,
                AutoSize = false,
                Dock = DockStyle.Fill,
                Margin = Padding.Empty
            };
        }

        /// <summary>Метка с абсолютными координатами (для простых окон).</summary>
        public static Label CreateLabelAt(string text, int x, int y, int width, int height, Font font, Color color)
        {
            return new Label
            {
                Text = text,
                Font = font,
                ForeColor = color,
                AutoSize = false,
                Bounds = new Rectangle(x, y, width, height)
            };
        }

        public static RoundedButton CreateButton(string text, bool primary)
        {
            return new RoundedButton
            {
                Text = text,
                Primary = primary,
                Dock = DockStyle.Fill,
                Margin = new Padding(3)
            };
        }

        /// <summary>
        /// Вертикальный "стек": одна колонка и строки заданной высоты.
        /// Высота 0 означает "занять всё оставшееся место".
        /// </summary>
        public static TableLayoutPanel CreateStack(params int[] rowHeights)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = rowHeights.Length,
                Margin = Padding.Empty
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int height in rowHeights)
                table.RowStyles.Add(height > 0 ? new RowStyle(SizeType.Absolute, height) : new RowStyle(SizeType.Percent, 100));
            return table;
        }

        /// <summary>Горизонтальный ряд колонок. Ширина 0 означает "поровну делить остаток".</summary>
        public static TableLayoutPanel CreateColumns(params int[] widths)
        {
            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = widths.Length,
                RowCount = 1,
                Margin = Padding.Empty
            };
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            foreach (int width in widths)
                table.ColumnStyles.Add(width > 0 ? new ColumnStyle(SizeType.Absolute, width) : new ColumnStyle(SizeType.Percent, 100));
            return table;
        }

        public static void SetRowHeight(TableLayoutPanel table, int row, int height)
        {
            table.RowStyles[row] = new RowStyle(SizeType.Absolute, height);
        }
    }
}
