using System.Diagnostics;
using System.IO;
using System.Text;

namespace dRz.Loader.Infrastructure.Logging.Diagnostics
{
    /// <summary>
    /// отладочная информация из nLog в output VS только для отладки!!!
    /// </summary>
    /// <seealso cref="TextWriter" />
    public sealed class OutputDebugTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            Debug.WriteLine("[NLog] " + value);
        }

        public override void Write(string? value)
        {
            Debug.Write("[NLog] " + value);
        }
    }
}
//}
