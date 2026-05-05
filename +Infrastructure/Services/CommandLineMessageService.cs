using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using System;

#if !TEST
using HostMgd.ApplicationServices;
#endif

namespace drz.Infrastructure.Services
{
    public class CommandLineMessageService : IMessageService, ICommandLineMessageService
    {
        public CommandLineMessageService(IApplicationInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
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
            WriteMessage("Exception", $"{message}\n{ex.Message}\n{ex.StackTrace.ToString()}", caller);
        }

        private void WriteMessage(string prefix, string message, string caller)
        {
            string formatted =
                   "\n" +
                   (string.IsNullOrWhiteSpace(prefix) ? "" : $"[{prefix}] ") +
                   (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") +
                   message;

#if !TEST
            Document doc = Application.DocumentManager.MdiActiveDocument;

            if (doc != null && doc.Editor != null)
            {
                doc.Editor.WriteMessage(formatted);
            }
            else
            {
                try
                {
                    McNotificator.WriteMessage(formatted);
                }
                catch
                {
                    Application.ShowAlertDialog(formatted);
                }
            }
#else
            Console.WriteLine("\n" + (string.IsNullOrWhiteSpace(prefix) ? "" : $"[{prefix}]\t") +
                          (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") + message);
#endif


        }

        private IApplicationInfo _applicationInfo;
    }
}