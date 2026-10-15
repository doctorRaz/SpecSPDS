using dRz.Abstractions.Infrastructure;
using dRz.Abstractions.Services.Message;
using System;
using System.Runtime.CompilerServices;

#if !TEST
using HostMgd.ApplicationServices;
#endif

namespace dRz.NC.Infrastructure.Services.Message
{
    public class CommandLineMessageService : ICommandLineMessageService
    {
        private readonly IAddOnInfo _applicationInfo;

        public CommandLineMessageService(IAddOnInfo applicationInfo)
        {
            _applicationInfo = applicationInfo;
        }

        public void ConsoleMessage(string message, [CallerMemberName] string? caller = null) =>
            WriteMessage(string.Empty, message, caller, null);

        public void WarningMessage(string message, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage("Warn", message, caller, title);

        public void ErrorMessage(Exception ex, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage("Error", $"{ex.Message}\n{ex.StackTrace}", caller, title);

        public void ErrorMessage(string message, Exception? ex = null, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage("Error", ex == null ? message : $"{message}\n{ex.Message}\n{ex.StackTrace}", caller, title);

        public void InfoMessage(string message, [CallerMemberName] string? caller = null, string? title = null) =>
            WriteMessage("Info", message, caller, title);

        private void WriteMessage(string prefix, string message, string? caller, string? title)
        {
            string formatted =
                "\n" +
                (string.IsNullOrWhiteSpace(prefix) ? string.Empty : $"[{prefix}] ") +
                (string.IsNullOrWhiteSpace(title) ? string.Empty : $"[{title}] ") +
                (string.IsNullOrWhiteSpace(caller) ? string.Empty : $"{caller} >> ") +
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
                    McNotificatorMessageServise.WriteMessage(formatted, caller);
                }
                catch
                {
                    Application.ShowAlertDialog(formatted);
                }
            }
#else
            Console.WriteLine(formatted);
#endif
        }
    }
}