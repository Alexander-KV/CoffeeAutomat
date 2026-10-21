using System;

namespace CoffeeMachineApp.Models
{
    /// <summary>
    /// Сахар: 0-4 ложки. До 2 ложек бесплатно, 3-4 ложки - фиксированная надбавка.
    /// Класс сам следит за допустимым диапазоном.
    /// </summary>
    public class Sugar : IPayable
    {
        public const int MaxSpoons = 4;
        public const int FreeSpoons = 2;
        public const decimal ExtraPrice = 10m;

        private int _spoons;

        public Sugar(int spoons = 0)
        {
            if (spoons < 0 || spoons > MaxSpoons)
                throw new ArgumentOutOfRangeException(nameof(spoons), "Сахар: от 0 до 4 ложек");
            _spoons = spoons;
        }

        public int Spoons => _spoons;

        public void Increase()
        {
            if (_spoons < MaxSpoons) _spoons++;
        }

        public void Decrease()
        {
            if (_spoons > 0) _spoons--;
        }

        public decimal GetPrice() => _spoons > FreeSpoons ? ExtraPrice : 0m;

        public override string ToString()
        {
            string word = _spoons == 1 ? "ложка" : (_spoons >= 2 ? "ложки" : "ложек");
            return _spoons + " " + word;
        }
    }
}
