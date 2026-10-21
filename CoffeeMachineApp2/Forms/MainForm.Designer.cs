namespace CoffeeMachineApp.Forms
{
    partial class MainForm
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

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.coffeeMachineControl = new CoffeeMachineApp.Forms.CoffeeMachineControl();
            this.SuspendLayout();
            // 
            // coffeeMachineControl
            // 
            this.coffeeMachineControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.coffeeMachineControl.Location = new System.Drawing.Point(0, 0);
            this.coffeeMachineControl.Name = "coffeeMachineControl";
            this.coffeeMachineControl.Size = new System.Drawing.Size(1240, 830);
            this.coffeeMachineControl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoScroll = true;
            this.AutoScrollMinSize = new System.Drawing.Size(1240, 830);
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1240, 830);
            this.Controls.Add(this.coffeeMachineControl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Coffee Machine";
            this.ResumeLayout(false);

        }

        #endregion

        private CoffeeMachineControl coffeeMachineControl;
    }
}
