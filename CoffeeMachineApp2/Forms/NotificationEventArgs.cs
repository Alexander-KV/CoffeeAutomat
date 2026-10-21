using System;

namespace CoffeeMachineApp.Forms
{
    /// <summary>Сообщение, которое модуль просит показать пользователю.</summary>
    public class NotificationEventArgs : EventArgs
    {
        public NotificationEventArgs(string message, bool isError)
        {
            Message = message;
            IsError = isError;
        }

        public string Message { get; }
        public bool IsError { get; }
    }
}
