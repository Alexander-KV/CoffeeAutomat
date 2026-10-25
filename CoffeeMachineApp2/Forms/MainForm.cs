using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp.Forms
{
    public sealed class MainForm : Form
    {
        private readonly IMachineStore _store;
        private readonly List<CoffeeMachineControl> _machines = new List<CoffeeMachineControl>();
        private readonly FlowLayoutPanel _floor = new FlowLayoutPanel();
        private readonly Label _count;
        public MainForm(IMachineStore store)
        {
            _store = store;
            Text = "BEAN & CUP — кофе в вашем ритме"; Font = UiStyle.Regular;
            BackColor = Color.FromArgb(232, 231, 220);
            AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(1440, 1040); MinimumSize = new Size(740, 640);
            StartPosition = FormStartPosition.CenterScreen; WindowState = FormWindowState.Maximized;
            var header = new Panel { Dock = DockStyle.Top, Height = 126, Padding = new Padding(28, 16, 28, 12), BackColor = UiStyle.Cream };
            var actions = new Panel { Dock = DockStyle.Right, Width = 230 };
            var add = UiStyle.Button("+ Новый автомат", (s,e) => AddMachine(MachineConfiguration.CreateDefault()), true);
            add.Bounds = new Rectangle(20, 18, 210, 44); actions.Controls.Add(add);
            var rightHint = UiStyle.Label("Свой кофе — в каждом автомате", 22); rightHint.Bounds = new Rectangle(0, 68, 230, 22); rightHint.Dock = DockStyle.None;
            rightHint.TextAlign = ContentAlignment.MiddleRight; rightHint.ForeColor = UiStyle.Gray; rightHint.Font = UiStyle.Small; actions.Controls.Add(rightHint);
            var identity = new Panel { Dock = DockStyle.Fill };
            var brand = UiStyle.Label("B E A N  &  C U P    /    COFFEE ATELIER", 19, true); brand.Font = UiStyle.Small; brand.ForeColor = UiStyle.Forest;
            var title = UiStyle.Label("Кофе. В вашем ритме.", 47); title.Font = new Font("Georgia", 26F); title.Top = 21; title.Dock = DockStyle.None; title.Width = 650;
            _count = UiStyle.Label("", 24); _count.Top = 71; _count.Dock = DockStyle.None; _count.Width = 650; _count.ForeColor = UiStyle.Gray;
            identity.Controls.Add(_count); identity.Controls.Add(title); identity.Controls.Add(brand); header.Controls.Add(identity); header.Controls.Add(actions);
            _floor.Dock = DockStyle.Fill; _floor.AutoScroll = true; _floor.WrapContents = false;
            _floor.FlowDirection = FlowDirection.LeftToRight; _floor.Padding = new Padding(14, 18, 14, 12);
            var footer = UiStyle.Label("СВЕЖИЙ ПОМОЛ     /     АВТОРСКИЕ РЕЦЕПТЫ     /     МАЛЕНЬКАЯ ПАУЗА ДЛЯ СЕБЯ", 34);
            footer.Dock = DockStyle.Bottom; footer.ForeColor = UiStyle.Gray; footer.Font = UiStyle.Small; footer.Padding = new Padding(28, 0, 0, 0); footer.BackColor = UiStyle.Cream;
            Controls.Add(_floor); Controls.Add(footer); Controls.Add(header);
            foreach (var config in _store.Load()) Attach(config);
            UpdateCount();
            FormClosing += (s,e) =>
            {
                if (_machines.Any(x => x.Machine.Order != null) && MessageBox.Show(this,
                    "Есть незавершённые заказы. При закрытии они и демо-деньги будут сброшены. Закрыть программу?", "Завершение работы",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) e.Cancel = true;
            };
        }
        private void Attach(MachineConfiguration configuration)
        {
            var machine = new CoffeeMachine(configuration); var view = new CoffeeMachineControl(machine, SaveConfiguration);
            view.DuplicateRequested += (s,e) => AddMachine(machine.Configuration);
            view.RemoveRequested += (s,e) => RemoveMachine(view);
            _machines.Add(view); _floor.Controls.Add(view);
        }
        private void AddMachine(MachineConfiguration source)
        {
            try
            {
                if (_machines.Count >= 12) throw new InvalidOperationException("Максимум 12 автоматов.");
                int number = 1;
                while (_machines.Any(x => x.Machine.Configuration.Name == $"Кофейня №{number}")) number++;
                var next = source.Duplicate($"Кофейня №{number}");
                _store.Save(_machines.Select(x => x.Machine.Configuration).Append(next).ToArray());
                Attach(next); UpdateCount(); _floor.ScrollControlIntoView(_machines.Last());
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException || ex is InvalidOperationException || ex is ArgumentException)
            { MessageBox.Show(this, ex.Message, "Не удалось добавить автомат", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        private void RemoveMachine(CoffeeMachineControl view)
        {
            if (_machines.Count <= 1) { MessageBox.Show(this, "Оставьте хотя бы один автомат.", "Кофейное пространство"); return; }
            if (view.Machine.Order != null) return;
            if (MessageBox.Show(this, $"Убрать «{view.Machine.Configuration.Name}» вместе с его настройками? Остальные автоматы сохранят свои меню.",
                "Удаление автомата", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                _store.Save(_machines.Where(x => x != view).Select(x => x.Machine.Configuration).ToArray());
                _machines.Remove(view); _floor.Controls.Remove(view); view.Dispose(); UpdateCount();
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException)
            { MessageBox.Show(this, ex.Message, "Не удалось убрать автомат", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        private void SaveConfiguration(MachineConfiguration updated)
        {
            _store.Save(_machines.Select(x => x.Machine.Configuration.Id == updated.Id ? updated : x.Machine.Configuration).ToArray());
        }
        private void UpdateCount() { _count.Text = $"Автоматов: {_machines.Count}  ·  Выберите напиток, настройте под себя и заберите свежую чашку."; }
    }
}
