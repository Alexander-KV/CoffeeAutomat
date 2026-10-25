using System;
using System.Collections.Generic;
using System.Linq;

namespace CoffeeMachineApp.Models
{
    public sealed record MachineConfiguration
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string Name { get; init; } = "Кофейня №1";
        public IReadOnlyList<Beverage> Menu { get; init; } = Array.Empty<Beverage>();
        public void Validate()
        {
            if (!Guid.TryParseExact(Id, "N", out _) || string.IsNullOrWhiteSpace(Name) || Name.Length > 40)
                throw new ArgumentException("Название автомата: от 1 до 40 символов; необходим корректный ID.");
            if (Menu == null || Menu.Count == 0 || Menu.Count > 100 || Menu.Any(x => x == null))
                throw new ArgumentException("В меню должно быть от 1 до 100 напитков.");
            foreach (var drink in Menu) drink.Validate();
            if (Menu.Select(x => x.Id).Distinct().Count() != Menu.Count)
                throw new ArgumentException("В меню обнаружены повторяющиеся ID напитков.");
        }
        public MachineConfiguration Snapshot() => this with { Menu = Array.AsReadOnly(Menu.ToArray()) };
        public MachineConfiguration Duplicate(string name) => this with
        {
            Id = Guid.NewGuid().ToString("N"), Name = name,
            Menu = Array.AsReadOnly(Menu.Select(x => x with { Id = Guid.NewGuid().ToString("N") }).ToArray())
        };
        public static MachineConfiguration CreateDefault() => new MachineConfiguration
        {
            Menu = new[]
            {
                Drink("Эспрессо",120,60,"Эспрессо из свежемолотых зёрен",1,false,"#3A2212"),
                Drink("Американо",140,200,"Эспрессо и горячая вода",2,false,"#4A2E1B"),
                Drink("Капучино",180,300,"Эспрессо, молоко и нежная пена",3,true,"#B5875C"),
                Drink("Латте",190,300,"Эспрессо и много тёплого молока",4,true,"#C9A27A"),
                Drink("Латте макиато",200,300,"Молоко и эспрессо слоями",5,true,"#D9B98E"),
                Drink("Моккачино",210,300,"Эспрессо, шоколад и молоко",6,true,"#7A4A2E"),
                Drink("Флэт уайт",200,250,"Двойной эспрессо и бархатное молоко",7,true,"#B98558"),
                Drink("Раф",220,300,"Эспрессо, сливки и ваниль",8,true,"#E0C39A"),
                Drink("Горячий шоколад",180,250,"Шоколад и горячее молоко",9,true,"#6B3826") with { IsCoffee = false }
            }
        };
        private static Beverage Drink(string name, decimal price, int volume, string recipe, int image, bool milk, string color)
            => new Beverage { Name = name, Price = price, VolumeMl = volume, Recipe = recipe, ImageFile = $"coffee{image}.png", HasMilk = milk, LiquidColor = color };
    }
}
