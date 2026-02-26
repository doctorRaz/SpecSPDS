using System.Diagnostics;
using System.IO;
using System.Text;

//todo только для отладки загрузки nlog конфигурации
// скопировать в drz.nCad.nCad.InternalDiagnostic

namespace dRz.SpecSpds.Test.Infrastructure.InternalDiagnostic
{
    /// <summary>
    /// отладочная информация из nLog в output VS только для отладки!!!
    /// </summary>
    /// <seealso cref="TextWriter" />
    internal sealed class DebugTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            Debug.WriteLine(value);
        }

        public override void Write(string? value)
        {
            Debug.Write(value);
        }
    }
}