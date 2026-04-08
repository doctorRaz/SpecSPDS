using drz.SpecSPDS.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace drz.SpecSPDS.Core.Services._DeepSeek
{
    /// <summary>
    /// сортировка группировка суммирование свойств
    /// </summary>
    public class GroupAmount
    {

        public GroupAmount(List<DefinitionMarkerProps> props)
        {
            _props = props;

            List<DefinitionMarkerProps> markers = props;





            var compactResult = markers
                                .GroupBy(m => new { m.Section, m.DeviceName })
                                .OrderBy(g => g.Key.Section)
                                .ThenBy(g => g.Key.DeviceName)
                                .Select(g => new
                                {
                                    g.Key.Section,
                                    g.Key.DeviceName,
                                    TotalAmount = g.Sum(m => m.Amount),
                                    PositionNumbers = g
                                        .Select(m => m.PositionNumber)
                                        .Distinct()
                                        .OrderBy(p => p)
                                        .ToList(),

                                    // Берем первое непустое значение каждого свойства
                                    g.FirstOrDefault(m => !string.IsNullOrEmpty(m.TypeModel))?.TypeModel,
                                    g.FirstOrDefault(m => !string.IsNullOrEmpty(m.ArticleNumber))?.ArticleNumber,
                                    g.FirstOrDefault(m => !string.IsNullOrEmpty(m.Vendor))?.Vendor,
                                    g.FirstOrDefault(m => !string.IsNullOrEmpty(m.Unit))?.Unit,

                                    // Или все значения
                                    AllTypeModels = g.Select(m => m.TypeModel).Distinct().ToList(),
                                    AllArticleNumbers = g.Select(m => m.ArticleNumber).Distinct().ToList(),
                                    AllVendors = g.Select(m => m.Vendor).Distinct().ToList(),
                                    AllUnits = g.Select(m => m.Unit).Distinct().ToList()
                                })
                                .GroupBy(x => x.Section)
                                .Select(g => new
                                {
                                    Section = g.Key,
                                    Devices = g.ToList()
                                })
                                .ToList();

            var result =
                        from marker in markers
                        group marker by marker.Section into sectionGroup
                        orderby sectionGroup.Key
                        select new
                        {
                            Section = sectionGroup.Key,
                            Devices =
                                from device in sectionGroup
                                group device by device.DeviceName into deviceGroup
                                orderby deviceGroup.Key
                                select new
                                {
                                    DeviceName = deviceGroup.Key,
                                    TotalAmount = deviceGroup.Sum(d => d.Amount),
                                    PositionNumbers = deviceGroup
                                        .Select(d => d.PositionNumber)
                                        .Distinct()
                                        .OrderBy(p => p)
                                        .ToList()
                                }
                        };

            //группировка по Раздел

            IEnumerable<IGrouping<string, DefinitionMarkerProps>> SectioGroup = _props.GroupBy(p => p.Section);

            //SectioGroup=

            foreach (IGrouping<string, DefinitionMarkerProps> section in SectioGroup)
            {

                string nameGroup = section.Key;
                IEnumerable<IGrouping<string, DefinitionMarkerProps>> devise = section.GroupBy(p => p.DeviceName);

                foreach (IGrouping<string, DefinitionMarkerProps> dev in devise)
                {
                    string namedev = dev.Key;
                    double amount = dev.Sum(p => p.Amount);

                    //var poz = dev.Aggregate(p => p.PositionNumber);
                }

            }


        }



        public List<DefinitionMarkerProps> Props => _props;

        private List<DefinitionMarkerProps> _props;
    }
}
