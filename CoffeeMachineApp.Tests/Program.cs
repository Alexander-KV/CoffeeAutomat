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
            Run("Наличные: чашка и сдача выдаются отдельно, заказ завершается после обеих выдач", () =>
            {
                var m = NewMachine(); m.Select(m.Configuration.Menu[0].Id);
                m.Configure(3, true, "Ваниль"); Equal(190m, m.Order.Total);
                m.BeginPayment(); m.InsertMoney(500); m.Pay();
                Equal(310m, m.Change); Equal(0m, m.Cash.Inserted); Equal(MachineState.Brewing, m.State);
                Throws<InvalidOperationException>(() => m.Cancel());
                Throws<InvalidOperationException>(m.CollectDrink);
                m.FinishPreparing(); m.CollectDrink();
                Equal(MachineState.Ready, m.State); Equal(true, m.DrinkCollected); Equal(310m, m.Change);
                Throws<InvalidOperationException>(m.CollectDrink);
                Throws<InvalidOperationException>(() => m.Select(m.Configuration.Menu[0].Id));
                Equal(310m, m.CollectChange());
                Equal(MachineState.Choosing, m.State); Equal(null, m.Order); Equal(0m, m.Change);
                Throws<InvalidOperationException>(() => m.CollectChange());
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
                Equal(false, m.UseCard); Equal(false, m.Card.IsInserted);
            });
            Run("Переключение на карту возвращает наличные ровно один раз", () =>
            {
                var m = Paying(); m.InsertMoney(100);
                Equal(100m, m.SelectPayment(true)); Equal(0m, m.SelectPayment(true));
                Throws<InvalidOperationException>(() => m.InsertMoney(100));
                m.InsertCard(); m.Pay(); Equal(true, m.Card.IsInserted); Equal(0m, m.Change);
                m.EjectCard(); m.FinishPreparing(); m.CollectDrink();
                Equal(false, m.UseCard);
            });
            Run("Карта без ограничения средств оплачивает повторные заказы дороже 2000 ₽", () =>
            {
                var config = MachineConfiguration.CreateDefault();
                config = config with { Menu = new[] { config.Menu[0] with { Price = 9999 } } };
                var m = new CoffeeMachine(config);
                for (int i = 0; i < 4; i++)
                {
                    m.Select(config.Menu[0].Id); m.BeginPayment(); m.SelectPayment(true); m.InsertCard(); m.Pay();
                    Equal(MachineState.Brewing, m.State); Equal(0m, m.Change);
                    m.EjectCard(); m.FinishPreparing(); m.CollectDrink();
                    Equal(MachineState.Choosing, m.State); Equal(null, m.Order);
                }
            });
            Run("Для оплаты картой необходима вставленная карта, повторная вставка и извлечение запрещены", () =>
            {
                var m = Paying();
                Throws<InvalidOperationException>(m.InsertCard);
                m.SelectPayment(true); Throws<InvalidOperationException>(m.Pay);
                Equal(MachineState.Paying, m.State); Equal(false, m.Card.IsInserted);
                m.InsertCard(); Equal(true, m.Card.IsInserted); Throws<InvalidOperationException>(m.InsertCard);
                m.EjectCard(); Equal(false, m.Card.IsInserted); Throws<InvalidOperationException>(m.EjectCard);
                Throws<InvalidOperationException>(m.Pay);
            });
            Run("Неоплаченный заказ нельзя отменить или изменить способ оплаты, пока не забрана карта", () =>
            {
                var m = Paying(); var order = m.Order;
                m.SelectPayment(true); m.InsertCard();
                Throws<InvalidOperationException>(() => m.Cancel());
                Throws<InvalidOperationException>(() => m.BackToSelection());
                Throws<InvalidOperationException>(() => m.SelectPayment(false));
                Equal(order, m.Order); Equal(MachineState.Paying, m.State); Equal(true, m.Card.IsInserted);
                m.EjectCard(); Equal(0m, m.BackToSelection()); Equal(order, m.Order);
                m.BeginPayment(); m.SelectPayment(true); m.InsertCard(); m.EjectCard();
                Equal(0m, m.SelectPayment(false)); Equal(0m, m.Cancel()); Equal(null, m.Order);
            });
            Run("Забытая карта удерживает завершённый заказ до извлечения", () =>
            {
                var m = Paying(); m.SelectPayment(true); m.InsertCard(); m.Pay();
                Throws<InvalidOperationException>(() => m.Cancel()); Throws<InvalidOperationException>(m.Pay);
                m.FinishPreparing(); m.CollectDrink();
                Equal(MachineState.Ready, m.State); Equal(true, m.Card.IsInserted); Equal(true, m.DrinkCollected);
                Throws<InvalidOperationException>(() => m.Select(m.Configuration.Menu[0].Id));
                Throws<InvalidOperationException>(m.CollectDrink); Throws<InvalidOperationException>(() => m.CollectChange());
                m.EjectCard(); Equal(MachineState.Choosing, m.State); Equal(null, m.Order);
                Throws<InvalidOperationException>(m.EjectCard);
            });
            Run("Сдачу можно получить во время приготовления ровно один раз", () =>
            {
                var m = Paying(); m.InsertMoney(500); m.Pay();
                Equal(380m, m.CollectChange()); Equal(0m, m.Change); Equal(MachineState.Brewing, m.State);
                Throws<InvalidOperationException>(() => m.CollectChange()); Throws<InvalidOperationException>(m.CollectDrink);
                m.FinishPreparing(); m.CollectDrink(); Equal(MachineState.Choosing, m.State); Equal(null, m.Order);
            });
            Run("Готовый заказ разрешает сначала взять сдачу, затем чашку", () =>
            {
                var m = Paying(); m.InsertMoney(500); m.Pay(); m.FinishPreparing();
                Equal(380m, m.CollectChange()); Equal(MachineState.Ready, m.State); Equal(false, m.DrinkCollected);
                Throws<InvalidOperationException>(() => m.CollectChange()); m.CollectDrink();
                Equal(MachineState.Choosing, m.State); Equal(null, m.Order);
            });
            Run("При точной оплате лоток пуст и выдача чашки завершает заказ", () =>
            {
                var m = Paying(); m.InsertMoney(100); m.InsertMoney(10); m.InsertMoney(10); m.Pay();
                Equal(0m, m.Change); Throws<InvalidOperationException>(() => m.CollectChange());
                m.FinishPreparing(); m.CollectDrink(); Equal(MachineState.Choosing, m.State);
            });
            Run("Переходы состояний защищены от действий не по порядку", () =>
            {
                var m = NewMachine();
                Throws<InvalidOperationException>(m.BeginPayment); Throws<InvalidOperationException>(m.Pay);
                Throws<InvalidOperationException>(m.FinishPreparing); Throws<InvalidOperationException>(m.CollectDrink);
                Throws<InvalidOperationException>(() => m.CollectChange());
                Throws<InvalidOperationException>(() => m.InsertMoney(100));
                m.Select(m.Configuration.Menu[0].Id); m.BeginPayment();
                Throws<InvalidOperationException>(() => m.Select(m.Configuration.Menu[1].Id));
                Throws<InvalidOperationException>(() => m.Configure(2, false, "Ваниль"));
                Throws<InvalidOperationException>(() => m.UpdateConfiguration(m.Configuration));
                m.InsertMoney(500); m.Pay();
                Throws<InvalidOperationException>(() => m.SelectPayment(true));
                Throws<InvalidOperationException>(() => m.InsertMoney(50));
                m.FinishPreparing(); Throws<InvalidOperationException>(() => m.Cancel());
                m.CollectDrink(); m.CollectChange(); Throws<InvalidOperationException>(m.CollectDrink);
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
                Equal(379.75m, m.CollectChange()); Equal(0m, m.Change);
            });
            Run("Номиналы в лотке точно составляют сдачу, включая копейки", () =>
            {
                decimal[] allowed = { 500m, 100m, 50m, 10m, 5m, 2m, 1m, .5m, .1m, .05m, .01m };
                foreach (decimal amount in new[] { 0m, .01m, .75m, 1.01m, 379.75m, 10237.99m })
                {
                    var pieces = CashPayment.SplitChange(amount);
                    Equal(amount, pieces.Sum()); Check(pieces.All(x => allowed.Contains(x)), "Недопустимый номинал");
                }
                Equal(0, CashPayment.SplitChange(0).Count);
                Throws<ArgumentException>(() => CashPayment.SplitChange(-1));
                Throws<ArgumentException>(() => CashPayment.SplitChange(.001m));
            });
            Run("Недопустимые купюры и отрицательная оплата отклоняются", () =>
            {
                var cash = new CashPayment();
                Throws<ArgumentException>(() => cash.Insert(-100)); Throws<ArgumentException>(() => cash.Insert(20));
                Throws<ArgumentOutOfRangeException>(() => cash.Pay(-1)); Equal(0m, cash.Inserted);
                var card = new CardPayment(); Throws<ArgumentOutOfRangeException>(() => card.Pay(0)); Equal(false, card.IsInserted);
            });
            Run("Два автомата: меню, карты, заказы и выдача сдачи независимы", () =>
            {
                var a = NewMachine(); var b = new CoffeeMachine(a.Configuration.Duplicate("Второй"));
                Check(a.Configuration.Id != b.Configuration.Id, "Повторный ID автомата");
                Check(a.Configuration.Menu[0].Id != b.Configuration.Menu[0].Id, "Повторный ID напитка");
                a.Select(a.Configuration.Menu[0].Id); a.BeginPayment(); a.InsertMoney(500); a.Pay();
                Equal(MachineState.Choosing, b.State); Equal(null, b.Order); Equal(0m, b.Cash.Inserted);
                var edited = b.Configuration with { Menu = b.Configuration.Menu.Select(x => x with { Price = 999 }).ToArray() };
                b.UpdateConfiguration(edited); Equal(120m, a.Configuration.Menu[0].Price); Equal(999m, b.Configuration.Menu[0].Price);
                b.Select(b.Configuration.Menu[0].Id); b.BeginPayment(); b.SelectPayment(true); b.InsertCard(); b.Pay();
                Equal(false, a.Card.IsInserted); Equal(true, b.Card.IsInserted); Equal(0m, b.Change);
                Equal(380m, a.CollectChange()); Equal(MachineState.Brewing, b.State); Equal(true, b.Card.IsInserted);
                a.FinishPreparing(); a.CollectDrink(); Equal(null, a.Order); Check(b.Order != null, "Заказ соседа потерян");
                b.FinishPreparing(); b.CollectDrink(); Equal(MachineState.Ready, b.State);
                b.EjectCard(); Equal(MachineState.Choosing, b.State);
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
            AdminAccessChecks.Run(Run, Temp);
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
