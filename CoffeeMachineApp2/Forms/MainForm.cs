using System.Windows.Forms;

namespace CoffeeMachineApp.Forms
{
    /// <summary>
    /// Окно-оболочка. Сама ничего не делает: просто показывает элемент CoffeeMachineControl.
    /// Весь интерфейс автомата - в этом элементе, поэтому его можно использовать в любом окне.
    /// </summary>
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
    }
}
