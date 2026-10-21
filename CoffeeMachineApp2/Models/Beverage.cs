using System;
using System.Collections.Generic;

namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Напиток: товар с объёмом, цветом и временем приготовления.
    /// Каждый вид напитка по-своему описывает приготовление (абстрактные методы).
    /// </summary>
    public abstract class Beverage : Product
    {
        protected Beverage(string name, decimal price, int volumeMl, string imageFile, string liquidColor, bool hasMilk)
            : base(name, price)
        {
            if (volumeMl <= 0)
                throw new ArgumentException("Объём должен быть больше нуля");

            VolumeMl = volumeMl;
            ImageFile = imageFile;
            LiquidColor = liquidColor;
            HasMilk = hasMilk;
        }

        public int VolumeMl { get; }
        public string ImageFile { get; }

        /// <summary>Цвет напитка в формате #RRGGBB (нужен для анимации приготовления).</summary>
        public string LiquidColor { get; }

        public bool HasMilk { get; }

        /// <summary>Администратор может временно выключить напиток.</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>Сколько секунд готовится одна порция. Наследники могут изменить.</summary>
        public virtual int PrepareSeconds => 2;

        /// <summary>Одна строка о приготовлении.</summary>
        public abstract string Prepare();

        /// <summary>Этапы приготовления (показываются по очереди во время анимации).</summary>
        public abstract IReadOnlyList<string> GetStages();
    }
}
