namespace CoffeeMachineApp.Forms.Modules
{
    partial class DispensingControl
    {
        private System.ComponentModel.IContainer components = null;
        private CoffeeMachineApp.Forms.PreparationView preparationView;
        private System.Windows.Forms.Label bayTitleLabel;
        private System.Windows.Forms.Label bayDescriptionLabel;
        private CoffeeMachineApp.Forms.RoundedButton collectButton;
        private CoffeeMachineApp.Forms.ChangeTrayView changeTrayView;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.preparationView = new CoffeeMachineApp.Forms.PreparationView();
            this.bayTitleLabel = new System.Windows.Forms.Label();
            this.bayDescriptionLabel = new System.Windows.Forms.Label();
            this.collectButton = new CoffeeMachineApp.Forms.RoundedButton();
            this.changeTrayView = new CoffeeMachineApp.Forms.ChangeTrayView();
            this.SuspendLayout();
            //
            // preparationView
            //
            this.preparationView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
            this.preparationView.BackColor = System.Drawing.Color.FromArgb(21, 27, 23);
            this.preparationView.Location = new System.Drawing.Point(8, 8);
            this.preparationView.Name = "preparationView";
            this.preparationView.Size = new System.Drawing.Size(240, 142);
            this.preparationView.TabIndex = 0;
            //
            // bayTitleLabel
            //
            this.bayTitleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.bayTitleLabel.AutoEllipsis = true;
            this.bayTitleLabel.BackColor = System.Drawing.Color.Transparent;
            this.bayTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.bayTitleLabel.ForeColor = System.Drawing.Color.FromArgb(245, 241, 228);
            this.bayTitleLabel.Location = new System.Drawing.Point(258, 15);
            this.bayTitleLabel.Name = "bayTitleLabel";
            this.bayTitleLabel.Size = new System.Drawing.Size(156, 25);
            this.bayTitleLabel.TabIndex = 1;
            this.bayTitleLabel.Text = "Ваш кофе";
            this.bayTitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bayTitleLabel.UseMnemonic = false;
            //
            // bayDescriptionLabel
            //
            this.bayDescriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right));
            this.bayDescriptionLabel.AutoEllipsis = true;
            this.bayDescriptionLabel.BackColor = System.Drawing.Color.Transparent;
            this.bayDescriptionLabel.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.bayDescriptionLabel.ForeColor = System.Drawing.Color.FromArgb(157, 173, 148);
            this.bayDescriptionLabel.Location = new System.Drawing.Point(258, 44);
            this.bayDescriptionLabel.Name = "bayDescriptionLabel";
            this.bayDescriptionLabel.Size = new System.Drawing.Size(156, 40);
            this.bayDescriptionLabel.TabIndex = 2;
            this.bayDescriptionLabel.Text = "Свежий помол.\nТочный рецепт.";
            this.bayDescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.bayDescriptionLabel.UseMnemonic = false;
            //
            // collectButton
            //
            this.collectButton.Anchor = ((System.Windows.Forms.AnchorStyles)(System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right));
            this.collectButton.Dark = true;
            this.collectButton.Enabled = false;
            this.collectButton.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.collectButton.Location = new System.Drawing.Point(256, 94);
            this.collectButton.Name = "collectButton";
            this.collectButton.Primary = true;
            this.collectButton.Size = new System.Drawing.Size(160, 43);
            this.collectButton.TabIndex = 3;
            this.collectButton.Text = "Забрать кофе";
            this.collectButton.Click += new System.EventHandler(this.CollectButton_Click);
            //
            // changeTrayView
            //
            this.changeTrayView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                | System.Windows.Forms.AnchorStyles.Right));
            this.changeTrayView.BackColor = System.Drawing.Color.FromArgb(21, 27, 23);
            this.changeTrayView.Enabled = false;
            this.changeTrayView.Location = new System.Drawing.Point(426, 10);
            this.changeTrayView.Name = "changeTrayView";
            this.changeTrayView.Size = new System.Drawing.Size(154, 134);
            this.changeTrayView.TabIndex = 4;
            this.changeTrayView.Click += new System.EventHandler(this.ChangeTrayView_Click);
            //
            // DispensingControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(21, 27, 23);
            this.Size = new System.Drawing.Size(592, 156);
            this.Controls.Add(this.preparationView);
            this.Controls.Add(this.bayTitleLabel);
            this.Controls.Add(this.bayDescriptionLabel);
            this.Controls.Add(this.collectButton);
            this.Controls.Add(this.changeTrayView);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "DispensingControl";
            this.ResumeLayout(false);
        }
    }
}
