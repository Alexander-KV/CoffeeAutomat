using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Что сейчас происходит с оплатой картой.</summary>
    public enum CardPhase
    {
        Idle,        // ждём карту
        Processing,  // связь с банком
        Approved,    // оплата прошла
        Declined     // отказ
    }

    /// <summary>Картинка платёжного терминала: банковская карта, волны NFC, спиннер и статус.</summary>
    public class CardTerminalView : Control
    {
        private int _tick;
        private CardPhase _phase = CardPhase.Idle;

        public CardTerminalView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(260, 170);
            CardNumber = "•••• •••• •••• 4242";
            Holder = "COFFEE LOVER";
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CardPhase Phase
        {
            get { return _phase; }
            set { _phase = value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string CardNumber { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Holder { get; set; }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public decimal Balance { get; set; }

        /// <summary>Шаг анимации (вызывается таймером модуля).</summary>
        public void Advance()
        {
            _tick++;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(UiStyle.BackgroundOf(this));

            float statusH = 26f;
            float availH = Height - statusH - 6f;
            if (availH < 40f || Width < 80)
                return;

            float cardH = Math.Min(availH, (Width - 24f) / 1.586f);
            float cardW = cardH * 1.586f;
            float x = (Width - cardW) / 2f;
            float dy = 0f;
            if (_phase == CardPhase.Idle) dy = (float)Math.Sin(_tick * 0.12) * 2.5f;
            if (_phase == CardPhase.Processing) dy = (float)Math.Sin(_tick * 0.35) * 3f;
            float y = 3f + (availH - cardH) / 2f + dy;

            DrawCard(g, x, y, cardW, cardH);
            DrawStatus(g, availH + 6f);
        }

        private void DrawCard(Graphics g, float x, float y, float w, float h)
        {
            RectangleF rect = new RectangleF(x, y, w, h);
            int radius = (int)(h * 0.10f);

            // тень
            using (var shadow = new SolidBrush(Color.FromArgb(40, 60, 40, 30)))
            using (GraphicsPath sp = UiStyle.RoundedRect(new Rectangle((int)x + 2, (int)y + 5, (int)w, (int)h), radius))
                g.FillPath(shadow, sp);

            using (GraphicsPath path = UiStyle.RoundedRect(Rectangle.Round(rect), radius))
            using (var brush = new LinearGradientBrush(rect, Color.FromArgb(82, 56, 40), Color.FromArgb(170, 120, 82), 35f))
            {
                g.FillPath(brush, path);

                // блик
                g.SetClip(path);
                using (var gloss = new SolidBrush(Color.FromArgb(26, 255, 255, 255)))
                    g.FillEllipse(gloss, x - w * 0.25f, y - h * 0.95f, w * 1.2f, h * 1.4f);
                g.ResetClip();
            }

            // чип
            RectangleF chip = new RectangleF(x + w * 0.08f, y + h * 0.36f, w * 0.17f, h * 0.20f);
            using (GraphicsPath cp = UiStyle.RoundedRect(Rectangle.Round(chip), 4))
            using (var chipBrush = new LinearGradientBrush(chip, Color.FromArgb(246, 222, 150), Color.FromArgb(206, 164, 78), 45f))
            using (var chipPen = new Pen(Color.FromArgb(150, 120, 60), 1f))
            {
                g.FillPath(chipBrush, cp);
                g.DrawPath(chipPen, cp);
                g.DrawLine(chipPen, chip.X, chip.Y + chip.Height / 2, chip.Right, chip.Y + chip.Height / 2);
                g.DrawLine(chipPen, chip.X + chip.Width / 2, chip.Y, chip.X + chip.Width / 2, chip.Bottom);
            }

            // волны бесконтактной оплаты
            float cx = x + w * 0.86f;
            float cy = y + h * 0.22f;
            for (int i = 1; i <= 3; i++)
            {
                int alpha = 90 + (int)(110 * Math.Max(0.0, Math.Sin(_tick * 0.22 - i * 0.8)));
                float r = h * 0.045f * i;
                using (var pen = new Pen(Color.FromArgb(Math.Min(255, alpha), 255, 255, 255), 2f))
                    g.DrawArc(pen, cx - r, cy - r, r * 2, r * 2, -45, 90);
            }

            // надписи
            using (var small = new Font("Segoe UI", Math.Max(7f, h * 0.075f), FontStyle.Bold, GraphicsUnit.Pixel))
            using (var number = new Font("Segoe UI Semibold", Math.Max(8f, h * 0.105f), FontStyle.Regular, GraphicsUnit.Pixel))
            using (var white = new SolidBrush(Color.White))
            using (var soft = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
            {
                g.DrawString("COFFEE BANK", small, soft, x + w * 0.08f, y + h * 0.10f);
                g.DrawString(CardNumber, number, white, x + w * 0.08f, y + h * 0.62f);
                g.DrawString(Holder, small, soft, x + w * 0.08f, y + h * 0.84f);
                var right = new StringFormat { Alignment = StringAlignment.Far };
                g.DrawString("Баланс " + Balance.ToString("0") + " ₽", small, soft,
                    new RectangleF(x, y + h * 0.84f, w - w * 0.08f, h * 0.12f), right);
                right.Dispose();
            }
        }

        private void DrawStatus(Graphics g, float y)
        {
            string text;
            Color color;
            switch (_phase)
            {
                case CardPhase.Processing: text = "Связь с банком…"; color = UiStyle.Brown; break;
                case CardPhase.Approved: text = "Оплата одобрена"; color = UiStyle.Green; break;
                case CardPhase.Declined: text = "Недостаточно средств на карте"; color = UiStyle.Red; break;
                default: text = "Приложите карту к терминалу"; color = UiStyle.Gray; break;
            }

            SizeF size = g.MeasureString(text, UiStyle.Bold);
            float textX = (Width - size.Width) / 2f + 10f;
            float midY = y + 11f;

            using (var brush = new SolidBrush(color))
                g.DrawString(text, UiStyle.Bold, brush, textX, y + 1f);

            float ix = textX - 14f;
            if (_phase == CardPhase.Processing)
            {
                using (var pen = new Pen(UiStyle.Brown, 2.5f))
                    g.DrawArc(pen, ix - 7, midY - 7, 14, 14, _tick * 14, 270);
            }
            else if (_phase == CardPhase.Approved)
            {
                using (var fill = new SolidBrush(UiStyle.Green))
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.FillEllipse(fill, ix - 8, midY - 8, 16, 16);
                    g.DrawLines(pen, new[] { new PointF(ix - 4, midY), new PointF(ix - 1, midY + 3), new PointF(ix + 4, midY - 3) });
                }
            }
            else if (_phase == CardPhase.Declined)
            {
                using (var fill = new SolidBrush(UiStyle.Red))
                using (var pen = new Pen(Color.White, 2f))
                {
                    g.FillEllipse(fill, ix - 8, midY - 8, 16, 16);
                    g.DrawLine(pen, ix - 3, midY - 3, ix + 3, midY + 3);
                    g.DrawLine(pen, ix + 3, midY - 3, ix - 3, midY + 3);
                }
            }
        }
    }
}
