namespace CoffeeMachineApp.Forms
{
    partial class CoffeeMachineControl
    {
        private System.ComponentModel.IContainer components = null;
        private Modules.CoffeeSelectionControl selectionModule;
        private Modules.PaymentControl paymentModule;
        private Modules.DispensingControl dispensingModule;
        private System.Windows.Forms.Label brandLabel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label serialLabel;
        private System.Windows.Forms.Label messageLabel;
        private RoundedButton adminButton;
        private RoundedButton copyButton;
        private RoundedButton removeButton;
        private System.Windows.Forms.Timer brewingTimer;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            selectionModule = new Modules.CoffeeSelectionControl();
            paymentModule = new Modules.PaymentControl();
            dispensingModule = new Modules.DispensingControl();
            brandLabel = new System.Windows.Forms.Label();
            titleLabel = new System.Windows.Forms.Label();
            serialLabel = new System.Windows.Forms.Label();
            messageLabel = new System.Windows.Forms.Label();
            adminButton = new RoundedButton();
            copyButton = new RoundedButton();
            removeButton = new RoundedButton();
            brewingTimer = new System.Windows.Forms.Timer(components);
            SuspendLayout();
            // selectionModule
            selectionModule.Location = new System.Drawing.Point(24, 100);
            selectionModule.Name = "selectionModule";
            selectionModule.Size = new System.Drawing.Size(422, 640);
            selectionModule.TabIndex = 0;
            // paymentModule
            paymentModule.Location = new System.Drawing.Point(464, 100);
            paymentModule.Name = "paymentModule";
            paymentModule.Size = new System.Drawing.Size(152, 640);
            paymentModule.TabIndex = 1;
            // dispensingModule
            dispensingModule.Location = new System.Drawing.Point(24, 782);
            dispensingModule.Name = "dispensingModule";
            dispensingModule.Size = new System.Drawing.Size(592, 156);
            dispensingModule.TabIndex = 2;
            // brandLabel
            brandLabel.AutoEllipsis = true;
            brandLabel.BackColor = System.Drawing.Color.Transparent;
            brandLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            brandLabel.ForeColor = System.Drawing.Color.FromArgb(188, 151, 91);
            brandLabel.Location = new System.Drawing.Point(32, 19);
            brandLabel.Name = "brandLabel";
            brandLabel.Size = new System.Drawing.Size(330, 18);
            brandLabel.Text = "B E A N  &  C U P";
            brandLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            brandLabel.UseMnemonic = false;
            // titleLabel
            titleLabel.AutoEllipsis = true;
            titleLabel.BackColor = System.Drawing.Color.Transparent;
            titleLabel.Font = new System.Drawing.Font("Georgia", 23F);
            titleLabel.ForeColor = System.Drawing.Color.FromArgb(245, 241, 228);
            titleLabel.Location = new System.Drawing.Point(30, 39);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(470, 38);
            titleLabel.Text = "Кофейня №1";
            titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            titleLabel.UseMnemonic = false;
            // serialLabel
            serialLabel.AutoEllipsis = true;
            serialLabel.BackColor = System.Drawing.Color.Transparent;
            serialLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            serialLabel.ForeColor = System.Drawing.Color.FromArgb(171, 177, 159);
            serialLabel.Location = new System.Drawing.Point(490, 27);
            serialLabel.Name = "serialLabel";
            serialLabel.Size = new System.Drawing.Size(116, 42);
            serialLabel.Text = "CRAFTED COFFEE\n№ PREVIEW";
            serialLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            serialLabel.UseMnemonic = false;
            // messageLabel
            messageLabel.AutoEllipsis = true;
            messageLabel.BackColor = System.Drawing.Color.Transparent;
            messageLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            messageLabel.ForeColor = System.Drawing.Color.FromArgb(170, 199, 147);
            messageLabel.Location = new System.Drawing.Point(33, 746);
            messageLabel.Name = "messageLabel";
            messageLabel.Size = new System.Drawing.Size(574, 28);
            messageLabel.Text = "Готов к работе. Выберите свой кофе.";
            messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            messageLabel.UseMnemonic = false;
            // adminButton
            adminButton.Dark = true;
            adminButton.Font = new System.Drawing.Font("Segoe UI", 8F);
            adminButton.Location = new System.Drawing.Point(26, 952);
            adminButton.Name = "adminButton";
            adminButton.Size = new System.Drawing.Size(132, 30);
            adminButton.Text = "Настройки";
            adminButton.TabIndex = 3;
            adminButton.Click += OpenAdmin;
            // copyButton
            copyButton.Dark = true;
            copyButton.Font = new System.Drawing.Font("Segoe UI", 8F);
            copyButton.Location = new System.Drawing.Point(168, 952);
            copyButton.Name = "copyButton";
            copyButton.Size = new System.Drawing.Size(226, 30);
            copyButton.Text = "+ Копировать автомат";
            copyButton.TabIndex = 4;
            copyButton.Click += CopyMachine;
            // removeButton
            removeButton.Dark = true;
            removeButton.Font = new System.Drawing.Font("Segoe UI", 8F);
            removeButton.Location = new System.Drawing.Point(434, 952);
            removeButton.Name = "removeButton";
            removeButton.Size = new System.Drawing.Size(174, 30);
            removeButton.Text = "Убрать автомат";
            removeButton.TabIndex = 5;
            removeButton.Click += RemoveMachine;
            // brewingTimer
            brewingTimer.Interval = 40;
            brewingTimer.Tick += Tick;
            // CoffeeMachineControl
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.FromArgb(232, 231, 220);
            Font = new System.Drawing.Font("Segoe UI", 9.5F);
            Margin = new System.Windows.Forms.Padding(14, 8, 14, 24);
            Name = "CoffeeMachineControl";
            Size = new System.Drawing.Size(640, 1000);
            Controls.Add(selectionModule);
            Controls.Add(paymentModule);
            Controls.Add(dispensingModule);
            Controls.Add(brandLabel);
            Controls.Add(titleLabel);
            Controls.Add(serialLabel);
            Controls.Add(messageLabel);
            Controls.Add(adminButton);
            Controls.Add(copyButton);
            Controls.Add(removeButton);
            ResumeLayout(false);
        }
    }
}
