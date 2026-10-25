using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Одна рисуемая кнопка для действий, переключателей и карточек меню.</summary>
    public class RoundedButton : Button
    {
        private bool _hover, _down, _primary, _selected;
        private string _detail = "", _price = "";
        private bool _dark;
        [DefaultValue(false)] public bool Primary { get => _primary; set { _primary = value; Invalidate(); } }
        [DefaultValue(false)] public bool Selected { get => _selected; set { _selected = value; Invalidate(); } }
        [DefaultValue(false)] public bool Dark { get => _dark; set { _dark = value; Invalidate(); } }
        [DefaultValue("")] public string Detail { get => _detail; set { _detail = value; Invalidate(); } }
        [DefaultValue("")] public string Price { get => _price; set { _price = value; Invalidate(); } }
        public RoundedButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0;
            Font = UiStyle.Bold; Cursor = Cursors.Hand; Size = new Size(110, 40);
        }
        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = _down = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { _down = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { _down = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnEnabledChanged(EventArgs e) { Invalidate(); base.OnEnabledChanged(e); }
        protected override void OnTextChanged(EventArgs e) { Invalidate(); base.OnTextChanged(e); }
        protected override void OnGotFocus(EventArgs e) { Invalidate(); base.OnGotFocus(e); }
        protected override void OnLostFocus(EventArgs e) { Invalidate(); base.OnLostFocus(e); }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Dark && Parent is CoffeeMachineControl
                ? (Top >= 782 && Top < 938 ? Color.FromArgb(21, 27, 23) : Color.FromArgb(29, 36, 29))
                : UiStyle.BackgroundOf(this));
            bool card = Image != null;
            Color fill = Dark ? Color.FromArgb(47, 49, 47) : Color.White;
            Color ink = Dark ? UiStyle.Ivory : UiStyle.Ink;
            Color border = Dark ? Color.FromArgb(68, 70, 64) : UiStyle.Line;
            if (Selected) { fill = card ? Color.FromArgb(249, 237, 216) : UiStyle.Forest; border = card ? UiStyle.Gold : UiStyle.Forest; ink = card ? UiStyle.Ink : Color.White; }
            else if (Primary) { fill = UiStyle.Forest; border = fill; ink = Color.White; }
            else if (_hover && Enabled) { fill = Dark ? Color.FromArgb(60, 63, 57) : Color.FromArgb(246, 241, 231); border = UiStyle.Gold; }
            if (_down && Enabled) fill = ControlPaint.Dark(fill, 0.08f);
            if (!Enabled) { fill = Dark ? Color.FromArgb(37, 40, 37) : Color.FromArgb(237, 235, 229); ink = Dark ? Color.FromArgb(122, 126, 115) : Color.FromArgb(155, 153, 145); border = Dark ? Color.FromArgb(49, 53, 48) : Color.FromArgb(225, 223, 215); }
            var r = new Rectangle(1, 1, Width - 3, Height - 3);
            using (var path = UiStyle.RoundedRect(r, card ? 13 : 10))
            using (var brush = new SolidBrush(fill))
            using (var pen = new Pen(border, Selected && card ? 2f : 1f)) { g.FillPath(brush, path); g.DrawPath(pen, path); }
            var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding;
            if (card)
            {
                float scale = Width / 122f;
                int imageWidth = (int)(66 * scale), imageHeight = (int)(44 * scale);
                var imageRect = new Rectangle((Width - imageWidth) / 2, (int)(4 * scale), imageWidth, imageHeight);
                if (Enabled) g.DrawImage(Image, imageRect); else ControlPaint.DrawImageDisabled(g, Image, imageRect.X, imageRect.Y, fill);
                using var nameFont = new Font(Font.FontFamily, Font.Size * 0.89f, FontStyle.Bold);
                using var detailFont = new Font(Font.FontFamily, Font.Size * 0.8f);
                int y = (int)(49 * scale);
                TextRenderer.DrawText(g, Text, nameFont, new Rectangle(5, y, Width - 10, (int)(19 * scale)), ink, flags);
                TextRenderer.DrawText(g, Enabled ? Detail + "  ·  " + Price : "Нет в продаже", detailFont,
                    new Rectangle(3, y + (int)(19 * scale), Width - 6, (int)(18 * scale)), Enabled ? UiStyle.Gray : ink, flags);
                if (Selected)
                {
                    using var marker = new SolidBrush(UiStyle.Forest);
                    g.FillEllipse(marker, Width - 22 * scale, 8 * scale, 14 * scale, 14 * scale);
                    using var tick = new Pen(Color.White, 1.6f * scale);
                    g.DrawLines(tick, new[] { new PointF(Width - 18 * scale, 15 * scale), new PointF(Width - 15 * scale, 18 * scale), new PointF(Width - 11 * scale, 12 * scale) });
                }
            }
            else TextRenderer.DrawText(g, Text, Font, new Rectangle(6, 2, Width - 12, Height - 4), ink, flags);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(r, -5, -5), ink, fill);
        }
    }
}
