using dRz.SpecSPDS.Core.Services.DeepSeek;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSpdsConsole
{
    public class OutputString
    {

    }
    internal class OutConsole
    {

        public OutConsole(List<GroupedDefinitionMarkerProps> groupedDefinitionMarkerProps)
        {
            var groupedDefinitionMarkerPropsGrouped = groupedDefinitionMarkerProps.GroupBy(p => p.Section);

            foreach (var section in groupedDefinitionMarkerPropsGrouped)
            {

                Console.WriteLine($"\t{section.Key}");
                foreach (var devise in section)
                {
                    Console.WriteLine($"{string.Join(", ", devise.PositionNumbers)} | {devise.DeviceName} | {devise.FirstTypeModel} | {devise.FirstArticleNumber} | {devise.FirstVendor} | {devise.TotalAmount.ToString()} | {devise.FirstUnit}");
                }
                Console.WriteLine();
                Console.WriteLine();
            }

        }

        void PrintConsole()
        {
            Console.WriteLine();
        }
    }
}
