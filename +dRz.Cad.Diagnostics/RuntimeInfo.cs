using dRz.Cad.Diagnostics.Cad;
using dRz.Cad.Diagnostics.Os;
using System;

namespace dRz.Cad.Diagnostics
{
    public sealed class RuntimeInfo
    {
        private static readonly Lazy<RuntimeInfo> _current =
            new(() => new RuntimeInfo(), true);

        public static RuntimeInfo Current => _current.Value;

        public InfoOs Os { get; }
        public InfoCad Cad { get; }

        private RuntimeInfo()
        {
            Os = InfoOs.Current;
            Cad = InfoCad.Current;
        }

        public override string ToString()
        {
            return $"{Cad}\n{Os}";
        }
    }

    public static class RT
    {
        public static RuntimeInfo Info => RuntimeInfo.Current;

        public static InfoCad Cad => Info.Cad;

        public static InfoOs Os => Info.Os;
    }
}
