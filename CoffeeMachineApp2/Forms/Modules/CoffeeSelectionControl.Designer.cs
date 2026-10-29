namespace CoffeeMachineApp.Forms.Modules
{
    partial class CoffeeSelectionControl
    {
        private System.Windows.Forms.TableLayoutPanel selectionLayout;
        private BeverageMenuControl menuModule;
        private OrderOptionsControl orderModule;
        private void InitializeComponent()
        {
            menuModule = new BeverageMenuControl();
            orderModule = new OrderOptionsControl();
            selectionLayout = new System.Windows.Forms.TableLayoutPanel();
            SuspendLayout();
            selectionLayout.SuspendLayout();
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
            selectionLayout.Name = "selectionLayout";
            selectionLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            selectionLayout.ColumnCount = 1;
            selectionLayout.RowCount = 2;
            selectionLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            selectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            selectionLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 172F));
            Padding = new System.Windows.Forms.Padding(8);
            menuModule.Dock = System.Windows.Forms.DockStyle.Fill;
            orderModule.Dock = System.Windows.Forms.DockStyle.Fill;
            menuModule.Margin = System.Windows.Forms.Padding.Empty;
            orderModule.Margin = System.Windows.Forms.Padding.Empty;
            selectionLayout.Controls.Add(menuModule, 0, 0);
            selectionLayout.Controls.Add(orderModule, 0, 1);
            Controls.Add(selectionLayout);
            selectionLayout.ResumeLayout(false);
            ResumeLayout(false);
        }
    }
}
