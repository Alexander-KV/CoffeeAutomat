using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace CoffeeMachineApp.Storage
{
    /// <summary>Локальный пароль администратора и сеанс доступа. Сам пароль на диск не записывается.</summary>
    public sealed class AdminAccess
    {
        private const int Iterations = 210000;
        private readonly string _path;
        public bool IsAdmin { get; private set; }
        public bool HasPassword => File.Exists(_path);
        public AdminAccess(string path) { _path = Path.GetFullPath(path); }

        public void CreatePassword(string password)
        {
            if (HasPassword) throw new InvalidOperationException("Пароль администратора уже создан. Войдите с существующим паролем.");
            if (string.IsNullOrWhiteSpace(password) || password.Length < 6 || password.Length > 128)
                throw new ArgumentException("Пароль должен содержать от 6 до 128 символов.");
            byte[] salt = RandomNumberGenerator.GetBytes(16);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
            Directory.CreateDirectory(Path.GetDirectoryName(_path));
            string temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    JsonSerializer.Serialize(stream, new
                    {
                        Version = 1, Iterations, Salt = Convert.ToBase64String(salt), Hash = Convert.ToBase64String(hash)
                    }, new JsonSerializerOptions { WriteIndented = true });
                    stream.Flush(true);
                }
                // Не заменяем существующий файл даже при одновременной попытке первичной настройки.
                File.Move(temporary, _path, false);
                IsAdmin = true;
            }
            finally
            {
                CryptographicOperations.ZeroMemory(hash);
                if (File.Exists(temporary)) File.Delete(temporary);
            }
        }

        public bool Login(string password)
        {
            IsAdmin = false;
            if (!HasPassword) throw new InvalidOperationException("Сначала создайте пароль администратора.");
            if (password == null || password.Length > 128) return false;
            byte[] salt, expected;
            int iterations;
            try
            {
                if (new FileInfo(_path).Length > 4096) throw new InvalidDataException("Файл доступа имеет недопустимый размер.");
                using var document = JsonDocument.Parse(File.ReadAllText(_path));
                var root = document.RootElement;
                if (root.GetProperty("Version").GetInt32() != 1) throw new InvalidDataException("Неизвестная версия файла доступа.");
                iterations = root.GetProperty("Iterations").GetInt32();
                salt = Convert.FromBase64String(root.GetProperty("Salt").GetString());
                expected = Convert.FromBase64String(root.GetProperty("Hash").GetString());
                if (iterations < 100000 || iterations > 1000000 || salt.Length != 16 || expected.Length != 32)
                    throw new InvalidDataException("Некорректные параметры файла доступа.");
            }
            catch (Exception ex) when (ex is JsonException || ex is FormatException || ex is ArgumentException || ex is System.Collections.Generic.KeyNotFoundException || ex is InvalidOperationException)
            {
                throw new InvalidDataException("Файл доступа администратора повреждён. Он не был изменён; восстановите его из резервной копии.", ex);
            }
            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            try { IsAdmin = CryptographicOperations.FixedTimeEquals(actual, expected); return IsAdmin; }
            finally { CryptographicOperations.ZeroMemory(actual); }
        }
        public void Logout() { IsAdmin = false; }
        public void RequireAdministrator()
        {
            if (!IsAdmin) throw new UnauthorizedAccessException("Для изменения автоматов войдите в режим администратора.");
        }
    }
}
