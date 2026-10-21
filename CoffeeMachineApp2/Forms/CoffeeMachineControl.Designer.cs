namespace CoffeeMachineApp.Forms
{
    partial class CoffeeMachineControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Расположение модулей. Можно спокойно перетаскивать модули мышью
        /// и менять им размер в окне дизайнера Visual Studio.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.btnAdmin = new CoffeeMachineApp.Forms.RoundedButton();
            this.headerLine = new System.Windows.Forms.Panel();
            this.menuModule = new CoffeeMachineApp.Forms.MenuModule();
            this.orderModule = new CoffeeMachineApp.Forms.OrderModule();
            this.paymentModule = new CoffeeMachineApp.Forms.PaymentModule();
            this.preparationModule = new CoffeeMachineApp.Forms.PreparationModule();
            this.lblNotify = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = false;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(111)))), ((int)(((byte)(78)))), ((int)(((byte)(55)))));
            this.lblTitle.Location = new System.Drawing.Point(16, 6);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(420, 44);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "COFFEE MACHINE";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblState
            // 
            this.lblState.AutoSize = false;
            this.lblState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(125)))), ((int)(((byte)(117)))), ((int)(((byte)(110)))));
            this.lblState.Location = new System.Drawing.Point(500, 16);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(540, 26);
            this.lblState.TabIndex = 1;
            this.lblState.Text = "Состояние:";
            this.lblState.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnAdmin
            // 
            this.btnAdmin.Location = new System.Drawing.Point(1056, 10);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(168, 36);
            this.btnAdmin.TabIndex = 2;
            this.btnAdmin.Text = "Администратор";
            this.btnAdmin.Click += new System.EventHandler(this.btnAdmin_Click);
            // 
            // headerLine
            // 
            this.headerLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(218)))), ((int)(((byte)(209)))));
            this.headerLine.Location = new System.Drawing.Point(0, 55);
            this.headerLine.Name = "headerLine";
            this.headerLine.Size = new System.Drawing.Size(1240, 1);
            this.headerLine.TabIndex = 3;
            // 
            // menuModule
            // 
            this.menuModule.Location = new System.Drawing.Point(16, 64);
            this.menuModule.Name = "menuModule";
            this.menuModule.Size = new System.Drawing.Size(560, 718);
            this.menuModule.TabIndex = 4;
            this.menuModule.Title = "1 · Выберите напиток";
            // 
            // orderModule
            // 
            this.orderModule.Location = new System.Drawing.Point(588, 64);
            this.orderModule.Name = "orderModule";
            this.orderModule.Size = new System.Drawing.Size(636, 330);
            this.orderModule.TabIndex = 6;
            this.orderModule.Title = "2 · Ваш заказ";
            // 
            // paymentModule
            // 
            this.paymentModule.Location = new System.Drawing.Point(588, 402);
            this.paymentModule.Name = "paymentModule";
            this.paymentModule.Size = new System.Drawing.Size(316, 380);
            this.paymentModule.TabIndex = 7;
            this.paymentModule.Title = "3 · Оплата";
            // 
            // preparationModule
            // 
            this.preparationModule.Location = new System.Drawing.Point(912, 402);
            this.preparationModule.Name = "preparationModule";
            this.preparationModule.Size = new System.Drawing.Size(312, 380);
            this.preparationModule.TabIndex = 8;
            this.preparationModule.Title = "4 · Приготовление и выдача";
            // 
            // lblNotify
            // 
            this.lblNotify.AutoSize = false;
            this.lblNotify.BackColor = System.Drawing.Color.White;
            this.lblNotify.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblNotify.Location = new System.Drawing.Point(0, 790);
            this.lblNotify.Name = "lblNotify";
            this.lblNotify.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.lblNotify.Size = new System.Drawing.Size(1240, 40);
            this.lblNotify.TabIndex = 9;
            this.lblNotify.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CoffeeMachineControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(240)))), ((int)(((byte)(234)))));
            this.Controls.Add(this.lblNotify);
            this.Controls.Add(this.preparationModule);
            this.Controls.Add(this.paymentModule);
            this.Controls.Add(this.orderModule);
            this.Controls.Add(this.menuModule);
            this.Controls.Add(this.headerLine);
            this.Controls.Add(this.btnAdmin);
            this.Controls.Add(this.lblState);
            this.Controls.Add(this.lblTitle);
            this.Name = "CoffeeMachineControl";
            this.Size = new System.Drawing.Size(1240, 830);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblState;
        private RoundedButton btnAdmin;
        private System.Windows.Forms.Panel headerLine;
        private MenuModule menuModule;
        private OrderModule orderModule;
        private PaymentModule paymentModule;
        private PreparationModule preparationModule;
        private System.Windows.Forms.Label lblNotify;
    }
}
