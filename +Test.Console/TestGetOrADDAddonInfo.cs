using drz.Abstractions.Infrastructure;
using drz.Infrastructure.Infrastructure;
using System.Collections.Generic;
using System.Reflection;

namespace drz.SpecSPDS.Test
{
    internal class TestGetOrADDAddonInfo
    {
        private readonly IAddOnInfoRegistry _iadR;

        internal TestGetOrADDAddonInfo()
        {
            //_assembly = assembly;
            _iadR = new AddOnInfoRegistry();
        }

        internal IAddOnInfo AddAssembly(Assembly assembly)
        {
            return _iadR.Register(assembly);
        }

        internal IReadOnlyCollection<IAddOnInfo> GetAll()
        {
            return _iadR.GetValues();
        }

        internal ICollection<string> GetKeys()
        {
            return _iadR.GetKeys();
        }
    }
}