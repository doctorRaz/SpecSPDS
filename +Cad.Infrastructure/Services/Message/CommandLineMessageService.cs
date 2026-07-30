using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;

//все связанное с HostMgd в отдельную сборку CadInfrastructure
#if !TEST

using HostMgd.ApplicationServices;

#endif

namespace drz.n.Infrastructure.Services.Message
{
    public class CommandLineMessageService : ICommandLineMessageService
    {
        #region Private Fields

        private readonly IAddOnInfo _applicationInfo;

        #endregion Private Fields

        #region Public Constructors

        public CommandLineMessageService(IAddOnInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
        }

        #endregion Public Constructors

        #region Public Methods

        public void ConsoleMessage(string message, [CallerMemberName] string caller = null)
        {
            WriteMessage("", message, caller);
        }

        public void WarningMessage(string message, [CallerMemberName] string caller = null)
        {
            WriteMessage("Error", message, caller);
        }

        public void ErrorMessage(Exception ex, [CallerMemberName] string caller = null)
        {
            WriteMessage("Exception", $"{ex.Message}\n{ex.StackTrace}", caller);
        }

        /// <summary>
        /// Сообщение об исключении
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="caller">Вызывающий метод</param>
        public void ErrorMessage(string message, Exception ex = null, [CallerMemberName] string caller = null)
        {
            WriteMessage("Exception", $"{message}\n{ex.Message}\n{ex.StackTrace.ToString()}", caller);
        }

        /// <summary>Информационное сообщение для CAD</summary>
        /// <param name="message">Выводимое сообщение</param>
        /// <param name="caller"></param>
        /// <example>Пример использования в C#
        /// <code language="cs"><![CDATA[
        /// IMessageService msgService = new MyMessageService();
        /// msgService.InfoMessage("Информационное сообщение. Может быть, в аналог MessageBox");
        /// ]]></code></example>
        public void InfoMessage(string message, [CallerMemberName] string caller = null)
        {
            WriteMessage("Info", message, caller);
        }

        #endregion Public Methods

        #region Private Methods

        private void WriteMessage(string prefix, string message, string caller)
        {
            string formatted =
                   "\n" +
                   (string.IsNullOrWhiteSpace(prefix) ? "" : $"[{prefix}] ") +
                   (string.IsNullOrWhiteSpace(caller) ? "" : $"{caller} >> ") +
                   message;

#if !TEST
            //todo получать документ из контейнера?
            Document doc = Application.DocumentManager.MdiActiveDocument;

            if (doc != null && doc.Editor != null)
            {
                doc.Editor.WriteMessage(formatted);
            }
            else
            {
                try
                {
                    //todo для нотифай отдельный метод и интерфейс
                    McNotificatorMessageServise.WriteMessage(formatted);
                }
                catch
                {
                    //fallback
                    Application.ShowAlertDialog(formatted);
                }
            }
#else
            Console.WriteLine(formatted);
#endif
        }

        #endregion Private Methods
    }
}