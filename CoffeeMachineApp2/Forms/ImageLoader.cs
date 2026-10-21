using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Загружает картинку напитка из папки Assets.
    /// Если файла нет - рисует простую заглушку (чашка), так что программа работает и без картинок.
    /// </summary>
    internal static class ImageLoader
    {
        private static readonly Dictionary<string, Image> Cache = new Dictionary<string, Image>();

        public static Image Load(Beverage beverage)
        {
            Image image;
            if (Cache.TryGetValue(beverage.ImageFile, out image))
                return image;

            image = LoadFromDisk(beverage.ImageFile) ?? CreatePlaceholder();
            Cache[beverage.ImageFile] = image;
            return image;
        }

        private static Image LoadFromDisk(string fileName)
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", fileName);
                if (!File.Exists(path))
                    return null;

                // читаем через поток и копируем, чтобы файл не оставался заблокированным
                using (var stream = File.OpenRead(path))
                using (var loaded = Image.FromStream(stream))
                    return new Bitmap(loaded);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Image CreatePlaceholder()
        {
            var bitmap = new Bitmap(300, 225);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.Clear(Color.White);

                using (var saucer = new SolidBrush(Color.FromArgb(225, 215, 205)))
                using (var cup = new SolidBrush(UiStyle.Brown))
                using (var coffee = new SolidBrush(Color.FromArgb(60, 40, 28)))
                using (var handle = new Pen(UiStyle.Brown, 10))
                using (var steam = new Pen(Color.FromArgb(205, 205, 205), 4))
                {
                    g.FillEllipse(saucer, 70, 170, 160, 26);
                    g.DrawArc(handle, 190, 105, 40, 44, -90, 180);
                    g.FillPolygon(cup, new[] { new Point(95, 100), new Point(200, 100), new Point(188, 178), new Point(107, 178) });
                    g.FillEllipse(coffee, 95, 90, 105, 20);
                    g.DrawArc(steam, 125, 30, 14, 40, 90, 180);
                    g.DrawArc(steam, 148, 24, 14, 40, 270, 180);
                    g.DrawArc(steam, 171, 30, 14, 40, 90, 180);
                }
            }
            return bitmap;
        }
    }
}
