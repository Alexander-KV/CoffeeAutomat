using System.Drawing;
using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    partial class MainForm
    {
        private FlowLayoutPanel _floor;
        private Label _count;
        private Label _modeHint;
        private RoundedButton _add;
        private RoundedButton _adminButton;
        private Panel header;
        private Panel actions;
        private Panel identity;
        private Label brand;
        private Label title;
        private Label footer;
        private CoffeeMachineControl machinePreview;

        private void InitializeComponent()
        {
            _floor = new FlowLayoutPanel();
            _count = new Label();
            _modeHint = new Label();
            _add = new RoundedButton();
            _adminButton = new RoundedButton();
            header = new Panel();
            actions = new Panel();
            identity = new Panel();
            brand = new Label();
            title = new Label();
            footer = new Label();
            machinePreview = new CoffeeMachineControl();
            SuspendLayout();
            header.SuspendLayout();
            actions.SuspendLayout();
            identity.SuspendLayout();
            _floor.SuspendLayout();
            // header
            header.Name = "header";
            header.Dock = DockStyle.Top;
            header.Height = 126;
            header.Padding = new Padding(28, 16, 28, 12);
            header.BackColor = Color.FromArgb(248, 247, 240);
            // actions
            actions.Name = "actions";
            actions.Dock = DockStyle.Right;
            actions.Width = 260;
            // _adminButton
            _adminButton.Name = "_adminButton";
            _adminButton.Text = "Администратор";
            _adminButton.Location = new Point(20, 6);
            _adminButton.Size = new Size(240, 38);
            _adminButton.Click += SwitchMode;
            // _add
            _add.Name = "_add";
            _add.Text = "+ Новый автомат";
            _add.Primary = true;
            _add.Location = new Point(20, 53);
            _add.Size = new Size(240, 38);
            _add.Visible = false;
            _add.Click += AddMachineClick;
            // _modeHint
            _modeHint.Name = "_modeHint";
            _modeHint.Text = "Режим покупателя";
            _modeHint.Location = new Point(0, 62);
            _modeHint.Size = new Size(260, 22);
            _modeHint.TextAlign = ContentAlignment.MiddleRight;
            _modeHint.ForeColor = Color.FromArgb(120, 126, 113);
            _modeHint.Font = new Font("Segoe UI", 8F);
            // identity
            identity.Name = "identity";
            identity.Dock = DockStyle.Fill;
            // brand
            brand.Name = "brand";
            brand.Text = "B E A N  &  C U P    /    COFFEE ATELIER";
            brand.Dock = DockStyle.Top;
            brand.Height = 19;
            brand.Font = new Font("Segoe UI", 8F);
            brand.ForeColor = Color.FromArgb(41, 79, 57);
            brand.TextAlign = ContentAlignment.MiddleLeft;
            brand.AutoEllipsis = true;
            // title
            title.Name = "title";
            title.Text = "Кофе. В вашем ритме.";
            title.Location = new Point(0, 21);
            title.Size = new Size(650, 47);
            title.Font = new Font("Georgia", 26F);
            title.ForeColor = Color.FromArgb(36, 43, 36);
            title.TextAlign = ContentAlignment.MiddleLeft;
            title.AutoEllipsis = true;
            // _count
            _count.Name = "_count";
            _count.Text = "Автоматов: 1  ·  Выберите напиток, настройте под себя и заберите свежую чашку.";
            _count.Location = new Point(0, 71);
            _count.Size = new Size(650, 24);
            _count.ForeColor = Color.FromArgb(120, 126, 113);
            _count.TextAlign = ContentAlignment.MiddleLeft;
            _count.AutoEllipsis = true;
            // _floor
            _floor.Name = "_floor";
            _floor.Dock = DockStyle.Fill;
            _floor.AutoScroll = true;
            _floor.WrapContents = false;
            _floor.FlowDirection = FlowDirection.LeftToRight;
            _floor.Padding = new Padding(14, 18, 14, 12);
            // machinePreview
            machinePreview.Name = "machinePreview";
            machinePreview.Location = new Point(28, 26);
            machinePreview.Size = new Size(680, 860);
            machinePreview.Margin = new Padding(14, 8, 14, 24);
            // footer
            footer.Name = "footer";
            footer.Text = "СВЕЖИЙ ПОМОЛ     /     АВТОРСКИЕ РЕЦЕПТЫ     /     МАЛЕНЬКАЯ ПАУЗА ДЛЯ СЕБЯ";
            footer.Dock = DockStyle.Bottom;
            footer.Height = 34;
            footer.ForeColor = Color.FromArgb(120, 126, 113);
            footer.Font = new Font("Segoe UI", 8F);
            footer.Padding = new Padding(28, 0, 0, 0);
            footer.BackColor = Color.FromArgb(248, 247, 240);
            footer.TextAlign = ContentAlignment.MiddleLeft;
            footer.AutoEllipsis = true;
            actions.Controls.Add(_adminButton);
            actions.Controls.Add(_add);
            actions.Controls.Add(_modeHint);
            identity.Controls.Add(_count);
            identity.Controls.Add(title);
            identity.Controls.Add(brand);
            header.Controls.Add(identity);
            header.Controls.Add(actions);
            _floor.Controls.Add(machinePreview);
            Font = new Font("Segoe UI", 9.5F);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(232, 231, 220);
            ClientSize = new Size(1440, 1040);
            MinimumSize = new Size(740, 640);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Text = "BEAN & CUP — кофе в вашем ритме";
            Controls.Add(_floor);
            Controls.Add(footer);
            Controls.Add(header);
            _floor.ResumeLayout(false);
            identity.ResumeLayout(false);
            actions.ResumeLayout(false);
            header.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
