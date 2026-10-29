using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Масштабирует реальную раскладку Designer, включая шрифты и абсолютные строки таблиц.</summary>
    public class ScalableControl : UserControl
    {
        private Size _referenceSize;
        private Size _designSize;
        private List<LayoutItem> _layout;
        private bool _applying;
        private readonly Dictionary<Control, Font> _ownedFonts = new Dictionary<Control, Font>();

        public ScalableControl() { AutoScaleMode = AutoScaleMode.None; }

        // Родитель/DPI изменяет границы модуля; содержимым управляет только этот класс.
        protected override bool ScaleChildren => false;

        protected void CompleteLayout()
        {
            PerformLayout();
            if (_designSize.IsEmpty) _designSize = ClientSize;
            CaptureLayout();
        }

        protected void SetDisplayFont(Control control, Font font)
        {
            if (_layout == null) { control.Font = font; return; }
            float referenceScale = Math.Min((float)_referenceSize.Width / _designSize.Width,
                (float)_referenceSize.Height / _designSize.Height);
            foreach (var item in _layout) item.UpdateFont(control, font, referenceScale);
            float scale = Math.Min((float)ClientSize.Width / _designSize.Width,
                (float)ClientSize.Height / _designSize.Height);
            AssignFont(control, font, scale);
        }

        private void AssignFont(Control control, Font font, float scale)
        {
            float size = Math.Max(1, font.Size * scale);
            if (control.Font.FontFamily.Name == font.FontFamily.Name && control.Font.Style == font.Style
                && Math.Abs(control.Font.Size - size) < 0.001f) return;
            var next = new Font(font.FontFamily, size, font.Style, font.Unit);
            control.Font = next;
            if (_ownedFonts.TryGetValue(control, out var previous)) previous.Dispose();
            _ownedFonts[control] = next;
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            if (_layout == null) CompleteLayout(); // Корневой элемент в конструкторе VS.
        }

        private void CaptureLayout()
        {
            if (_applying || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            _referenceSize = ClientSize;
            if (_layout != null) foreach (var item in _layout) item.Release();
            _layout = Controls.Cast<Control>().Select(c => new LayoutItem(c)).ToList();
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            e.Control.LocationChanged += ChildLayoutChanged;
            e.Control.SizeChanged += ChildLayoutChanged;
            if (_layout != null) CaptureLayout();
        }

        protected override void OnControlRemoved(ControlEventArgs e)
        {
            e.Control.LocationChanged -= ChildLayoutChanged;
            e.Control.SizeChanged -= ChildLayoutChanged;
            base.OnControlRemoved(e);
            if (_layout != null) CaptureLayout();
        }

        private void ChildLayoutChanged(object sender, EventArgs e)
        {
            // Ручная перестановка в Designer становится новой исходной раскладкой.
            if (!_applying && _layout != null) CaptureLayout();
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (!_applying && _layout != null && ClientSize.Width > 0 && ClientSize.Height > 0)
            {
                _applying = true;
                SuspendLayout();
                try
                {
                    float x = (float)ClientSize.Width / _referenceSize.Width;
                    float y = (float)ClientSize.Height / _referenceSize.Height;
                    foreach (var item in _layout) item.Apply(this, x, y);
                }
                finally { ResumeLayout(true); _applying = false; }
            }
            base.OnSizeChanged(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _layout != null)
            {
                foreach (var item in _layout) item.Release();
                _layout = null;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                foreach (var font in _ownedFonts.Values) font.Dispose();
                _ownedFonts.Clear();
            }
        }

        private sealed class LayoutItem
        {
            private readonly Control _control;
            private readonly Rectangle _bounds;
            private readonly Padding _padding, _margin;
            private Font _font;
            private readonly List<LayoutItem> _children;
            private readonly float[] _rows, _columns;

            public LayoutItem(Control control)
            {
                _control = control; _bounds = control.Bounds;
                _padding = control.Padding; _margin = control.Margin;
                _font = (Font)control.Font.Clone();
                // Вложенный модуль сам масштабирует своё содержимое.
                _children = control is ScalableControl ? new List<LayoutItem>()
                    : control.Controls.Cast<Control>().Select(c => new LayoutItem(c)).ToList();
                if (control is TableLayoutPanel table)
                {
                    _rows = table.RowStyles.Cast<RowStyle>().Select(s => s.Height).ToArray();
                    _columns = table.ColumnStyles.Cast<ColumnStyle>().Select(s => s.Width).ToArray();
                }
            }

            public void UpdateFont(Control control, Font font, float scale)
            {
                if (_control == control)
                {
                    _font.Dispose();
                    _font = new Font(font.FontFamily, Math.Max(1, font.Size * scale), font.Style, font.Unit);
                }
                foreach (var child in _children) child.UpdateFont(control, font, scale);
            }

            public void Apply(ScalableControl owner, float x, float y)
            {
                _control.SuspendLayout();
                try
                {
                    _control.Padding = ScalePadding(_padding, x, y);
                    _control.Margin = ScalePadding(_margin, x, y);
                    if (!(_control is ScalableControl))
                    {
                        owner.AssignFont(_control, _font, Math.Min(x, y));
                    }
                    if (_control is TableLayoutPanel table)
                    {
                        for (int i = 0; i < _rows.Length; i++)
                            if (table.RowStyles[i].SizeType == SizeType.Absolute) table.RowStyles[i].Height = _rows[i] * y;
                        for (int i = 0; i < _columns.Length; i++)
                            if (table.ColumnStyles[i].SizeType == SizeType.Absolute) table.ColumnStyles[i].Width = _columns[i] * x;
                    }
                    _control.Bounds = new Rectangle((int)Math.Round(_bounds.X * x), (int)Math.Round(_bounds.Y * y),
                        Math.Max(1, (int)Math.Round(_bounds.Width * x)), Math.Max(1, (int)Math.Round(_bounds.Height * y)));
                    foreach (var child in _children) child.Apply(owner, x, y);
                }
                finally { _control.ResumeLayout(true); }
            }

            private static Padding ScalePadding(Padding p, float x, float y) => new Padding(
                (int)Math.Round(p.Left * x), (int)Math.Round(p.Top * y),
                (int)Math.Round(p.Right * x), (int)Math.Round(p.Bottom * y));

            public void Release()
            {
                _font.Dispose();
                foreach (var child in _children) child.Release();
            }
        }
    }
}
