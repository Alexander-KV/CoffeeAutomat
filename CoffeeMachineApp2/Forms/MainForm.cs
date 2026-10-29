using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly IMachineStore _store;
        private readonly AdminAccess _access;
        private readonly List<CoffeeMachineControl> _machines = new List<CoffeeMachineControl>();
        private Size _preferredMachineSize;
        // Конструктор VS: только интерфейс, без файлов и авторизации.
        public MainForm()
        {
            InitializeComponent();
        }
        public MainForm(IMachineStore store) : this()
        {
            _store = store;
            _access = new AdminAccess(Path.Combine(Path.GetDirectoryName(JsonMachineStore.DefaultPath), "admin-access.json"));
            // Используем сам элемент из Designer: его настройки больше не теряются.
            _preferredMachineSize = machinePreview.Size;
            foreach (var config in _store.Load()) Attach(config);
            _floor.SizeChanged += (s, e) => FitMachines();
            Shown += (s, e) => FitMachines();
            FitMachines();
            RefreshMode();
            FormClosing += (s,e) =>
            {
                if (_machines.Any(x => x.Machine.Order != null) && MessageBox.Show(this,
                    "Есть незавершённые заказы. При закрытии внесённые деньги, незабранные напитки и сдача будут сброшены. Закрыть программу?", "Завершение работы",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) e.Cancel = true;
            };
        }
        private void AddMachineClick(object sender, EventArgs e) => AddMachine(MachineConfiguration.CreateDefault());

        private void Attach(MachineConfiguration configuration)
        {
            var machine = new CoffeeMachine(configuration);
            var view = machinePreview;
            if (view != null) machinePreview = null;
            else
            {
                view = new CoffeeMachineControl();
                _floor.Controls.Add(view);
            }
            view.Bind(machine, SaveConfiguration, _access);
            view.DuplicateRequested += (s,e) => AddMachine(machine.Configuration);
            view.RemoveRequested += (s,e) => RemoveMachine(view);
            _machines.Add(view);
            view.RefreshAccess();
            FitMachines();
        }

        // Меняется только внешний размер. Внутри работают Dock и TableLayoutPanel.
        private void FitMachines()
        {
            if (_machines.Count == 0) return;
            int height = _floor.ClientSize.Height - _floor.Padding.Vertical
                - _machines[0].Margin.Vertical - SystemInformation.HorizontalScrollBarHeight;
            int width = Math.Min(_preferredMachineSize.Width,
                _floor.ClientSize.Width - _floor.Padding.Horizontal - _machines[0].Margin.Horizontal);
            foreach (var view in _machines)
                view.Size = new Size(Math.Max(view.MinimumSize.Width, width), Math.Max(view.MinimumSize.Height, height));
        }
        private void AddMachine(MachineConfiguration source)
        {
            try
            {
                _access.RequireAdministrator();
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
            try
            {
                _access.RequireAdministrator();
                if (_machines.Count <= 1) { MessageBox.Show(this, "Оставьте хотя бы один автомат.", "Кофейное пространство"); return; }
                if (view.Machine.Order != null) return;
                if (MessageBox.Show(this, $"Убрать «{view.Machine.Configuration.Name}» вместе с его настройками? Остальные автоматы сохранят свои меню.",
                    "Удаление автомата", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                _store.Save(_machines.Where(x => x != view).Select(x => x.Machine.Configuration).ToArray());
                _machines.Remove(view); _floor.Controls.Remove(view); view.Dispose(); UpdateCount();
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException)
            { MessageBox.Show(this, ex.Message, "Не удалось убрать автомат", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }
        private void SaveConfiguration(MachineConfiguration updated)
        {
            _access.RequireAdministrator();
            _store.Save(_machines.Select(x => x.Machine.Configuration.Id == updated.Id ? updated : x.Machine.Configuration).ToArray());
        }
        private void SwitchMode(object sender, EventArgs e)
        {
            if (_access.IsAdmin) { _access.Logout(); RefreshMode(); return; }
            if (_machines.Any(x => x.Machine.Order != null))
            {
                MessageBox.Show(this, "Перед входом завершите или отмените все заказы, заберите напитки, карты и сдачу.",
                    "Автоматы обслуживают покупателей", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dialog = new AdminLoginForm(_access)) dialog.ShowDialog(this);
            RefreshMode();
        }
        private void RefreshMode()
        {
            _add.Visible = _access.IsAdmin; _modeHint.Visible = !_access.IsAdmin;
            _adminButton.Text = _access.IsAdmin ? "Выйти из админ-режима" : "Администратор";
            _adminButton.Selected = _access.IsAdmin;
            foreach (var view in _machines) view.RefreshAccess();
            UpdateCount();
        }
        private void UpdateCount()
        {
            _count.Text = _access.IsAdmin
                ? $"РЕЖИМ АДМИНИСТРАТОРА  ·  Автоматов: {_machines.Count}  ·  Покупки приостановлены"
                : $"Автоматов: {_machines.Count}  ·  Выберите напиток, настройте под себя и заберите свежую чашку.";
        }
    }
}
