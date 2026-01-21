

using System.Diagnostics;
using System.IO;
using System.Text;

//todo только для отладки загрузки nlog конфигурации
// скопировать в dRz.SpecSPDS.Cad.Bootstrap

namespace dRz.SpecSPDS.Core.InternalDiagnostic
{
    /// <summary>
    /// отладочная информация из nLog в output VS
    /// </summary>
    /// <seealso cref="TextWriter" />
    sealed class DebugTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string value)
        {
            Debug.WriteLine(value);
        }

        public override void Write(string value)
        {
            Debug.Write(value);
        }
    }
}