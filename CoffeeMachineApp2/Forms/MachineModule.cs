using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Базовый класс всех модулей интерфейса (меню, настройка, заказ, оплата, приготовление).
    /// Модуль - это самостоятельный UserControl: его можно перетащить на форме в дизайнере,
    /// изменить размер или поменять местами с другим модулем.
    ///
    /// Общие правила для всех модулей:
    ///   * автомат (модель) передаётся свойством Machine;
    ///   * модуль сам умеет перерисовать себя по состоянию автомата (RefreshView);
    ///   * действия пользователя выполняются через Run(...): ошибки правил превращаются в уведомление;
    ///   * когда что-то изменилось, модуль сообщает об этом событием Changed - и хозяин обновит всех.
    /// </summary>
    public class MachineModule : UserControl
    {
        private CoffeeMachine _machine;
        private bool _active = true;
        private string _title = "";

        public event EventHandler Changed;
        public event EventHandler<NotificationEventArgs> Notification;

        public MachineModule()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
        }

        protected override Padding DefaultPadding
        {
            get { return new Padding(14, 44, 14, 14); }
        }

        protected override Size DefaultSize
        {
            get { return new Size(300, 300); }
        }

        /// <summary>Автомат, с которым работает модуль (задаёт хозяин при запуске).</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CoffeeMachine Machine
        {
            get { return _machine; }
            set
            {
                _machine = value;
                if (_machine != null)
                    OnMachineAttached();
            }
        }

        /// <summary>Заголовок в верхней части рамки модуля.</summary>
        [Category("Module"), Description("Заголовок модуля"), DefaultValue("")]
        public string Title
        {
            get { return _title; }
            set { _title = value ?? ""; Invalidate(); }
        }

        /// <summary>Сейчас ли этот шаг "в работе" (рамка подсвечивается коричневым).</summary>
        protected bool Active
        {
            get { return _active; }
            set
            {
                if (_active == value) return;
                _active = value;
                Invalidate();
            }
        }

        /// <summary>Обновить внешний вид по текущему состоянию автомата.</summary>
        public void RefreshView()
        {
            if (_machine == null)
                return;

            Active = IsActiveIn(_machine.State);
            OnRefreshView();
        }

        // ---- точки расширения: наследники переопределяют ----

        /// <summary>В каких состояниях автомата этот модуль "активен".</summary>
        protected virtual bool IsActiveIn(MachineState state) { return true; }

        /// <summary>Вызывается один раз, когда модулю передали автомат.</summary>
        protected virtual void OnMachineAttached() { }

        /// <summary>Показать актуальные данные автомата.</summary>
        protected virtual void OnRefreshView() { }

        // ---- общие помощники ----

        /// <summary>
        /// Выполнить действие пользователя. Ошибка правил (исключение автомата) показывается
        /// как уведомление, после любого действия все модули обновляются.
        /// </summary>
        protected bool Run(Action action)
        {
            bool ok = true;
            try
            {
                action();
            }
            catch (InvalidOperationException ex)
            {
                ok = false;
                Notify(ex.Message, true);
            }
            catch (ArgumentException ex)
            {
                ok = false;
                Notify(ex.Message, true);
            }
            RaiseChanged();
            return ok;
        }

        protected void Notify(string message, bool isError)
        {
            EventHandler<NotificationEventArgs> handler = Notification;
            if (handler != null)
                handler(this, new NotificationEventArgs(message, isError));
        }

        protected void RaiseChanged()
        {
            EventHandler handler = Changed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        // ---- оформление рамки ----

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UiStyle.ApplyRoundRegion(this, 14);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath path = UiStyle.RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 14))
            using (var pen = new Pen(_active ? UiStyle.Brown : UiStyle.Line, _active ? 2f : 1f))
                g.DrawPath(pen, path);

            if (_title.Length > 0)
                TextRenderer.DrawText(g, _title, UiStyle.Bold, new Point(16, 13), _active ? UiStyle.Brown : UiStyle.Gray);
        }
    }
}
