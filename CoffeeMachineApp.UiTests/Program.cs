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
            machine.Size = new Size(640, 1000);
            var baseline = machine.SelectionModule.Bounds;
            var label = Find(machine.PaymentModule, "_terminalTotal");
            float font = label.Font.Size;
            foreach (var size in new[] { new Size(480, 750), new Size(320, 500), new Size(800, 1250), new Size(960, 1500), new Size(640, 1000) })
            {
                machine.Size = size;
                Application.DoEvents();
                float factor = size.Width / 640f;
                Assert(Math.Abs(machine.SelectionModule.Width - baseline.Width * factor) <= 2, "Menu did not scale");
                CheckModules(machine);
                CheckContents(machine.SelectionModule);
                CheckContents(machine.PaymentModule);
                CheckContents(machine.DispensingModule);
                Assert(Math.Abs(label.Font.Size - font * factor) < 0.1f, "Font did not scale");
                // Refreshing price/payment must not reset the scaled font.
                machine.PaymentModule.ShowState(new CoffeeMachine(MachineConfiguration.CreateDefault()), false);
                Assert(Math.Abs(label.Font.Size - font * factor) < 0.1f, "ShowState reset font size");
            }
            Assert(machine.SelectionModule.Bounds == baseline, "Resize round trip changed layout");
            machine.PaymentModule.Location = new Point(470, 110);
            machine.Size = new Size(320, 500);
            Assert(machine.PaymentModule.Location == new Point(235, 55), "Edited position was lost");
            machine.Size = new Size(640, 1000);
            Assert(machine.PaymentModule.Location == new Point(470, 110), "Edited position drifted");
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
                    Assert(view.Bottom <= floor.ClientSize.Height, "Machine is clipped vertically");
                }
            }
            string output = Path.Combine(Environment.CurrentDirectory, "artifacts", "ui");
            Directory.CreateDirectory(output);
            using var bitmap = new Bitmap(app.Width, app.Height);
            app.DrawToBitmap(bitmap, new Rectangle(Point.Empty, bitmap.Size));
            bitmap.Save(Path.Combine(output, "main-form.png"), ImageFormat.Png);
            Console.WriteLine("PASS: module bounds, contents, fonts, resize round trip, edited positions, full form and two machines.");
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static void CheckModules(CoffeeMachineControl machine)
    {
        Control[] modules = { machine.SelectionModule, machine.PaymentModule, machine.DispensingModule };
        foreach (var module in modules)
            Assert(machine.ClientRectangle.Contains(module.Bounds), module.Name + " outside cabinet");
        for (int i = 0; i < modules.Length; i++)
            for (int j = i + 1; j < modules.Length; j++)
                Assert(!modules[i].Bounds.IntersectsWith(modules[j].Bounds), "Modules overlap");
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
