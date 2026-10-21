using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Модуль 4: приготовление и выдача. Пока напитки готовятся - анимация со стаканом,
    /// потом "заказ готов": список выданного, лоток сдачи и кнопки "Забрать сдачу" / "Новый заказ".
    /// </summary>
    public class PreparationModule : MachineModule
    {
        private const int TrayRow = 3;
        private const int ButtonsRow = 4;

        private readonly PreparationView _view;
        private readonly Label _lblStatus;
        private readonly Label _lblInfo;
        private readonly CoinTrayView _tray;
        private readonly RoundedButton _btnTakeChange;
        private readonly RoundedButton _btnNewOrder;
        private readonly TableLayoutPanel _stack;
        private readonly Timer _timer = new Timer { Interval = 40 };

        private List<Beverage> _queue = new List<Beverage>();
        private int _index;
        private int _elapsedMs;
        private bool _running;

        public PreparationModule()
        {
            Title = "4 · Приготовление и выдача";

            _view = new PreparationView { Dock = DockStyle.Fill, Margin = Padding.Empty };
            _lblStatus = UiStyle.CreateLabel("", UiStyle.Big, UiStyle.DarkBrown, ContentAlignment.MiddleCenter);
            _lblInfo = UiStyle.CreateLabel("", UiStyle.Regular, UiStyle.Gray, ContentAlignment.TopCenter);
            _tray = new CoinTrayView { Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };

            _btnTakeChange = UiStyle.CreateButton("Забрать сдачу", true);
            _btnNewOrder = UiStyle.CreateButton("Новый заказ", false);
            TableLayoutPanel buttons = UiStyle.CreateColumns(0, 0);
            buttons.Controls.Add(_btnTakeChange, 0, 0);
            buttons.Controls.Add(_btnNewOrder, 1, 0);

            _stack = UiStyle.CreateStack(0, 30, 58, 58, 44);
            _stack.Controls.Add(_view, 0, 0);
            _stack.Controls.Add(_lblStatus, 0, 1);
            _stack.Controls.Add(_lblInfo, 0, 2);
            _stack.Controls.Add(_tray, 0, TrayRow);
            _stack.Controls.Add(buttons, 0, ButtonsRow);
            Controls.Add(_stack);

            UiStyle.SetRowHeight(_stack, TrayRow, 0);
            UiStyle.SetRowHeight(_stack, ButtonsRow, 0);
            _tray.Visible = false;
            buttons.Visible = false;
            _buttonsPanel = buttons;

            _timer.Tick += OnTimerTick;
            _btnTakeChange.Click += (s, e) => TakeChange();
            _btnNewOrder.Click += (s, e) => Run(() => Machine.StartNewOrder());
        }

        private readonly TableLayoutPanel _buttonsPanel;

        protected override bool IsActiveIn(MachineState state)
        {
            return state == MachineState.Preparing || state == MachineState.Ready;
        }

        private void TakeChange()
        {
            Run(() =>
            {
                decimal amount = Machine.TakeChange();
                Notify("Вы забрали сдачу: " + amount.ToString("0") + " ₽", false);
            });
        }

        // ---------- отображение ----------

        protected override void OnRefreshView()
        {
            switch (Machine.State)
            {
                case MachineState.Preparing:
                    if (!_running) StartPreparation();
                    ShowCollapsed();
                    break;

                case MachineState.Ready:
                    StopTimer();
                    ShowReady();
                    break;

                default:
                    StopTimer();
                    ShowIdle();
                    break;
            }
        }

        private void ShowCollapsed()
        {
            UiStyle.SetRowHeight(_stack, TrayRow, 0);
            UiStyle.SetRowHeight(_stack, ButtonsRow, 0);
            _tray.Visible = false;
            _buttonsPanel.Visible = false;
        }

        private void ShowIdle()
        {
            _view.Mode = PreparationMode.Idle;
            _view.Progress = 0;
            _lblStatus.Text = "Здесь появится ваш напиток";
            _lblStatus.Font = UiStyle.Regular;
            _lblStatus.ForeColor = UiStyle.Gray;
            _lblInfo.Text = "";
            ShowCollapsed();
        }

        private void ShowReady()
        {
            Beverage last = Machine.Order.Items[Machine.Order.Items.Count - 1].Beverage;
            _view.Mode = PreparationMode.Ready;
            _view.LiquidColor = UiStyle.ParseColor(last.LiquidColor);
            _view.HasMilk = last.HasMilk;

            _lblStatus.Text = "Ваш заказ готов!";
            _lblStatus.Font = UiStyle.Big;
            _lblStatus.ForeColor = UiStyle.Green;

            string paid = "Оплата: " + Machine.PaidWith;
            if (Machine.LastChange > 0)
                paid += Machine.ChangeTaken ? " · сдача получена" : " · сдача " + Machine.LastChange.ToString("0") + " ₽";
            _lblInfo.Text = "Выдано: " + string.Join(", ", Machine.GetIssuedList()) + "\n" + paid;

            bool pending = Machine.LastChange > 0 && !Machine.ChangeTaken;
            _tray.Coins = pending ? Machine.ChangeCoins : new List<int>();
            _tray.Visible = pending;
            UiStyle.SetRowHeight(_stack, TrayRow, pending ? 58 : 0);

            _buttonsPanel.Visible = true;
            UiStyle.SetRowHeight(_stack, ButtonsRow, 44);
            bool hasChange = Machine.LastChange > 0;
            _btnTakeChange.Visible = hasChange;
            _btnTakeChange.Enabled = pending;
            // без сдачи (оплата картой) кнопка "Новый заказ" занимает всю ширину
            _buttonsPanel.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, hasChange ? 100 : 0);
            _btnNewOrder.Enabled = !pending;
        }

        // ---------- анимация приготовления ----------

        private void StartPreparation()
        {
            _queue = Machine.GetPreparationQueue();
            _index = 0;
            _elapsedMs = 0;
            _running = true;
            ApplyQueueItem();
            _timer.Start();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _running = false;
        }

        private void ApplyQueueItem()
        {
            Beverage beverage = _queue[_index];
            _view.Mode = PreparationMode.Brewing;
            _view.Progress = 0;
            _view.LiquidColor = UiStyle.ParseColor(beverage.LiquidColor);
            _view.HasMilk = beverage.HasMilk;

            _lblStatus.Font = UiStyle.Big;
            _lblStatus.ForeColor = UiStyle.DarkBrown;
            _lblStatus.Text = beverage.Name + " · " + (_index + 1) + " из " + _queue.Count;
            UpdateStage(beverage, 0);
        }

        private void UpdateStage(Beverage beverage, double progress)
        {
            // Этапы возвращает сам напиток: у кофе и шоколада они разные (полиморфизм)
            IReadOnlyList<string> stages = beverage.GetStages();
            int stage = Math.Min(stages.Count - 1, (int)(progress * stages.Count));
            _lblInfo.Text = stages[stage];
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (Machine == null || !_running || _queue.Count == 0)
                return;

            _elapsedMs += _timer.Interval;
            Beverage current = _queue[_index];
            double progress = _elapsedMs / (current.PrepareSeconds * 1000.0);

            if (progress < 1.0)
            {
                _view.Progress = progress;
                _view.Advance();
                UpdateStage(current, progress);
                return;
            }

            // текущий напиток готов
            _view.Progress = 1.0;
            _view.Advance();
            _index++;
            _elapsedMs = 0;

            if (_index < _queue.Count)
            {
                ApplyQueueItem();
                return;
            }

            // все напитки готовы
            StopTimer();
            Machine.FinishPreparing();
            RaiseChanged();
        }
    }
}
