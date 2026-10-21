using System.Collections.Generic;

namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Кофе. Конкретный вид (эспрессо, капучино, латте...) задаётся данными,
    /// а не отдельным классом: так нет дублирования кода.
    /// </summary>
    public class Coffee : Beverage
    {
        public Coffee(string name, decimal price, int volumeMl, string imageFile,
                      string recipe, string liquidColor, bool hasMilk)
            : base(name, price, volumeMl, imageFile, liquidColor, hasMilk)
        {
            Recipe = recipe;
        }

        public string Recipe { get; }

        // с молоком готовится дольше
        public override int PrepareSeconds => HasMilk ? 3 : 2;

        public override string Prepare() => "Готовим " + Name + ": " + Recipe;

        public override IReadOnlyList<string> GetStages()
        {
            return new List<string>
            {
                "Мелем зёрна…",
                "Завариваем эспрессо…",
                HasMilk ? "Взбиваем молоко…" : "Разливаем напиток…",
                "Почти готово…"
            };
        }
    }
}
