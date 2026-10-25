using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Storage
{
    public interface IMachineStore
    {
        IReadOnlyList<MachineConfiguration> Load();
        void Save(IReadOnlyList<MachineConfiguration> machines);
    }

    /// <summary>Атомарная запись: сначала временный файл, затем замена. Ошибки не скрываются.</summary>
    public sealed class JsonMachineStore : IMachineStore
    {
        private readonly string _path;
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        public static string DefaultPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CoffeeMachineApp", "machines.json");
        public JsonMachineStore(string path) { _path = Path.GetFullPath(path); }
        public IReadOnlyList<MachineConfiguration> Load()
        {
            if (!File.Exists(_path))
            {
                var defaults = new[] { MachineConfiguration.CreateDefault() };
                Save(defaults);
                return defaults;
            }
            // Повреждённый JSON не заменяем настройками по умолчанию: пользователь не теряет данные.
            var machines = JsonSerializer.Deserialize<List<MachineConfiguration>>(File.ReadAllText(_path), Options);
            Validate(machines);
            return machines.Select(x => x.Snapshot()).ToArray();
        }
        public void Save(IReadOnlyList<MachineConfiguration> machines)
        {
            Validate(machines);
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            string temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, machines, Options);
                    stream.Flush(true);
                }
                File.Move(temporary, _path, true);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        private static void Validate(IReadOnlyList<MachineConfiguration> machines)
        {
            if (machines == null || machines.Count == 0 || machines.Count > 12 || machines.Any(x => x == null))
                throw new ArgumentException("В файле должно быть от 1 до 12 автоматов.");
            foreach (var machine in machines) machine.Validate();
            if (machines.Select(x => x.Id).Distinct().Count() != machines.Count)
                throw new ArgumentException("ID автоматов должны быть уникальны.");
        }
    }
}
