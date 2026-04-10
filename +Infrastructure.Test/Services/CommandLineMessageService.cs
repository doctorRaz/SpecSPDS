using Abstractions.Services;
using System;

namespace Test.Services
{
    public class CommandLineMessageService : IMessageService
    {
        //public CommandLineMessageService()
        //{

        //}

        public void ConsoleMessage(string message, string caller = null)
        {
            WriteMessage("", message, caller);
        }

        public void InfoMessage(string message, string caller = null)
        {
            WriteMessage("Info", message, caller);
        }

        public void ErrorMessage(string message, string caller = null)
        {
            WriteMessage("Error", message, caller);
        }

        public void ExceptionMessage(Exception ex, string caller = null)
        {
            WriteMessage("Exception", $"{ex.Message}\n{ex.StackTrace}", caller);
        }

        /// <summary>
        /// Сообщение об исключении
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="caller">Вызывающий метод</param>
        public void ExceptionMessage(string message, Exception ex, string caller = null)
        {
      
             WriteMessage("Exception", $"{message}\n{ex.Message}\n{ex.StackTrace}", caller);
          
           
     
        }


        private void WriteMessage(string prefix, string message, string caller)
        {

            Console.WriteLine("\n" + (string.IsNullOrWhiteSpace(prefix) ? "" : $"[{prefix}]\t") +
                          (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message);
        }


    }
}
