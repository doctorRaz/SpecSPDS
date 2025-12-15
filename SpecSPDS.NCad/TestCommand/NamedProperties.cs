using dRz.SpecSPDS.Core.Models;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.NCad.TestCommand
{
    //подгон от крыса
    public class NamedProperties
    {
        public IEnumerable<DefinitionMarkerProps> Get(IEnumerable<McObjectId> objectIds)
        {
            return objectIds.Select(o => McObjectManager.GetObject(o) as McUMarker)
                .Where(o => o != null)
                .Select((o) =>
                {
                    McProperties props = McPropertySource.GetPropertySource(o).ObjectProperties;
                    // Дальше тут наполнение своего класса необходимыми свойствами
                    return new DefinitionMarkerProps();
                });
        }
    }
}
