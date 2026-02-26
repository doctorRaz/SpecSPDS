using dRz.SpecSpds.Test.Interfaces;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;



namespace dRz.SpecSpds.Test.Services
{
    internal class MessageService : IMessageService
    {
        private const string prefix = "\n";
        static MessageService()
        {
            Assembly assembly = typeof(MessageService).Assembly;
            _titlePrefix = assembly.GetName().Name + " v." + assembly.GetName().Version + ": ";
          
        }

        /// <summary>
        /// Сообщение в консоль (када или еще чего - не сильно важно)
        /// </summary>
        /// <param name="message">Выводимое сообщение, без начальных и конечных переносов строк</param>
        /// <param name="caller">Вызывающий метода</param>
        public void ConsoleMessage(string message, [CallerMemberName] string? caller = null)
        {
    
            Console.WriteLine(prefix + message);
        }

        /// <summary>
        /// Информационное сообщение<br/> [NotImplemented]
        /// </summary>
        /// <param name="message">Выводимое сообщение, без начальных и конечных переносов строк</param>
        /// <param name="caller">Вызывающий метода</param>
        public void InfoMessage(string message, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
            //MessageBox.Show(message, _titlePrefix, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Сообщение об ошибке<br/> [NotImplemented]
        /// </summary>
        /// <param name="message">Выводимое сообщение, без начальных и конечных переносов строк</param>
        /// <param name="caller">Вызывающий метода</param>
        /// <exception cref="NotImplementedException"></exception>
        public void ErrorMessage(string message, [CallerMemberName] string caller = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Сообщение об исключении
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <param name="caller">Вызывающий метод</param>
        public void ExceptionMessage(Exception ex, [CallerMemberName] string caller = null)
        {
           Console.WriteLine($"{prefix}[{caller}]: {ex.Message}{prefix}{ex.StackTrace}");
                       
        }


        /// <summary>
        /// Сообщение об исключении
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <param name="ex">Исключение</param>
        /// <param name="caller">Вызывающий метод</param>
        public void ExceptionMessage(string message, Exception ex, [CallerMemberName] string caller = null)
        {
           Console.WriteLine($"{prefix}[{caller}]: {message} = {ex.Message}{prefix}{ex.StackTrace}");

       
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static string _titlePrefix;
        private static IntPtr _handle;
    }
}
