using System.Diagnostics;
using System.IO;
using System.Text;

namespace drz.LogBootstrap.Diagnostics
{
    /// <summary>
    /// отладочная информация из nLog в output VS только для отладки!!!
    /// </summary>
    /// <seealso cref="TextWriter" />
    public sealed class OutputDebugTextWriter : TextWriter
    {
        #region Public Properties

        /// <summary>
        /// При переопределении в производном классе возвращает кодировку символов, в которой записаны выходные данные.
        /// </summary>
        public override Encoding Encoding => Encoding.UTF8;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Асинхронно записывает строку в текстовую строку или поток.
        /// </summary>
        /// <param name="value">Строка для записи.</param>
        public override void Write(string value)
        {
            Debug.Write("[NLog] " + value);
        }

        /// <summary>
        /// Записывает в текстовую строку или поток строку, за которой следует признак конца строки.
        /// </summary>
        /// <param name="value">Строка для записи. Если <paramref name="value" /> имеет значение null, записывается только признак конца строки.</param>
        public override void WriteLine(string value)
        {
            Debug.WriteLine("[NLog] " + value);
        }

        #endregion Public Methods
    }
}