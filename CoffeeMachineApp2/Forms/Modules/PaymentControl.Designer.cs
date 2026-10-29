namespace CoffeeMachineApp.Forms.Modules
{
    partial class PaymentControl
    {
        private System.Windows.Forms.TableLayoutPanel paymentLayout;
        private System.ComponentModel.IContainer components = null;
        private CoffeeMachineApp.Forms.CardSlotView _cardSlot;
        private System.Windows.Forms.Panel _display;
        private System.Windows.Forms.Label _amountCaption;
        private System.Windows.Forms.Label _terminalTotal;
        private System.Windows.Forms.Label _wallet;
        private CoffeeMachineApp.Forms.RoundedButton _cash;
        private CoffeeMachineApp.Forms.RoundedButton _card;
        private System.Windows.Forms.Label _notesCaption;
        private CoffeeMachineApp.Forms.RoundedButton _note10;
        private CoffeeMachineApp.Forms.RoundedButton _note50;
        private CoffeeMachineApp.Forms.RoundedButton _note100;
        private CoffeeMachineApp.Forms.RoundedButton _note500;
        private CoffeeMachineApp.Forms.RoundedButton _pay;
        private System.Windows.Forms.Label _trainingCaption;
        private System.Windows.Forms.Label _secureCaption;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            _cardSlot = new CoffeeMachineApp.Forms.CardSlotView();
            _display = new System.Windows.Forms.Panel();
            _amountCaption = new System.Windows.Forms.Label();
            _terminalTotal = new System.Windows.Forms.Label();
            _wallet = new System.Windows.Forms.Label();
            _cash = new CoffeeMachineApp.Forms.RoundedButton();
            _card = new CoffeeMachineApp.Forms.RoundedButton();
            _notesCaption = new System.Windows.Forms.Label();
            _note10 = new CoffeeMachineApp.Forms.RoundedButton();
            _note50 = new CoffeeMachineApp.Forms.RoundedButton();
            _note100 = new CoffeeMachineApp.Forms.RoundedButton();
            _note500 = new CoffeeMachineApp.Forms.RoundedButton();
            _pay = new CoffeeMachineApp.Forms.RoundedButton();
            _trainingCaption = new System.Windows.Forms.Label();
            _secureCaption = new System.Windows.Forms.Label();
            paymentLayout = new System.Windows.Forms.TableLayoutPanel();
            paymentLayout.SuspendLayout();
            _display.SuspendLayout();
            SuspendLayout();
            //
            // _cardSlot
            //
            _cardSlot.Location = new System.Drawing.Point(8, 5);
            _cardSlot.Name = "_cardSlot";
            _cardSlot.Size = new System.Drawing.Size(136, 132);
            _cardSlot.TabIndex = 0;
            _cardSlot.Click += new System.EventHandler(CardSlotClick);
            _cardSlot.AnimationFinished += new System.EventHandler(CardSlotAnimationFinished);
            //
            // _display
            //
            _display.BackColor = System.Drawing.Color.FromArgb(19, 27, 22);
            _display.Controls.Add(_amountCaption);
            _display.Controls.Add(_terminalTotal);
            _display.Controls.Add(_wallet);
            _display.Location = new System.Drawing.Point(10, 141);
            _display.Name = "_display";
            _display.Size = new System.Drawing.Size(132, 105);
            _display.TabIndex = 1;
            //
            // _amountCaption
            //
            _amountCaption.AutoEllipsis = true;
            _amountCaption.BackColor = System.Drawing.Color.Transparent;
            _amountCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            _amountCaption.ForeColor = System.Drawing.Color.FromArgb(138, 167, 131);
            _amountCaption.Location = new System.Drawing.Point(9, 6);
            _amountCaption.Name = "_amountCaption";
            _amountCaption.Size = new System.Drawing.Size(114, 18);
            _amountCaption.TabIndex = 0;
            _amountCaption.Text = "К ОПЛАТЕ";
            _amountCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _amountCaption.UseMnemonic = false;
            //
            // _terminalTotal
            //
            _terminalTotal.AutoEllipsis = true;
            _terminalTotal.BackColor = System.Drawing.Color.Transparent;
            _terminalTotal.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            _terminalTotal.ForeColor = System.Drawing.Color.FromArgb(183, 220, 162);
            _terminalTotal.Location = new System.Drawing.Point(4, 27);
            _terminalTotal.Name = "_terminalTotal";
            _terminalTotal.Size = new System.Drawing.Size(124, 37);
            _terminalTotal.TabIndex = 1;
            _terminalTotal.Text = "— ₽";
            _terminalTotal.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _terminalTotal.UseMnemonic = false;
            //
            // _wallet
            //
            _wallet.AutoEllipsis = true;
            _wallet.BackColor = System.Drawing.Color.Transparent;
            _wallet.Font = new System.Drawing.Font("Segoe UI", 8F);
            _wallet.ForeColor = System.Drawing.Color.FromArgb(138, 167, 131);
            _wallet.Location = new System.Drawing.Point(7, 68);
            _wallet.Name = "_wallet";
            _wallet.Size = new System.Drawing.Size(118, 32);
            _wallet.TabIndex = 2;
            _wallet.Text = "Выберите кофе";
            _wallet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _wallet.UseMnemonic = false;
            //
            // _cash
            //
            _cash.Dark = true;
            _cash.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _cash.Location = new System.Drawing.Point(10, 252);
            _cash.Name = "_cash";
            _cash.Size = new System.Drawing.Size(132, 32);
            _cash.TabIndex = 2;
            _cash.Text = "Наличные";
            _cash.Click += new System.EventHandler(CashClick);
            //
            // _card
            //
            _card.Dark = true;
            _card.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _card.Location = new System.Drawing.Point(10, 290);
            _card.Name = "_card";
            _card.Size = new System.Drawing.Size(132, 32);
            _card.TabIndex = 3;
            _card.Text = "Карта";
            _card.Click += new System.EventHandler(CardClick);
            //
            // _notesCaption
            //
            _notesCaption.AutoEllipsis = true;
            _notesCaption.BackColor = System.Drawing.Color.Transparent;
            _notesCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            _notesCaption.ForeColor = System.Drawing.Color.FromArgb(157, 169, 146);
            _notesCaption.Location = new System.Drawing.Point(10, 327);
            _notesCaption.Name = "_notesCaption";
            _notesCaption.Size = new System.Drawing.Size(132, 19);
            _notesCaption.TabIndex = 4;
            _notesCaption.Text = "ВНЕСИТЕ КУПЮРУ";
            _notesCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _notesCaption.UseMnemonic = false;
            //
            // _note10
            //
            _note10.Dark = true;
            _note10.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _note10.Location = new System.Drawing.Point(10, 348);
            _note10.Name = "_note10";
            _note10.Size = new System.Drawing.Size(63, 35);
            _note10.TabIndex = 5;
            _note10.Text = "+10";
            _note10.Click += new System.EventHandler(Note10Click);
            //
            // _note50
            //
            _note50.Dark = true;
            _note50.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _note50.Location = new System.Drawing.Point(79, 348);
            _note50.Name = "_note50";
            _note50.Size = new System.Drawing.Size(63, 35);
            _note50.TabIndex = 6;
            _note50.Text = "+50";
            _note50.Click += new System.EventHandler(Note50Click);
            //
            // _note100
            //
            _note100.Dark = true;
            _note100.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _note100.Location = new System.Drawing.Point(10, 389);
            _note100.Name = "_note100";
            _note100.Size = new System.Drawing.Size(63, 35);
            _note100.TabIndex = 7;
            _note100.Text = "+100";
            _note100.Click += new System.EventHandler(Note100Click);
            //
            // _note500
            //
            _note500.Dark = true;
            _note500.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _note500.Location = new System.Drawing.Point(79, 389);
            _note500.Name = "_note500";
            _note500.Size = new System.Drawing.Size(63, 35);
            _note500.TabIndex = 8;
            _note500.Text = "+500";
            _note500.Click += new System.EventHandler(Note500Click);
            //
            // _pay
            //
            _pay.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            _pay.Location = new System.Drawing.Point(10, 446);
            _pay.Name = "_pay";
            _pay.Primary = true;
            _pay.Size = new System.Drawing.Size(132, 47);
            _pay.TabIndex = 9;
            _pay.Text = "Оплатить →";
            _pay.Click += new System.EventHandler(PayClick);
            //
            // _trainingCaption
            //
            _trainingCaption.AutoEllipsis = true;
            _trainingCaption.BackColor = System.Drawing.Color.Transparent;
            _trainingCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            _trainingCaption.ForeColor = System.Drawing.Color.FromArgb(137, 147, 129);
            _trainingCaption.Location = new System.Drawing.Point(10, 509);
            _trainingCaption.Name = "_trainingCaption";
            _trainingCaption.Size = new System.Drawing.Size(132, 40);
            _trainingCaption.TabIndex = 10;
            _trainingCaption.Text = "Учебный терминал\nБез реального списания";
            _trainingCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _trainingCaption.UseMnemonic = false;
            //
            // _secureCaption
            //
            _secureCaption.AutoEllipsis = true;
            _secureCaption.BackColor = System.Drawing.Color.Transparent;
            _secureCaption.Font = new System.Drawing.Font("Segoe UI", 8F);
            _secureCaption.ForeColor = System.Drawing.Color.FromArgb(137, 147, 129);
            _secureCaption.Location = new System.Drawing.Point(10, 597);
            _secureCaption.Name = "_secureCaption";
            _secureCaption.Size = new System.Drawing.Size(132, 20);
            _secureCaption.TabIndex = 11;
            _secureCaption.Text = "SECURE  /  COFFEE PAY";
            _secureCaption.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            _secureCaption.UseMnemonic = false;
            //
            // PaymentControl
            //
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.Color.FromArgb(35, 40, 35);
            Size = new System.Drawing.Size(152, 640);
            Font = new System.Drawing.Font("Segoe UI", 9.5F);
            Name = "PaymentControl";
            paymentLayout.Name = "paymentLayout";
            paymentLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            paymentLayout.ColumnCount = 2;
            paymentLayout.RowCount = 9;
            paymentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            paymentLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            Padding = new System.Windows.Forms.Padding(6);
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 0F));
            paymentLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize, 0F));
            _cardSlot.Dock = System.Windows.Forms.DockStyle.Fill;
            _cardSlot.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_cardSlot, 0, 0);
            paymentLayout.SetColumnSpan(_cardSlot, 2);
            _display.Dock = System.Windows.Forms.DockStyle.Fill;
            _display.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_display, 0, 1);
            paymentLayout.SetColumnSpan(_display, 2);
            _cash.Dock = System.Windows.Forms.DockStyle.Fill;
            _cash.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_cash, 0, 2);
            _card.Dock = System.Windows.Forms.DockStyle.Fill;
            _card.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_card, 1, 2);
            _notesCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            _notesCaption.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_notesCaption, 0, 3);
            paymentLayout.SetColumnSpan(_notesCaption, 2);
            _note10.Dock = System.Windows.Forms.DockStyle.Fill;
            _note10.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_note10, 0, 4);
            _note50.Dock = System.Windows.Forms.DockStyle.Fill;
            _note50.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_note50, 1, 4);
            _note100.Dock = System.Windows.Forms.DockStyle.Fill;
            _note100.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_note100, 0, 5);
            _note500.Dock = System.Windows.Forms.DockStyle.Fill;
            _note500.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_note500, 1, 5);
            _pay.Dock = System.Windows.Forms.DockStyle.Fill;
            _pay.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_pay, 0, 6);
            paymentLayout.SetColumnSpan(_pay, 2);
            _trainingCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            _trainingCaption.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_trainingCaption, 0, 7);
            paymentLayout.SetColumnSpan(_trainingCaption, 2);
            _secureCaption.Dock = System.Windows.Forms.DockStyle.Fill;
            _secureCaption.Margin = new System.Windows.Forms.Padding(2);
            paymentLayout.Controls.Add(_secureCaption, 0, 8);
            paymentLayout.SetColumnSpan(_secureCaption, 2);
            _amountCaption.Dock = System.Windows.Forms.DockStyle.Top;
            _amountCaption.Height = 18;
            _wallet.Dock = System.Windows.Forms.DockStyle.Bottom;
            _wallet.Height = 32;
            _terminalTotal.Dock = System.Windows.Forms.DockStyle.Fill;
            _terminalTotal.BringToFront();
            Controls.Add(paymentLayout);
            paymentLayout.ResumeLayout(false);
            _display.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
