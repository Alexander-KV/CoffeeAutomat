using System;
using System.IO;
using System.Windows.Forms;
using CoffeeMachineApp.Forms;
using CoffeeMachineApp.Storage;

namespace CoffeeMachineApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string path = JsonMachineStore.DefaultPath;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                // Два окна программы не должны перезаписывать один файл настроек одновременно.
                using var appLock = new FileStream(path + ".lock", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                Application.Run(new MainForm(new JsonMachineStore(path)));
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is System.Text.Json.JsonException || ex is ArgumentException)
            {
                MessageBox.Show($"Не удалось открыть настройки:\n{path}\n\n{ex.Message}\n\nЗакройте другой экземпляр программы, если он запущен. Если JSON повреждён, исправьте его или переместите в резервную копию; исходный файл не перезаписан.",
                    "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
