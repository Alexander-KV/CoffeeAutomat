using CoffeeMachineApp.Models;
using CoffeeMachineApp.Storage;
using System.Text.Json;

// Исполняемые проверки без NuGet-зависимостей; при любой ошибке код возврата равен 1.
internal static class Program
{
    private static int _passed;
    private static int _failed;
    private static readonly string Temp = Path.Combine(Path.GetTempPath(), "coffee-tests-" + Guid.NewGuid().ToString("N"));
    private static void Main()
    {
        Directory.CreateDirectory(Temp);
        try
        {
            Run("Наличные: настройка, оплата, сдача, выдача, новый заказ", () =>
            {
                var m = NewMachine(); m.Select(m.Configuration.Menu[0].Id);
                m.Configure(3, true, "Ваниль"); Equal(190m, m.Order.Total);
                m.BeginPayment(); m.InsertMoney(500); m.Pay();
                Equal(310m, m.Change); Equal(0m, m.Cash.Inserted); Equal(MachineState.Brewing, m.State);
                Throws<InvalidOperationException>(() => m.Cancel());
                Throws<InvalidOperationException>(() => m.Collect());
                m.FinishPreparing(); Equal(310m, m.Collect());
                Equal(MachineState.Choosing, m.State); Equal(null, m.Order); Equal(0m, m.Change);
            });
            Run("Недоплата не теряет внесённые деньги и допускает повторную оплату", () =>
            {
                var m = Paying(); m.InsertMoney(100); Throws<InvalidOperationException>(m.Pay);
                Equal(100m, m.Cash.Inserted); Equal(MachineState.Paying, m.State);
                m.InsertMoney(50); m.Pay(); Equal(30m, m.Change);
                Throws<InvalidOperationException>(m.Pay);
            });
            Run("Отмена возвращает все наличные", () =>
            {
                var m = Paying(); m.InsertMoney(500); Equal(500m, m.Cancel());
                Equal(0m, m.Cash.Inserted); Equal(MachineState.Choosing, m.State); Equal(null, m.Order);
            });
            Run("Возврат из оплаты сохраняет рецепт и возвращает деньги без повторной выдачи", () =>
            {
                var m = NewMachine(); m.Select(m.Configuration.Menu[2].Id); m.Configure(3, true, "Карамель");
                var order = m.Order; m.BeginPayment(); m.InsertMoney(500);
                Equal(500m, m.BackToSelection()); Equal(order, m.Order); Equal(MachineState.Choosing, m.State);
                Equal(0m, m.Cash.Inserted); Throws<InvalidOperationException>(() => m.BackToSelection());
                m.Configure(1, false, "Без сиропа"); Equal(180m, m.Order.Total);
                m.BeginPayment(); m.SelectPayment(true); Equal(0m, m.BackToSelection());
                Equal(false, m.UseCard); Equal(2000m, m.Card.Balance);
            });
            Run("Переключение на карту возвращает наличные ровно один раз", () =>
            {
                var m = Paying(); m.InsertMoney(100);
                Equal(100m, m.SelectPayment(true)); Equal(0m, m.SelectPayment(true));
                Throws<InvalidOperationException>(() => m.InsertMoney(100));
                m.Pay(); Equal(1880m, m.Card.Balance); Equal(0m, m.Change);
                m.FinishPreparing(); Equal(0m, m.Collect());
                Equal(false, m.UseCard);
            });
            Run("Недостаточный баланс карты не списывает деньги", () =>
            {
                var config = MachineConfiguration.CreateDefault();
                config = config with { Menu = new[] { config.Menu[0] with { Price = 2500 } } };
                var m = new CoffeeMachine(config); m.Select(config.Menu[0].Id); m.BeginPayment(); m.SelectPayment(true);
                Throws<InvalidOperationException>(m.Pay); Equal(2000m, m.Card.Balance); Equal(MachineState.Paying, m.State);
                m.SelectPayment(false); for (int i = 0; i < 5; i++) m.InsertMoney(500);
                m.Pay(); Equal(MachineState.Brewing, m.State);
            });
            Run("Переходы состояний защищены от действий не по порядку", () =>
            {
                var m = NewMachine();
                Throws<InvalidOperationException>(m.BeginPayment); Throws<InvalidOperationException>(m.Pay);
                Throws<InvalidOperationException>(m.FinishPreparing); Throws<InvalidOperationException>(() => m.Collect());
                Throws<InvalidOperationException>(() => m.InsertMoney(100));
                m.Select(m.Configuration.Menu[0].Id); m.BeginPayment();
                Throws<InvalidOperationException>(() => m.Select(m.Configuration.Menu[1].Id));
                Throws<InvalidOperationException>(() => m.Configure(2, false, "Ваниль"));
                Throws<InvalidOperationException>(() => m.UpdateConfiguration(m.Configuration));
                m.InsertMoney(500); m.Pay();
                Throws<InvalidOperationException>(() => m.SelectPayment(true));
                Throws<InvalidOperationException>(() => m.InsertMoney(50));
                m.FinishPreparing(); Throws<InvalidOperationException>(() => m.Cancel());
                m.Collect(); Throws<InvalidOperationException>(() => m.Collect());
            });
            Run("Недоступный напиток и некорректные настройки не меняют заказ", () =>
            {
                var config = MachineConfiguration.CreateDefault();
                var disabled = config.Menu[1] with { IsEnabled = false };
                var m = new CoffeeMachine(config with { Menu = new[] { config.Menu[0], disabled } });
                m.Select(config.Menu[0].Id); var before = m.Order;
                Throws<InvalidOperationException>(() => m.Select(disabled.Id)); Equal(before, m.Order);
                Throws<ArgumentException>(() => m.Configure(5, false, "Без сиропа")); Equal(before, m.Order);
                Throws<ArgumentException>(() => m.Configure(1, false, "Несуществующий")); Equal(before, m.Order);
                m.Configure(2, false, "Без сиропа"); Equal(120m, m.Order.Total);
                m.Configure(4, false, "Карамель"); Equal(160m, m.Order.Total);
            });
            Run("Дробные цены и сдача не округляются до целого рубля", () =>
            {
                var drink = new Beverage { Price = 120.25m };
                var m = new CoffeeMachine(new MachineConfiguration { Menu = new[] { drink } });
                m.Select(drink.Id); m.BeginPayment(); m.InsertMoney(500); m.Pay(); Equal(379.75m, m.Change);
            });
            Run("Недопустимые купюры и отрицательная оплата отклоняются", () =>
            {
                var cash = new CashPayment();
                Throws<ArgumentException>(() => cash.Insert(-100)); Throws<ArgumentException>(() => cash.Insert(20));
                Throws<ArgumentOutOfRangeException>(() => cash.Pay(-1)); Equal(0m, cash.Inserted);
                var card = new CardPayment(); Throws<ArgumentOutOfRangeException>(() => card.Pay(0)); Equal(2000m, card.Balance);
            });
            Run("Два автомата: меню копируется, заказы, баланс и настройки независимы", () =>
            {
                var a = NewMachine(); var b = new CoffeeMachine(a.Configuration.Duplicate("Второй"));
                Check(a.Configuration.Id != b.Configuration.Id, "Повторный ID автомата");
                Check(a.Configuration.Menu[0].Id != b.Configuration.Menu[0].Id, "Повторный ID напитка");
                a.Select(a.Configuration.Menu[0].Id); a.BeginPayment(); a.InsertMoney(500); a.Pay();
                Equal(MachineState.Choosing, b.State); Equal(null, b.Order); Equal(0m, b.Cash.Inserted);
                var edited = b.Configuration with { Menu = b.Configuration.Menu.Select(x => x with { Price = 999 }).ToArray() };
                b.UpdateConfiguration(edited); Equal(120m, a.Configuration.Menu[0].Price); Equal(999m, b.Configuration.Menu[0].Price);
                b.Select(b.Configuration.Menu[0].Id); b.BeginPayment(); b.SelectPayment(true); b.Pay();
                Equal(2000m, a.Card.Balance); Equal(1001m, b.Card.Balance);
            });
            Run("Модель снимает копию переданной коллекции меню", () =>
            {
                var menu = new List<Beverage> { new Beverage() };
                var m = new CoffeeMachine(new MachineConfiguration { Menu = menu });
                menu.Clear(); Equal(1, m.Configuration.Menu.Count);
            });
            Run("Админка не может менять меню во время выбора или подменить ID", () =>
            {
                var m = NewMachine(); var before = m.Configuration;
                Throws<ArgumentException>(() => m.UpdateConfiguration(before with { Id = Guid.NewGuid().ToString("N") }));
                m.Select(before.Menu[0].Id);
                Throws<InvalidOperationException>(() => m.UpdateConfiguration(before with { Name = "Другое" }));
                Equal(before, m.Configuration);
            });
            Run("JSON: первый запуск создаёт меню, изменения и новые напитки переживают перезапуск", () =>
            {
                string path = Path.Combine(Temp, "roundtrip", "machines.json"); var store = new JsonMachineStore(path);
                var original = store.Load()[0]; Check(File.Exists(path), "Нет JSON первого запуска");
                var added = new Beverage { Name = "Кокосовый раф", Price = 245.50m, Recipe = "Эспрессо, кокосовое молоко и сливки", HasMilk = true, ImageFile = "" };
                var menu = original.Menu.Select((x,i) => i == 0 ? x with { Price = 135, IsEnabled = false } : x).Append(added).ToArray();
                var updated = original with { Name = "Кофейня на углу", Menu = menu };
                var clone = updated.Duplicate("Вторая кофейня"); store.Save(new[] { updated, clone });
                var loaded = new JsonMachineStore(path).Load(); Equal(2, loaded.Count); Equal(updated.Id, loaded[0].Id);
                Equal("Кофейня на углу", loaded[0].Name); Equal(135m, loaded[0].Menu[0].Price); Equal(false, loaded[0].Menu[0].IsEnabled);
                Equal(10, loaded[0].Menu.Count); Equal(added, loaded[0].Menu.Last()); Equal(clone.Id, loaded[1].Id);
                Equal(0, Directory.GetFiles(Path.GetDirectoryName(path), "*.tmp").Length);
                store.Save(loaded); Equal(2, store.Load().Count);
            });
            Run("Отменённая правка не изменяет текущие настройки", () =>
            {
                var m = NewMachine(); var before = m.Configuration;
                var draft = before with { Menu = before.Menu.Select(x => x with { Price = 1 }).ToArray() };
                Equal(120m, m.Configuration.Menu[0].Price); Equal(1m, draft.Menu[0].Price);
            });
            Run("Повреждённый и пустой JSON не перезаписывается", () =>
            {
                foreach (string contents in new[] { "{broken", "null", "[]" })
                {
                    string path = Path.Combine(Temp, Guid.NewGuid() + ".json"); File.WriteAllText(path, contents);
                    Throws<Exception>(() => new JsonMachineStore(path).Load()); Equal(contents, File.ReadAllText(path));
                }
            });
            Run("Невалидные настройки и повторные ID не повреждают существующий JSON", () =>
            {
                string path = Path.Combine(Temp, "validation.json"); var store = new JsonMachineStore(path);
                var config = store.Load()[0]; string original = File.ReadAllText(path);
                Throws<ArgumentException>(() => store.Save(new[] { config, config }));
                Throws<ArgumentException>(() => store.Save(new[] { config with { Menu = new[] { config.Menu[0], config.Menu[0] } } }));
                Throws<ArgumentException>(() => store.Save(new[] { config with { Menu = Array.Empty<Beverage>() } }));
                Throws<ArgumentException>(() => store.Save(new[] { config with { Name = " " } }));
                foreach (var bad in new[] {
                    config.Menu[0] with { Price = -1 }, config.Menu[0] with { Price = 1.001m },
                    config.Menu[0] with { VolumeMl = 0 }, config.Menu[0] with { PrepareSeconds = 0 },
                    config.Menu[0] with { ImageFile = "../file.png" }, config.Menu[0] with { LiquidColor = "red" },
                    config.Menu[0] with { Name = "" }, config.Menu[0] with { Recipe = null }, config.Menu[0] with { Id = "wrong" }
                }) Throws<ArgumentException>(() => store.Save(new[] { config with { Menu = new[] { bad } } }));
                Equal(original, File.ReadAllText(path));
            });
            Run("Сбой файловой записи не оставляет временных файлов", () =>
            {
                string path = Path.Combine(Temp, "directory-instead-of-file"); Directory.CreateDirectory(path);
                Throws<IOException>(() => new JsonMachineStore(path).Save(new[] { MachineConfiguration.CreateDefault() }));
                Equal(0, Directory.GetFiles(Temp, "*.tmp").Length);
            });
            Run("Все напитки могут быть выключены: состояние сохраняется", () =>
            {
                var m = NewMachine(); m.UpdateConfiguration(m.Configuration with { Menu = m.Configuration.Menu.Select(x => x with { IsEnabled = false }).ToArray() });
                Throws<InvalidOperationException>(() => m.Select(m.Configuration.Menu[0].Id));
                Equal(null, m.Order);
            });
            Run("Удаление автомата сохраняет меню соседа и не допускает пустой список", () =>
            {
                var store = new JsonMachineStore(Path.Combine(Temp, "remove.json"));
                var original = store.Load()[0]; var clone = original.Duplicate("Второй автомат");
                clone = clone with { Menu = clone.Menu.Select(x => x with { Price = 499 }).ToArray() };
                store.Save(new[] { original, clone }); store.Save(new[] { clone });
                var restored = store.Load(); Equal(1, restored.Count); Equal(clone.Id, restored[0].Id);
                Equal(499m, restored[0].Menu[0].Price);
                Throws<ArgumentException>(() => store.Save(Array.Empty<MachineConfiguration>()));
                Equal(clone.Id, store.Load()[0].Id);
            });
        }
        finally { Directory.Delete(Temp, true); }
        Console.WriteLine($"\nPassed: {_passed}, failed: {_failed}");
        Environment.ExitCode = _failed == 0 ? 0 : 1;
    }
    private static CoffeeMachine NewMachine() => new CoffeeMachine(MachineConfiguration.CreateDefault());
    private static CoffeeMachine Paying() { var m = NewMachine(); m.Select(m.Configuration.Menu[0].Id); m.BeginPayment(); return m; }
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}, got {actual}"); }
    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new Exception($"Expected {typeof(T).Name}");
    }
    private static void Run(string name, Action test)
    {
        try { test(); _passed++; Console.WriteLine("PASS " + name); }
        catch (Exception ex) { _failed++; Console.WriteLine("FAIL " + name + ": " + ex); }
    }
}
