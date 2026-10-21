# CoffeeMachineApp - симулятор кофейного автомата (C#, Windows Forms)

## Запуск
1. Visual Studio 2022, нагрузка «Разработка классических приложений на .NET».
2. Откройте `CoffeeMachineApp.csproj`, нажмите **Сборка → Собрать решение**, затем **F5**.
3. Если стоит другая версия .NET, поменяйте `net8.0-windows` в `.csproj`.

## Как двигать модули в Visual Studio
1. Соберите проект (Ctrl+Shift+B).
2. Двойной щелчок по `Forms/CoffeeMachineControl.cs` -> откроется дизайнер.
3. Модули (Menu / Order / Payment / Preparation) - обычные элементы:
   тащите мышью, тяните за рамку (размер), правьте `Location` и `Size` в окне Свойства.
   Внутри модули растягиваются сами, поэтому любой можно поставить на любое место.
4. Новый модуль: класс от `MachineModule` -> перетащить из Панели элементов -> добавить в массив
   `_modules` в `CoffeeMachineControl.cs`.

## Структура
Models/  - вся логика (в заказе один напиток) (Product -> Beverage -> Coffee / HotChocolate, CoffeeMachine, CoffeeOrder,
           OrderItem, Sugar, Additive, IPayable, IPaymentMethod -> CashPayment / CardPayment, BankCard)
Forms/   - MainForm (оболочка) -> CoffeeMachineControl (весь интерфейс как элемент)
           -> модули от MachineModule (MenuModule, OrderModule, PaymentModule, PreparationModule)
Assets/  - картинки напитков
Tools/   - скрипт, которым нарисованы картинки (Python + Pillow, не нужен для запуска)
