using drz.Abstractions.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using System;

namespace Test.Services
{
    public class CommandLineMessageService : IMessageService
    {
        public CommandLineMessageService()
        {
            Document document = Application.DocumentManager.MdiActiveDocument;
            if (document == null)
            {
                throw new ArgumentNullException("Нет активного документа");
            }

            Editor editor = document.Editor;
            if (editor == null)
            {
                throw new ArgumentNullException("Невозможно использовать редактор");
            }

            _editor = editor;
        }

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
            _editor.WriteMessage("\n" + (string.IsNullOrWhiteSpace(prefix) ? "" : $"[{prefix}]\t") +
                          (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message);
        }

        private Editor _editor;
    }
}
