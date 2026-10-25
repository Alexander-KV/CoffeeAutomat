using System;
using System.IO;
using CoffeeMachineApp.Storage;

internal static class AdminAccessChecks
{
    public static void Run(Action<string, Action> run, string temp)
    {
        run("Администратор: настройка пароля, вход, выход и запрет операций покупателю", () =>
        {
            string path = Path.Combine(temp, "admin-access.json");
            var access = new AdminAccess(path);
            Check(!access.IsAdmin && !access.HasPassword, "Первый запуск должен быть режимом покупателя");
            Throws<UnauthorizedAccessException>(access.RequireAdministrator);
            access.CreatePassword("пароль-владельца-2026");
            Check(access.IsAdmin && access.HasPassword, "Первичная настройка должна открыть админ-режим");
            access.RequireAdministrator();
            string contents = File.ReadAllText(path);
            Check(!contents.Contains("пароль-владельца-2026"), "Пароль записан открытым текстом");
            access.Logout(); Check(!access.IsAdmin, "Выход не завершил сеанс");
            Throws<UnauthorizedAccessException>(access.RequireAdministrator);
            Check(!access.Login("ошибочный пароль") && !access.IsAdmin, "Неверный пароль открыл доступ");
            Check(access.Login("пароль-владельца-2026") && access.IsAdmin, "Правильный пароль не принят");
            var restart = new AdminAccess(path);
            Check(!restart.IsAdmin && restart.HasPassword, "Сеанс администратора пережил перезапуск");
            Check(restart.Login("пароль-владельца-2026"), "Пароль не сохранился между запусками");
            Throws<InvalidOperationException>(() => restart.CreatePassword("другой пароль"));
            Check(contents == File.ReadAllText(path), "Повторная настройка перезаписала пароль");
            Check(!restart.Login("неверный пароль"), "Повторная авторизация приняла неверный пароль");
            Throws<UnauthorizedAccessException>(restart.RequireAdministrator);
        });
        run("Администратор: некорректный пароль и ошибка записи не дают доступа", () =>
        {
            string path = Path.Combine(temp, "invalid-admin.json"); var access = new AdminAccess(path);
            foreach (string password in new[] { "", "12345", "      ", new string('a', 129) })
            {
                Throws<ArgumentException>(() => access.CreatePassword(password));
                Check(!access.IsAdmin && !access.HasPassword, "Некорректный пароль создал сеанс или файл");
            }
            Directory.CreateDirectory(path);
            Throws<IOException>(() => access.CreatePassword("valid-password"));
            Check(!access.IsAdmin, "Ошибка записи открыла админ-режим");
            Check(Directory.GetFiles(temp, "invalid-admin.json.*.tmp").Length == 0, "Остался временный файл пароля");
        });
        run("Администратор: повреждённый файл не сбрасывает пароль", () =>
        {
            foreach (string contents in new[] { "broken json", "{}", "null", "{\"Version\":1,\"Iterations\":0,\"Salt\":\"\",\"Hash\":\"\"}" })
            {
                string path = Path.Combine(temp, Guid.NewGuid().ToString("N") + ".admin.json"); File.WriteAllText(path, contents);
                var access = new AdminAccess(path);
                Check(access.HasPassword, "Повреждённый файл принят за отсутствие пароля");
                Throws<InvalidDataException>(() => access.Login("any-password"));
                Throws<InvalidOperationException>(() => access.CreatePassword("replacement-password"));
                Check(!access.IsAdmin && File.ReadAllText(path) == contents, "Повреждение открыло доступ или изменило файл");
            }
        });
        run("Администратор: одинаковые пароли получают разные соли и хеши", () =>
        {
            string one = Path.Combine(temp, "salt-one.json"), two = Path.Combine(temp, "salt-two.json");
            new AdminAccess(one).CreatePassword("same-password"); new AdminAccess(two).CreatePassword("same-password");
            Check(File.ReadAllText(one) != File.ReadAllText(two), "Для разных установок использована одна соль");
        });
    }
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
}
