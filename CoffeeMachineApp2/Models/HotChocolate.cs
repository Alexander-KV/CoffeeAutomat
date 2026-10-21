using System.Collections.Generic;

namespace CoffeeMachineApp.Models
{
    /// <summary>Горячий шоколад: не кофе, поэтому отдельная ветка наследования.</summary>
    public class HotChocolate : Beverage
    {
        public HotChocolate(decimal price, int volumeMl, string imageFile)
            : base("Горячий шоколад", price, volumeMl, imageFile, "#4B2A1C", true)
        {
        }

        public override int PrepareSeconds => 4;

        public override string Prepare() => "Готовим " + Name + ": шоколад + горячее молоко";

        public override IReadOnlyList<string> GetStages()
        {
            return new List<string>
            {
                "Растапливаем шоколад…",
                "Подогреваем молоко…",
                "Взбиваем и смешиваем…",
                "Почти готово…"
            };
        }
    }
}
