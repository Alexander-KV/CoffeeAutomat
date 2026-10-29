using CoffeeMachineApp.Forms;
using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;
using System.Drawing.Imaging;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        try
        {
            using var host = new Form { AutoScaleMode = AutoScaleMode.None, ClientSize = new Size(1100, 1100) };
            using var machine = new CoffeeMachineControl();
            host.Controls.Add(machine);
            host.Show();
            Application.DoEvents();
            var price = Find(machine, "_terminalTotal");
            float font = price.Font.Size;
            foreach (var size in new[] { new Size(680, 860), new Size(600, 680), new Size(900, 1000), new Size(680, 860) })
            {
                machine.Size = size;
                Application.DoEvents();
                CheckModules(machine);
                CheckContents(Find(machine, "selectionModule"));
                CheckContents(Find(machine, "paymentModule"));
                CheckContents(Find(machine, "dispensingModule"));
                Assert(Math.Abs(price.Font.Size - font) < 0.1f, "Resize changed readable font size");
            }
            // Standard WinForms cell operations, no bespoke layout API.
            var table = (TableLayoutPanel)Find(machine, "modulesLayout");
            var menu = Find(machine, "selectionModule");
            var payment = Find(machine, "paymentModule");
            table.SuspendLayout();
            table.Controls.Remove(menu);
            table.Controls.Remove(payment);
            table.Controls.Add(payment, 0, 0);
            table.Controls.Add(menu, 1, 0);
            table.ColumnStyles[0].Width = 30;
            table.ColumnStyles[1].Width = 70;
            table.ResumeLayout(true);
            machine.Size = new Size(900, 1000);
            Application.DoEvents();
            Assert(payment.Left < menu.Left, "Standard cell swap was lost after resize");
            CheckModules(machine);
            host.Hide();

            using var app = new MainForm(new MemoryStore());
            app.WindowState = FormWindowState.Normal;
            app.Show();
            foreach (var size in new[] { new Size(1100, 900), new Size(800, 650), new Size(1400, 1000) })
            {
                app.ClientSize = size;
                Application.DoEvents();
                var floor = Find(app, "_floor");
                var machines = floor.Controls.OfType<CoffeeMachineControl>().ToArray();
                Assert(machines.Length == 2, "Independent machines missing");
                foreach (var view in machines)
                {
                    CheckModules(view);
                    Assert(view.Height >= view.MinimumSize.Height, "Minimum usable height was lost");
                    if (view.Bottom > floor.ClientSize.Height) Assert(((ScrollableControl)floor).AutoScroll, "Small window must allow reaching the bottom");
                }
            }
            string output = Path.Combine(Environment.CurrentDirectory, "artifacts", "ui");
            Directory.CreateDirectory(output);
            using var bitmap = new Bitmap(app.Width, app.Height);
            app.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(Path.Combine(output, "main-form.png"), ImageFormat.Png);
            Console.WriteLine("PASS: standard docking, cell swap, readable fonts, module bounds, full form and two machines.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void CheckModules(CoffeeMachineControl machine)
    {
        Control[] modules = { Find(machine, "selectionModule"), Find(machine, "paymentModule"), Find(machine, "dispensingModule") };
        var bounds = modules.Select(m => new Rectangle(machine.PointToClient(m.PointToScreen(Point.Empty)), m.Size)).ToArray();
        foreach (var rectangle in bounds)
            Assert(machine.ClientRectangle.Contains(rectangle), "Module outside cabinet");
        for (int i = 0; i < bounds.Length; i++)
            for (int j = i + 1; j < bounds.Length; j++)
                Assert(!bounds[i].IntersectsWith(bounds[j]), "Modules overlap");
    }
    private static void CheckContents(Control parent)
    {
        var area = parent.ClientRectangle;
        area.Inflate(2, 2); // Integer rounding of percentage table cells.
        foreach (Control child in parent.Controls)
        {
            if (!child.Visible) continue;
            Assert(area.Contains(child.Bounds), child.Name + " clipped inside " + parent.Name + ": " + child.Bounds + " / " + area);
            CheckContents(child);
        }
    }
    private static Control Find(Control root, string name) => root.Controls.Find(name, true).Single();
    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private sealed class MemoryStore : IMachineStore
    {
        public IReadOnlyList<MachineConfiguration> Load() => new[] { MachineConfiguration.CreateDefault(), MachineConfiguration.CreateDefault() };
        public void Save(IReadOnlyList<MachineConfiguration> machines) => throw new InvalidOperationException("UI tests must not write settings");
    }
}
