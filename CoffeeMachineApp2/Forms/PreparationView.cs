using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    public enum PreparationMode
    {
        Idle,      // ничего не готовится
        Brewing,   // идёт приготовление
        Ready      // напиток готов
    }

    /// <summary>
    /// Анимация приготовления: носик автомата, струя, стакан, который наполняется,
    /// волна на поверхности, пузырьки, пар и полоска прогресса.
    /// Всё рисуется кодом и подстраивается под любой размер.
    /// </summary>
    public class PreparationView : Control
    {
        private int _tick;
        private PreparationMode _mode = PreparationMode.Idle;
        private double _progress;
        private Color _liquid = Color.FromArgb(74, 46, 27);
        private bool _hasMilk;

        public PreparationView()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            Size = new Size(260, 200);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public PreparationMode Mode
        {
            get { return _mode; }
            set { _mode = value; Invalidate(); }
        }

        /// <summary>Прогресс текущего напитка от 0 до 1.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double Progress
        {
            get { return _progress; }
            set { _progress = Math.Max(0.0, Math.Min(1.0, value)); Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color LiquidColor
        {
            get { return _liquid; }
            set { _liquid = value; Invalidate(); }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasMilk
        {
            get { return _hasMilk; }
            set { _hasMilk = value; Invalidate(); }
        }

        public void Advance()
        {
            _tick++;
            Invalidate();
        }

        private static Color Mix(Color a, Color b, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t));
            return Color.FromArgb(
                (int)(a.R + (b.R - a.R) * t),
                (int)(a.G + (b.G - a.G) * t),
                (int)(a.B + (b.B - a.B) * t));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            float w = Width, h = Height;
            if (w < 60 || h < 80)
                return;

            bool brewing = _mode == PreparationMode.Brewing;
            bool ready = _mode == PreparationMode.Ready;
            double level = ready ? 1.0 : (brewing ? _progress : 0.0);

            float gh = h * 0.58f;                          // высота стакана
            float gw = Math.Min(w * 0.42f, gh * 0.78f);    // ширина по верху
            float bw = gw * 0.78f;                         // ширина по низу
            float cx = w / 2f;
            float bottom = h * 0.88f;
            float top = bottom - gh;

            // Подсвеченный металлический поддон внутри тёмной ниши.
            using (var tray = new Pen(Color.FromArgb(78, 96, 75), 1.2f))
                for (int i = -4; i <= 4; i++)
                    g.DrawLine(tray, cx + i * gw * 0.18f, bottom + 4, cx + i * gw * 0.18f + 8, bottom + 10);

            // тень под стаканом
            using (var shadow = new SolidBrush(Color.FromArgb(40, 90, 60, 40)))
                g.FillEllipse(shadow, cx - gw * 0.62f, bottom - 3, gw * 1.24f, 12);

            // носик кофемашины
            float nozzleW = gw * 0.95f, nozzleH = h * 0.07f, nozzleY = h * 0.02f;
            RectangleF nozzle = new RectangleF(cx - nozzleW / 2, nozzleY, nozzleW, nozzleH);
            using (GraphicsPath np = UiStyle.RoundedRect(Rectangle.Round(nozzle), 7))
            using (var nb = new LinearGradientBrush(nozzle, Color.FromArgb(120, 116, 112), Color.FromArgb(62, 58, 54), 90f))
                g.FillPath(nb, np);
            float spoutY = nozzleY + nozzleH;
            RectangleF spout = new RectangleF(cx - 9, spoutY - 2, 18, h * 0.03f);
            using (var sb = new SolidBrush(Color.FromArgb(52, 48, 44)))
                g.FillRectangle(sb, spout);
            float spoutBottom = spout.Bottom;

            // стакан
            PointF[] outer = { new PointF(cx - gw / 2, top), new PointF(cx + gw / 2, top), new PointF(cx + bw / 2, bottom), new PointF(cx - bw / 2, bottom) };
            float inTop = top + 3, inBottom = bottom - 5;
            PointF[] inner = { new PointF(cx - gw / 2 + 4, inTop), new PointF(cx + gw / 2 - 4, inTop), new PointF(cx + bw / 2 - 4, inBottom), new PointF(cx - bw / 2 + 4, inBottom) };

            using (var tint = new SolidBrush(Color.FromArgb(55, 226, 236, 244)))
                g.FillPolygon(tint, outer);

            // жидкость
            float levelY = inBottom - (inBottom - inTop) * (float)level;
            Color current = _liquid;
            if (_hasMilk)
                current = Mix(Color.FromArgb(62, 38, 22), _liquid, (level - 0.35) / 0.4);   // сначала кофе, потом молоко

            if (level > 0.001)
            {
                using (var clip = new GraphicsPath())
                {
                    clip.AddPolygon(inner);
                    g.SetClip(clip);

                    float amp = brewing ? 2.4f : 0.8f;
                    var wave = new System.Collections.Generic.List<PointF>();
                    for (float x = cx - gw / 2; x <= cx + gw / 2 + 4; x += 4)
                        wave.Add(new PointF(x, levelY + (float)Math.Sin(x * 0.09 + _tick * 0.25) * amp));
                    var body = new System.Collections.Generic.List<PointF>(wave);
                    body.Add(new PointF(cx + gw / 2 + 4, bottom));
                    body.Add(new PointF(cx - gw / 2, bottom));

                    using (var lb = new LinearGradientBrush(new RectangleF(0, levelY - 2, w, Math.Max(4, inBottom - levelY + 4)),
                        Mix(current, Color.White, 0.18), Mix(current, Color.Black, 0.18), 90f))
                        g.FillPolygon(lb, body.ToArray());

                    // молочная пена сверху
                    if (_hasMilk && level > 0.8)
                    {
                        double foamK = Math.Min(1.0, (level - 0.8) / 0.2);
                        var foam = new System.Collections.Generic.List<PointF>(wave);
                        for (int i = wave.Count - 1; i >= 0; i--)
                            foam.Add(new PointF(wave[i].X, wave[i].Y + gh * 0.07f * (float)foamK));
                        using (var fb = new SolidBrush(Color.FromArgb((int)(255 * foamK), 248, 240, 226)))
                            g.FillPolygon(fb, foam.ToArray());
                    }

                    // пузырьки
                    if (brewing && level > 0.05)
                    {
                        float range = Math.Max(10f, inBottom - levelY);
                        using (var bubble = new SolidBrush(Color.FromArgb(70, 255, 255, 255)))
                            for (int k = 0; k < 5; k++)
                            {
                                float bx = cx - gw / 2 + 10 + (gw - 20) * (((k * 37) % 100) / 100f);
                                float by = inBottom - ((_tick * 1.6f + k * 29) % range);
                                float br = 2f + k % 3;
                                g.FillEllipse(bubble, bx - br, by - br, br * 2, br * 2);
                            }
                    }
                    g.ResetClip();
                }
            }

            // струя из носика
            if (brewing && _progress < 0.97)
            {
                float endY = level > 0.001 ? levelY : inBottom;
                using (var pen = new Pen(Mix(current, Color.Black, 0.1), 4f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    float wob = (float)Math.Sin(_tick * 0.5) * 0.6f;
                    g.DrawLine(pen, cx, spoutBottom, cx + wob, endY);
                }
            }

            // стенки и блики
            using (var wall = new Pen(Color.FromArgb(200, 196, 210, 220), 2.2f))
                g.DrawPolygon(wall, outer);
            using (var glare = new SolidBrush(Color.FromArgb(95, 255, 255, 255)))
                g.FillPolygon(glare, new[]
                {
                    new PointF(cx - gw / 2 + 10, top + 14), new PointF(cx - gw / 2 + 17, top + 14),
                    new PointF(cx - bw / 2 + 15, bottom - 18), new PointF(cx - bw / 2 + 8, bottom - 18)
                });

            // пар
            if (ready || (brewing && _progress > 0.12))
            {
                int alpha = ready ? 90 : (int)(90 * Math.Min(1.0, _progress * 2));
                using (var pen = new Pen(Color.FromArgb(alpha, 165, 165, 170), 3.5f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    for (int i = 0; i < 3; i++)
                    {
                        float baseX = cx + (i - 1) * gw * 0.26f;
                        var pts = new PointF[12];
                        for (int k = 0; k < 12; k++)
                        {
                            float t = k / 11f;
                            pts[k] = new PointF(baseX + (float)Math.Sin(t * 5 + _tick * 0.12 + i * 1.6) * 7 * (0.3f + t),
                                                top - 6 - t * gh * 0.36f);
                        }
                        g.DrawCurve(pen, pts);
                    }
                }
            }

            // значок "готово"
            if (ready)
            {
                float r = Math.Max(12f, Math.Min(w, h) * 0.07f);
                float bx = cx + gw * 0.62f, by = top + gh * 0.10f;
                using (var fill = new SolidBrush(UiStyle.Green))
                using (var pen = new Pen(Color.White, 2.6f))
                {
                    g.FillEllipse(fill, bx - r, by - r, r * 2, r * 2);
                    g.DrawLines(pen, new[] { new PointF(bx - r * 0.45f, by), new PointF(bx - r * 0.1f, by + r * 0.4f), new PointF(bx + r * 0.5f, by - r * 0.35f) });
                }
            }

            // полоска прогресса
            if (brewing)
            {
                RectangleF track = new RectangleF(w * 0.1f, h - 12, w * 0.8f, 7);
                using (GraphicsPath tp = UiStyle.RoundedRect(Rectangle.Round(track), 3))
                using (var tb = new SolidBrush(Color.FromArgb(50, 66, 52)))
                    g.FillPath(tb, tp);
                float fw = track.Width * (float)_progress;
                if (fw > 6)
                {
                    RectangleF fillRect = new RectangleF(track.X, track.Y, fw, track.Height);
                    using (GraphicsPath fp = UiStyle.RoundedRect(Rectangle.Round(fillRect), 3))
                    using (var fb = new SolidBrush(UiStyle.Gold))
                        g.FillPath(fb, fp);
                }
            }
        }
    }
}
