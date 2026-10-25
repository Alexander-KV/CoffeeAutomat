using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp.Forms
{
    public sealed class AdminLoginForm : Form
    {
        private readonly AdminAccess _access;
        private readonly bool _setup;
        private readonly TextBox _password = new TextBox { UseSystemPasswordChar = true, MaxLength = 128 };
        private readonly TextBox _confirmation = new TextBox { UseSystemPasswordChar = true, MaxLength = 128 };
        private readonly Label _error = new Label();
        public AdminLoginForm(AdminAccess access)
        {
            _access = access; _setup = !access.HasPassword;
            Text = _setup ? "Первичная настройка администратора" : "Вход администратора";
            Font = UiStyle.Regular; BackColor = UiStyle.Cream;
            AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96);
            ClientSize = new Size(490, 370); FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = MinimizeBox = false; StartPosition = FormStartPosition.CenterParent;
            var title = new Label { Text = _setup ? "Ваш пароль доступа" : "Режим администратора", Font = UiStyle.Display,
                ForeColor = UiStyle.Ink, Bounds = new Rectangle(24, 18, 444, 43) };
            var description = new Label { Text = _setup ? "При первом входе задайте пароль владельца.\nОн понадобится для изменения меню и автоматов." : "Введите пароль, чтобы управлять меню, ценами\nи составом автоматов.",
                ForeColor = UiStyle.Gray, Bounds = new Rectangle(26, 69, 438, 44) };
            var passwordLabel = new Label { Text = _setup ? "Пароль (не менее 6 символов)" : "Пароль", Bounds = new Rectangle(26, 121, 438, 22) };
            _password.Bounds = new Rectangle(26, 146, 438, 28); _password.AccessibleName = "Пароль администратора";
            var confirmationLabel = new Label { Text = "Повторите пароль", Bounds = new Rectangle(26, 181, 438, 22), Visible = _setup };
            _confirmation.Bounds = new Rectangle(26, 206, 438, 28); _confirmation.Visible = _setup; _confirmation.AccessibleName = "Подтверждение пароля";
            var show = new CheckBox { Text = "Показать пароль", AutoSize = true, Location = new Point(26, _setup ? 241 : 188) };
            show.CheckedChanged += (s,e) => _password.UseSystemPasswordChar = _confirmation.UseSystemPasswordChar = !show.Checked;
            _error.Bounds = new Rectangle(26, 268, 438, 41); _error.ForeColor = UiStyle.Red;
            var cancel = UiStyle.Button("Отмена", (s,e) => { DialogResult = DialogResult.Cancel; Close(); }); cancel.Bounds = new Rectangle(160, 318, 125, 36);
            var login = UiStyle.Button(_setup ? "Создать и войти" : "Войти", Login, true); login.Bounds = new Rectangle(297, 318, 167, 36);
            Controls.AddRange(new Control[] { title, description, passwordLabel, _password, confirmationLabel, _confirmation, show, _error, cancel, login });
            AcceptButton = login; CancelButton = cancel; Shown += (s,e) => _password.Focus();
        }
        private void Login(object sender, EventArgs e)
        {
            try
            {
                if (_setup)
                {
                    if (_password.Text != _confirmation.Text) { _error.Text = "Пароли не совпадают."; _confirmation.Focus(); return; }
                    _access.CreatePassword(_password.Text);
                }
                else if (!_access.Login(_password.Text))
                {
                    _error.Text = "Неверный пароль. Попробуйте ещё раз."; _password.SelectAll(); _password.Focus(); return;
                }
                _password.Clear(); _confirmation.Clear(); DialogResult = DialogResult.OK; Close();
            }
            catch (Exception ex) when (ex is IOException || ex is InvalidDataException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is InvalidOperationException)
            { _error.Text = ex.Message; }
        }
    }
}
