using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Редактор копии меню: карточки слева, поля рецепта справа. Применение только после записи JSON.</summary>
    public sealed class AdminForm : Form
    {
        private readonly MachineConfiguration _original;
        private readonly Action<MachineConfiguration> _save;
        private readonly List<Beverage> _draft;
        private readonly ListBox _list = new ListBox();
        private readonly TextBox _machineName = new TextBox(), _name = new TextBox(), _recipe = new TextBox(), _color = new TextBox();
        private readonly ComboBox _image = new ComboBox();
        private readonly NumericUpDown _price = Number(1, 9999, 2), _volume = Number(30, 1000), _seconds = Number(2, 60);
        private readonly CheckBox _enabled = new CheckBox { Text = "В продаже", AutoSize = true }, _milk = new CheckBox { Text = "Содержит молоко", AutoSize = true }, _coffee = new CheckBox { Text = "Кофейный напиток", AutoSize = true };
        private readonly PictureBox _preview = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.White };
        private readonly Label _status = UiStyle.Label("", 24), _previewTitle = UiStyle.Label("", 26, true);
        private int _current = -1;
        private bool _loading, _dirty;
        public AdminForm(MachineConfiguration configuration, Action<MachineConfiguration> save)
        {
            _original = configuration; _save = save; _draft = configuration.Menu.ToList();
            Text = "BEAN & CUP / Мастерская рецептов"; Font = UiStyle.Regular; BackColor = UiStyle.Cream;
            AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(1070, 760); MinimumSize = new Size(940, 740); StartPosition = FormStartPosition.CenterParent; MinimizeBox = false;
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 4 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58)); Controls.Add(root);
            var header = new Panel { Dock = DockStyle.Fill };
            var title = UiStyle.Label("Мастерская рецептов", 43); title.Font = UiStyle.Display;
            var subtitle = UiStyle.Label("Ваше меню. Ваш характер. Изменения применяются после сохранения.", 25); subtitle.ForeColor = UiStyle.Gray;
            header.Controls.Add(subtitle); header.Controls.Add(title); root.Controls.Add(header, 0, 0);
            var nameRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            nameRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170)); nameRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            nameRow.Controls.Add(UiStyle.Label("Название автомата"), 0, 0); _machineName.Dock = DockStyle.Fill; _machineName.MaxLength = 40; _machineName.Text = configuration.Name;
            nameRow.Controls.Add(_machineName, 1, 0); root.Controls.Add(nameRow, 0, 1);
            var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = Padding.Empty };
            body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 292)); body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); root.Controls.Add(body, 0, 2);
            var menu = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 15, 0) };
            _list.Dock = DockStyle.Fill; _list.BorderStyle = BorderStyle.None; _list.BackColor = Color.White; _list.DrawMode = DrawMode.OwnerDrawFixed;
            _list.ItemHeight = 70; _list.IntegralHeight = false; _list.DrawItem += DrawDrink; _list.AccessibleName = "Напитки в меню";
            var menuActions = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, WrapContents = false };
            var add = UiStyle.Button("+ Напиток", Add); add.Width = 132;
            var remove = UiStyle.Button("Удалить", Remove); remove.Width = 122;
            menuActions.Controls.AddRange(new Control[] { add, remove }); menu.Controls.Add(_list); menu.Controls.Add(menuActions); body.Controls.Add(menu, 0, 0);
            var editor = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 11, Padding = new Padding(12, 0, 0, 0) };
            editor.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 155)); editor.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            foreach (int h in new[] { 118, 35, 35, 35, 63, 35, 35, 35, 30, 30, 30 }) editor.RowStyles.Add(new RowStyle(SizeType.Absolute, h)); body.Controls.Add(editor, 1, 0);
            var previewArea = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(12) };
            UiStyle.Round(previewArea, 12); _preview.Dock = DockStyle.Left; _preview.Width = 126; _previewTitle.Dock = DockStyle.Fill;
            _previewTitle.Padding = new Padding(14, 0, 0, 0); previewArea.Controls.Add(_previewTitle); previewArea.Controls.Add(_preview);
            editor.Controls.Add(previewArea, 0, 0); editor.SetColumnSpan(previewArea, 2);
            _name.MaxLength = 60; _recipe.Multiline = true; _recipe.MaxLength = 200; _recipe.ScrollBars = ScrollBars.Vertical; _color.MaxLength = 7;
            _image.DropDownStyle = ComboBoxStyle.DropDown; _image.MaxLength = 100;
            _image.Items.Add(""); for (int i = 1; i <= 9; i++) _image.Items.Add($"coffee{i}.png");
            Row(editor, 1, "Название напитка", _name); Row(editor, 2, "Цена, ₽", _price); Row(editor, 3, "Объём, мл", _volume);
            Row(editor, 4, "Рецепт / описание", _recipe);
            var imageRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Margin = Padding.Empty };
            imageRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); imageRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
            _image.Dock = DockStyle.Fill; imageRow.Controls.Add(_image, 0, 0);
            var upload = UiStyle.Button("Загрузить…", ImportImage); upload.Dock = DockStyle.Fill; upload.Height = 28; upload.Font = UiStyle.Small; upload.Margin = new Padding(4, 0, 0, 0);
            imageRow.Controls.Add(upload, 1, 0); Row(editor, 5, "Изображение", imageRow);
            Row(editor, 6, "Цвет #RRGGBB", _color); Row(editor, 7, "Приготовление, сек.", _seconds);
            Row(editor, 8, "Доступность", _enabled); Row(editor, 9, "Рецептура", _milk); Row(editor, 10, "Тип", _coffee);
            var footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(0, 12, 0, 0) };
            footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110)); footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
            _status.Dock = DockStyle.Fill; _status.ForeColor = UiStyle.Gray; _status.Text = "Меню сохраняется в JSON";
            var cancel = UiStyle.Button("Отмена", (s,e) => Close()); cancel.Dock = DockStyle.Fill;
            var saveButton = UiStyle.Button("Сохранить изменения", Save, true); saveButton.Dock = DockStyle.Fill;
            footer.Controls.Add(_status, 0, 0); footer.Controls.Add(cancel, 1, 0); footer.Controls.Add(saveButton, 2, 0); root.Controls.Add(footer, 0, 3); CancelButton = cancel;
            _list.SelectedIndexChanged += SelectionChanged;
            foreach (var c in new TextBox[] { _name, _recipe, _color, _machineName }) c.TextChanged += Edited;
            _image.TextChanged += Edited; _price.ValueChanged += Edited; _volume.ValueChanged += Edited; _seconds.ValueChanged += Edited;
            _enabled.CheckedChanged += Edited; _milk.CheckedChanged += Edited; _coffee.CheckedChanged += Edited;
            FormClosing += (s,e) =>
            {
                if (_dirty && DialogResult != DialogResult.OK && MessageBox.Show(this, "Закрыть без сохранения изменений?", "Несохранённые изменения", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) e.Cancel = true;
            };
            ReloadList(0);
        }
        private static NumericUpDown Number(int min, int max, int decimals = 0) => new NumericUpDown { Minimum = min, Maximum = max, DecimalPlaces = decimals, ThousandsSeparator = true };
        private static void Row(TableLayoutPanel panel, int row, string title, Control field)
        {
            var label = UiStyle.Label(title); label.Dock = DockStyle.Fill; panel.Controls.Add(label, 0, row);
            field.Dock = DockStyle.Fill; field.Margin = new Padding(3, 3, 0, 4); panel.Controls.Add(field, 1, row);
        }
        private void DrawDrink(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var drink = (Beverage)_list.Items[e.Index]; bool selected = (e.State & DrawItemState.Selected) != 0;
            using var background = new SolidBrush(selected ? Color.FromArgb(232, 237, 222) : Color.White); e.Graphics.FillRectangle(background, e.Bounds);
            int pad = 10 * DeviceDpi / 96, image = 56 * DeviceDpi / 96;
            e.Graphics.DrawImage(ImageLoader.Load(drink), new Rectangle(e.Bounds.X + pad, e.Bounds.Y + pad, image, e.Bounds.Height - pad * 2));
            var rect = new Rectangle(e.Bounds.X + image + pad * 2, e.Bounds.Y + pad, e.Bounds.Width - image - pad * 3, e.Bounds.Height / 2 - pad / 2);
            TextRenderer.DrawText(e.Graphics, drink.Name, UiStyle.Bold, rect, UiStyle.Ink, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            rect.Y += e.Bounds.Height / 2 - pad / 2;
            TextRenderer.DrawText(e.Graphics, $"{drink.Price:0.##} ₽  ·  {drink.VolumeMl} мл" + (drink.IsEnabled ? "" : "  ·  выключен"), UiStyle.Small, rect, UiStyle.Gray, TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine);
            e.DrawFocusRectangle();
        }
        private void ReloadList(int selected)
        {
            _loading = true; _list.BeginUpdate(); _list.Items.Clear(); foreach (var d in _draft) _list.Items.Add(d);
            _list.SelectedIndex = selected; _list.EndUpdate(); _loading = false; LoadEditor(selected);
        }
        private void LoadEditor(int index)
        {
            _loading = true; _current = index;
            var d = _draft[index]; _name.Text = d.Name; _price.Value = d.Price; _volume.Value = d.VolumeMl; _recipe.Text = d.Recipe;
            _image.Text = d.ImageFile; _color.Text = d.LiquidColor; _seconds.Value = d.PrepareSeconds;
            _milk.Checked = d.HasMilk; _coffee.Checked = d.IsCoffee; _enabled.Checked = d.IsEnabled;
            _loading = false; UpdatePreview();
        }
        private Beverage ReadEditor() => _draft[_current] with
        {
            Name = _name.Text.Trim(), Price = _price.Value, VolumeMl = (int)_volume.Value, Recipe = _recipe.Text.Trim(),
            ImageFile = _image.Text.Trim(), LiquidColor = _color.Text.Trim(), PrepareSeconds = (int)_seconds.Value,
            HasMilk = _milk.Checked, IsCoffee = _coffee.Checked, IsEnabled = _enabled.Checked
        };
        private bool ApplyEditor()
        {
            if (_current < 0) return true;
            try
            {
                var drink = ReadEditor(); drink.Validate(); _draft[_current] = drink;
                _loading = true; _list.Items[_current] = drink; _loading = false;
                return true;
            }
            catch (ArgumentException ex) { _loading = false; Error(ex.Message); return false; }
        }
        private void SelectionChanged(object sender, EventArgs e)
        {
            if (_loading || _list.SelectedIndex < 0) return;
            int next = _list.SelectedIndex;
            if (!ApplyEditor()) { _loading = true; _list.SelectedIndex = _current; _loading = false; return; }
            LoadEditor(next);
        }
        private void Edited(object sender, EventArgs e)
        {
            if (_loading) return;
            _dirty = true; _status.ForeColor = UiStyle.Brown; _status.Text = "Есть несохранённые изменения"; UpdatePreview();
        }
        private void UpdatePreview()
        {
            if (_current < 0) return;
            _previewTitle.Text = _name.Text + $"\n{_price.Value:0.##} ₽  /  {_volume.Value} мл";
            var file = _image.Text.Trim();
            if (!file.Contains('/') && !file.Contains('\\')) _preview.Image = ImageLoader.Load(new Beverage { ImageFile = file });
        }
        private void Add(object sender, EventArgs e)
        {
            if (_draft.Count >= 100) { Error("В меню может быть до 100 напитков."); return; }
            if (!ApplyEditor()) return;
            _draft.Add(new Beverage { Name = "Новый авторский кофе" }); _dirty = true; ReloadList(_draft.Count - 1); _name.Focus(); _name.SelectAll();
        }
        private void Remove(object sender, EventArgs e)
        {
            if (_draft.Count <= 1) { Error("Оставьте хотя бы один напиток в меню."); return; }
            if (MessageBox.Show(this, $"Удалить «{_draft[_current].Name}» из меню? Изменение применится после сохранения.", "Удаление рецепта", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            int next = Math.Min(_current, _draft.Count - 2); _draft.RemoveAt(_current); _dirty = true; ReloadList(next);
        }
        private void ImportImage(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog { Title = "Картинка для напитка", Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp" };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            try
            {
                if (new FileInfo(dialog.FileName).Length > 10 * 1024 * 1024) throw new ArgumentException("Выберите изображение до 10 МБ.");
                using var image = Image.FromFile(dialog.FileName);
                if (image.Width > 4096 || image.Height > 4096) throw new ArgumentException("Максимальный размер изображения — 4096 × 4096.");
                Directory.CreateDirectory(ImageLoader.CustomImageDirectory);
                string fileName = Guid.NewGuid().ToString("N") + ".png";
                image.Save(Path.Combine(ImageLoader.CustomImageDirectory, fileName), ImageFormat.Png);
                _image.Text = fileName;
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is OutOfMemoryException || ex is System.Runtime.InteropServices.ExternalException)
            { Error("Не удалось загрузить картинку: " + ex.Message); }
        }
        private void Error(string text) { _status.ForeColor = UiStyle.Red; _status.Text = text; MessageBox.Show(this, text, "Проверьте настройки", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        private void Save(object sender, EventArgs e)
        {
            if (!ApplyEditor()) return;
            try
            {
                var config = _original with { Name = _machineName.Text.Trim(), Menu = _draft.ToArray() };
                config.Validate(); _save(config); _dirty = false; DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IOException || ex is UnauthorizedAccessException) { Error(ex.Message); }
        }
    }
}
