using System;
using System.IO;

namespace CoffeeMachineApp.Models
{
    // Вид кофе — данные, а не отдельный подкласс. Новые рецепты не требуют перекомпиляции.
    public sealed record Beverage
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string Name { get; init; } = "Новый кофе";
        public decimal Price { get; init; } = 150;
        public int VolumeMl { get; init; } = 200;
        public string Recipe { get; init; } = "Эспрессо и горячая вода";
        public string ImageFile { get; init; } = "coffee1.png";
        public string LiquidColor { get; init; } = "#70462D";
        public bool HasMilk { get; init; }
        public bool IsCoffee { get; init; } = true;
        public bool IsEnabled { get; init; } = true;
        public int PrepareSeconds { get; init; } = 6;

        public void Validate()
        {
            if (!Guid.TryParseExact(Id, "N", out _) || string.IsNullOrWhiteSpace(Name) || Name.Length > 60)
                throw new ArgumentException("Название напитка: от 1 до 60 символов; ID должен быть уникальным GUID.");
            if (Price < 1 || Price > 9999 || decimal.Round(Price, 2) != Price)
                throw new ArgumentException("Цена: от 1 до 9999 ₽, не более двух знаков после запятой.");
            if (VolumeMl < 30 || VolumeMl > 1000 || PrepareSeconds < 2 || PrepareSeconds > 60)
                throw new ArgumentException("Объём: 30–1000 мл; приготовление: 2–60 секунд.");
            if (string.IsNullOrWhiteSpace(Recipe) || Recipe.Length > 200)
                throw new ArgumentException("Укажите рецепт длиной до 200 символов.");
            if (LiquidColor == null || !System.Text.RegularExpressions.Regex.IsMatch(LiquidColor, "^#[0-9a-fA-F]{6}$"))
                throw new ArgumentException("Цвет напитка должен иметь формат #RRGGBB.");
            if (ImageFile == null || ImageFile.Length > 100 || ImageFile.Contains('/') || ImageFile.Contains('\\') || ImageFile != Path.GetFileName(ImageFile))
                throw new ArgumentException("Укажите имя картинки из Assets без пути или оставьте поле пустым.");
        }
    }
}
