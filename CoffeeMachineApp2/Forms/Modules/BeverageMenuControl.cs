using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using CoffeeMachineApp.Models;

namespace CoffeeMachineApp.Forms.Modules
{
    /// <summary>Меню одного автомата. Показывает напитки и сообщает выбранный ID владельцу.</summary>
    public partial class BeverageMenuControl : UserControl
    {
        private const int PageSize = 6;
        private readonly RoundedButton[] _cards;
        private IReadOnlyList<Beverage> _beverages = Array.Empty<Beverage>();
        private Beverage[] _visibleDrinks = Array.Empty<Beverage>();
        private CoffeeOrder _order;
        private MachineState _state = MachineState.Choosing;
        private bool _administrator, _preview = true;
        private int _filter, _page, _pageCount = 1;

        public event EventHandler<string> BeverageSelected;

        public BeverageMenuControl()
        {
            InitializeComponent();
            _cards = new[] { beverageButton1, beverageButton2, beverageButton3, beverageButton4, beverageButton5, beverageButton6 };
            // Без пользовательских файлов и модели автомата: предварительный вид для конструктора VS.
            _beverages = MachineConfiguration.CreateDefault().Menu;
            BuildPage();
        }

        public void SetMenu(IReadOnlyList<Beverage> menu)
        {
            if (menu == null) throw new ArgumentNullException(nameof(menu));
            _beverages = menu.ToArray();
            _preview = false;
            BuildPage();
        }

        public void ShowState(CoffeeOrder order, MachineState state, bool administrator)
        {
            _order = order;
            _state = state;
            _administrator = administrator;
            RefreshState();
        }

        private void BuildPage()
        {
            menuGrid.SuspendLayout();
            try
            {
                var drinks = _beverages.Where(x => _filter == 0 || (_filter == 1 ? !x.HasMilk : x.HasMilk)).ToArray();
                _pageCount = Math.Max(1, (drinks.Length + PageSize - 1) / PageSize);
                _page = Math.Clamp(_page, 0, _pageCount - 1);
                _visibleDrinks = drinks.Skip(_page * PageSize).Take(PageSize).ToArray();
                for (int i = 0; i < _cards.Length; i++)
                {
                    var button = _cards[i];
                    var oldImage = button.Image;
                    button.Image = null;
                    oldImage?.Dispose();
                    button.Visible = i < _visibleDrinks.Length;
                    if (i >= _visibleDrinks.Length)
                    {
                        button.Text = button.Detail = button.Price = "";
                        beverageTips.SetToolTip(button, null);
                        continue;
                    }
                    var drink = _visibleDrinks[i];
                    Image source = _preview ? ImageLoader.LoadEmbedded(drink) : ImageLoader.Load(drink);
                    button.Image = new Bitmap(source, new Size(132, 88));
                    button.Text = drink.Name;
                    button.Detail = drink.VolumeMl + " мл";
                    button.Price = $"{drink.Price:0.##} ₽";
                    button.AccessibleName = $"{drink.Name}, {drink.VolumeMl} мл, {drink.Price:0.##} рублей";
                    beverageTips.SetToolTip(button, drink.Name + "\n" + drink.Recipe);
                }
                menuGrid.Visible = drinks.Length > 0;
                emptyLabel.Visible = drinks.Length == 0;
                pageLabel.Text = _pageCount > 1 ? $"{_page + 1} / {_pageCount}" : $"Напитков: {drinks.Length}";
                pageLabel.AccessibleName = $"Страница {_page + 1} из {_pageCount}. Всего напитков: {drinks.Length}";
                previousPageButton.Visible = nextPageButton.Visible = _pageCount > 1;
                RefreshState();
            }
            finally { menuGrid.ResumeLayout(true); }
        }

        private void RefreshState()
        {
            bool choosing = !_administrator && _state == MachineState.Choosing;
            stepLabel.Text = _administrator ? "ОБСЛУЖИВАНИЕ · РЕЖИМ АДМИНИСТРАТОРА" : _state switch
            {
                MachineState.Choosing => "01  ВЫБОР     ·     02  ОПЛАТА     ·     03  ГОТОВО",
                MachineState.Paying => "01  ВЫБРАНО     →     02  ОПЛАТА     ·     03  ГОТОВО",
                MachineState.Brewing => "01  ВЫБРАНО     ·     02  ОПЛАЧЕНО     →     ГОТОВИМ",
                _ => "ВАШ КОФЕ ГОТОВ. ХОРОШЕГО ДНЯ!"
            };
            allFilterButton.Selected = _filter == 0;
            blackFilterButton.Selected = _filter == 1;
            milkFilterButton.Selected = _filter == 2;
            allFilterButton.Enabled = blackFilterButton.Enabled = milkFilterButton.Enabled = choosing;
            previousPageButton.Enabled = choosing && _page > 0;
            nextPageButton.Enabled = choosing && _page < _pageCount - 1;
            for (int i = 0; i < _cards.Length; i++)
            {
                _cards[i].Enabled = choosing && i < _visibleDrinks.Length && _visibleDrinks[i].IsEnabled;
                _cards[i].Selected = i < _visibleDrinks.Length && _order?.Beverage.Id == _visibleDrinks[i].Id;
            }
        }

        private void SelectFilter(int filter)
        {
            if (_administrator || _state != MachineState.Choosing) return;
            _filter = filter;
            _page = 0;
            BuildPage();
        }

        private void ChangePage(int direction)
        {
            if (_administrator || _state != MachineState.Choosing) return;
            int page = Math.Clamp(_page + direction, 0, _pageCount - 1);
            if (_page == page) return;
            _page = page;
            BuildPage();
        }

        private void BeverageButton_Click(object sender, EventArgs e)
        {
            if (_administrator || _state != MachineState.Choosing) return;
            int index = Array.IndexOf(_cards, sender as RoundedButton);
            if (index >= 0 && index < _visibleDrinks.Length && _visibleDrinks[index].IsEnabled)
                BeverageSelected?.Invoke(this, _visibleDrinks[index].Id);
        }

        private void AllFilterButton_Click(object sender, EventArgs e) => SelectFilter(0);
        private void BlackFilterButton_Click(object sender, EventArgs e) => SelectFilter(1);
        private void MilkFilterButton_Click(object sender, EventArgs e) => SelectFilter(2);
        private void PreviousPageButton_Click(object sender, EventArgs e) => ChangePage(-1);
        private void NextPageButton_Click(object sender, EventArgs e) => ChangePage(1);

        private void DisposeCardImages()
        {
            if (_cards == null) return;
            foreach (var button in _cards)
            {
                var image = button.Image;
                button.Image = null;
                image?.Dispose();
            }
        }
    }
}
