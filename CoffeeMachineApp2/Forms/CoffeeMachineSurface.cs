using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    // Дизайнер корневого UserControl выполняет отрисовку его базового класса.
    // Поэтому корпус виден и при редактировании, и при запуске.
    public class CoffeeMachineSurface : ScalableControl
    {
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Ready { get; set; }

        public CoffeeMachineSurface()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics; g.SmoothingMode = SmoothingMode.AntiAlias;
            var saved = g.Save(); g.ScaleTransform(Width / 640f, Height / 1000f);
            UiStyle.FillRound(g, new Rectangle(9, 9, 622, 984), Color.FromArgb(198, 197, 183), 30);
            using (var body = UiStyle.RoundedRect(new Rectangle(3, 1, 629, 990), 28))
            using (var gradient = new LinearGradientBrush(new Rectangle(3, 1, 629, 990), Color.FromArgb(51, 58, 48), Color.FromArgb(25, 32, 27), 0f))
            using (var edge = new Pen(Color.FromArgb(92, 102, 82))) { g.FillPath(gradient, body); g.DrawPath(edge, body); }
            using (var accent = new Pen(UiStyle.Gold, 2)) g.DrawLine(accent, 32, 83, 606, 83);
            using (var led = new SolidBrush(Ready ? UiStyle.Green : UiStyle.Gold)) g.FillEllipse(led, 601, 74, 6, 6);
            g.Restore(saved);
            var selectionModule = Controls["selectionModule"];
            if (selectionModule != null)
            {
                int x = LogicalToDeviceUnits(4), top = LogicalToDeviceUnits(9), bottom = LogicalToDeviceUnits(6);
                Rectangle frame = selectionModule.Bounds;
                frame = new Rectangle(frame.X - x, frame.Y - top, frame.Width + 2 * x, frame.Height + top + bottom);
                UiStyle.FillRound(g, frame, Color.FromArgb(12, 20, 16), LogicalToDeviceUnits(23));
            }
        }
    }
}
