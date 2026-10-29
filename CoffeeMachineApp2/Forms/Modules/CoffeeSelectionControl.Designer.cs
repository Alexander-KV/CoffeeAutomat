namespace CoffeeMachineApp.Forms.Modules
{
    partial class CoffeeSelectionControl
    {
        private BeverageMenuControl menuModule;
        private OrderOptionsControl orderModule;
        private void InitializeComponent()
        {
            menuModule = new BeverageMenuControl();
            orderModule = new OrderOptionsControl();
            SuspendLayout();
            menuModule.Location = new System.Drawing.Point(12,12);
            menuModule.Name = "menuModule";
            menuModule.Size = new System.Drawing.Size(398,412);
            menuModule.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            orderModule.Location = new System.Drawing.Point(12,424);
            orderModule.Name = "orderModule";
            orderModule.Size = new System.Drawing.Size(398,204);
            orderModule.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            AutoScaleDimensions = new System.Drawing.SizeF(96,96);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            BackColor = System.Drawing.Color.FromArgb(248,247,240);
            Font = new System.Drawing.Font("Segoe UI",9.5F);
            Name = "CoffeeSelectionControl";
            Size = new System.Drawing.Size(422,640);
            Controls.Add(menuModule);
            Controls.Add(orderModule);
            ResumeLayout(false);
        }
    }
}
