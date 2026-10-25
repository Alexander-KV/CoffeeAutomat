using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Карта действительно заезжает в прорезь. Button сохраняет управление Enter/Space.</summary>
    public sealed class CardSlotView : Button
    {
        private readonly Timer _animation = new Timer { Interval = 16 };
        private double _position, _target;
        public bool IsAnimating => Math.Abs(_position - _target) > .005;
        public event EventHandler AnimationFinished;
        public bool Inserted
        {
            get => _target == 1;
            set
            {
                double target = value ? 1 : 0;
                if (_target == target) return;
                _target = target; _animation.Start(); Invalidate();
            }
        }
        public CardSlotView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; Cursor = Cursors.Hand;
            AccessibleName = "Вставить или забрать карту";
            _animation.Tick += (s,e) =>
            {
                _position += (_target - _position) * .16;
                if (!IsAnimating) { _position = _target; _animation.Stop(); AnimationFinished?.Invoke(this, EventArgs.Empty); }
                Invalidate();
            };
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.Clear(UiStyle.BackgroundOf(this)); g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save(); g.ScaleTransform(Width / 136f, Height / 132f);
            UiStyle.FillRound(g, new Rectangle(7, 12, 122, 15), Color.FromArgb(12, 19, 14), 6);
            using (var slot = new Pen(Inserted ? UiStyle.Green : UiStyle.Gold, 2)) g.DrawLine(slot, 14, 17, 122, 17);
            var clip = g.Save(); g.SetClip(new Rectangle(0, 24, 136, 87));
            float top = 37 - (float)_position * 62;
            var cardState = g.Save(); g.TranslateTransform(16, top);
            using (var path = UiStyle.RoundedRect(new Rectangle(0, 0, 104, 67), 8))
            using (var color = new LinearGradientBrush(new Rectangle(0, 0, 104, 67), Color.FromArgb(196, 160, 99), Color.FromArgb(91, 74, 48), 35f)) g.FillPath(color, path);
            using (var f = new Font("Segoe UI", 9.3f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var white = new SolidBrush(UiStyle.Ivory))
            {
                g.DrawString("BEAN & CUP", f, white, 8, 6);
                g.DrawString("••••  4242", f, white, 8, 47);
                g.DrawString("∞", f, white, 82, 47);
            }
            UiStyle.FillRound(g, new Rectangle(11, 26, 18, 14), Color.FromArgb(235, 209, 145), 3);
            using (var chip = new Pen(Color.FromArgb(149, 116, 62)))
            { g.DrawLine(chip, 11, 33, 29, 33); g.DrawLine(chip, 20, 26, 20, 40); }
            using (var wave = new Pen(UiStyle.Ivory, 1.2f)) for (int i = 0; i < 3; i++) g.DrawArc(wave, 39 + i * 4, 26 - i * 3, 5 + i * 3, 13 + i * 6, -70, 140);
            g.Restore(cardState); g.Restore(clip);
            using (var font = new Font("Segoe UI", 10.5f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var ink = new SolidBrush(Enabled ? UiStyle.Ivory : UiStyle.Gray))
            using (var format = new StringFormat { Alignment = StringAlignment.Center })
                g.DrawString(IsAnimating ? (Inserted ? "Читаем карту…" : "Возвращаем карту…") : Inserted ? "Нажмите: забрать карту" : "Нажмите: вставить карту", font, ink, new RectangleF(0, 113, 136, 18), format);
            g.Restore(saved);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -3, -3), UiStyle.Gold, BackColor);
        }
        protected override void Dispose(bool disposing) { if (disposing) _animation.Dispose(); base.Dispose(disposing); }
    }

    /// <summary>Лоток сдачи: номиналы высыпаются после оплаты и исчезают при получении.</summary>
    public sealed class ChangeTrayView : Button
    {
        private readonly Timer _animation = new Timer { Interval = 25 };
        private decimal _amount, _lastAmount;
        private decimal[] _pieces = Array.Empty<decimal>();
        private float _phase = 1;
        private bool _collecting;
        public decimal Amount
        {
            get => _amount;
            set
            {
                if (_amount == value) return;
                _collecting = value == 0 && _amount > 0;
                if (value > 0) { _pieces = CashPayment.SplitChange(value).ToArray(); _lastAmount = value; }
                _amount = value; _phase = 0; _animation.Start();
                AccessibleName = value > 0 ? $"Забрать сдачу {value:0.##} рублей" : "Лоток сдачи пуст";
                Invalidate();
            }
        }
        public ChangeTrayView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            FlatStyle = FlatStyle.Flat; FlatAppearance.BorderSize = 0; Cursor = Cursors.Hand; AccessibleName = "Лоток сдачи пуст";
            _animation.Tick += (s,e) => { _phase = Math.Min(1, _phase + .035f); if (_phase >= 1) _animation.Stop(); Invalidate(); };
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics; g.Clear(Color.FromArgb(21, 27, 23)); g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save(); g.ScaleTransform(Width / 154f, Height / 134f);
            UiStyle.FillRound(g, new Rectangle(3, 2, 148, 129), Color.FromArgb(12, 19, 14), 12);
            using (var border = new Pen(_amount > 0 ? UiStyle.Gold : Color.FromArgb(53, 67, 54)))
            using (var path = UiStyle.RoundedRect(new Rectangle(3, 2, 148, 129), 12)) g.DrawPath(border, path);
            using var small = new Font("Segoe UI", 10.5f, FontStyle.Regular, GraphicsUnit.Pixel);
            using var ink = new SolidBrush(UiStyle.Ivory);
            using var muted = new SolidBrush(UiStyle.Gray);
            using var format = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("ЛОТОК СДАЧИ", small, muted, new RectangleF(3, 9, 148, 18), format);
            bool show = _amount > 0 || (_collecting && _phase < 1);
            if (show)
            {
                int count = Math.Min(_pieces.Length, 6);
                float opacity = _collecting ? 1 - _phase : 1;
                var clip = g.Save(); g.SetClip(new Rectangle(6, 28, 142, 70));
                for (int i = 0; i < count; i++)
                {
                    float arrive = Math.Clamp(_phase * 1.8f - i * .12f, 0, 1);
                    float offset = _collecting ? -_phase * 65 : -(1 - arrive) * 72;
                    int x = 13 + i % 3 * 42, y = (int)(34 + i / 3 * 28 + offset);
                    Color metal = Color.FromArgb((int)(opacity * 255), 210, 177, 105);
                    using var fill = new SolidBrush(metal);
                    if (_pieces[i] >= 50)
                    {
                        using var note = UiStyle.RoundedRect(new Rectangle(x - 3, y + 2, 39, 22), 3); g.FillPath(fill, note);
                    }
                    else g.FillEllipse(fill, x, y, 29, 29);
                    using var label = new SolidBrush(Color.FromArgb((int)(opacity * 255), 65, 51, 28));
                    g.DrawString(_pieces[i].ToString("0.##"), small, label, new RectangleF(x - 3, y + 5, 39, 20), format);
                }
                g.Restore(clip);
                if (_pieces.Length > 6) g.DrawString("+" + (_pieces.Length - 6), small, muted, 118, 77);
            }
            else g.DrawString(_collecting ? "✓ Получено" : "Пусто", small, _collecting ? ink : muted, new RectangleF(3, 50, 148, 22), format);
            string caption = _amount > 0 ? $"Забрать {_amount:0.##} ₽" : _collecting ? $"Выдано {_lastAmount:0.##} ₽" : "Сдача появится здесь";
            g.DrawString(caption, small, ink, new RectangleF(4, 105, 146, 20), format);
            g.Restore(saved);
            if (Focused && ShowFocusCues) ControlPaint.DrawFocusRectangle(g, Rectangle.Inflate(ClientRectangle, -5, -5), UiStyle.Gold, BackColor);
        }
        protected override void Dispose(bool disposing) { if (disposing) _animation.Dispose(); base.Dispose(disposing); }
    }
}
