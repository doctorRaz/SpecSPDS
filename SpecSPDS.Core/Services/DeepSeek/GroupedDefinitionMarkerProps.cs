using dRz.SpecSPDS.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dRz.SpecSPDS.Core.Services.DeepSeek
{
    public class GroupedDefinitionMarkerProps
    {
        public string Section { get; set; } = "";
        public string DeviceName { get; set; } = "";
        public double TotalAmount { get; set; }
        public List<string> PositionNumbers { get; set; } = new List<string>();
        public List<string> Comments { get; set; } = new List<string>();

        // Первые значения из группировки
        public string FirstMarkerName { get; set; } = "";
        public string FirstTypeModel { get; set; } = "";
        public string FirstArticleNumber { get; set; } = "";
        public string FirstVendor { get; set; } = "";
        public string FirstUnit { get; set; } = "";
        public string FirstUnitMass { get; set; } = "";
        public bool FirstFlagSpec { get; set; }
    }


    //https://chat.deepseek.com/a/chat/s/69f8a77a-7ee8-48af-b977-c1e8e838e0a9
    public static class DefinitionMarkerPropsGrouper
    {


        public static List<GroupedDefinitionMarkerProps> GroupAndSortDefinitionMarkers(
            List<DefinitionMarkerProps> markers)
        {
            return markers
                // Группировка по Section
                .GroupBy(m => m.Section)
                // Сортировка по Section
                .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                .SelectMany(sectionGroup => sectionGroup
                    // Внутри каждой секции группировка по DeviceName
                    .GroupBy(m => m.DeviceName)
                    // Сортировка по DeviceName внутри секции
                    .OrderBy(g => g.Key, StringComparer.OrdinalIgnoreCase)
                    .Select(deviceGroup => new GroupedDefinitionMarkerProps
                    {
                        Section = sectionGroup.Key,
                        DeviceName = deviceGroup.Key,
                        TotalAmount = deviceGroup.Sum(m => m.Amount),

                        // Список PositionNumber с удалением дубликатов и сортировкой
                        PositionNumbers = deviceGroup
                            .Select(m => m.PositionNumber)
                            .Where(pn => !string.IsNullOrWhiteSpace(pn))
                            .Distinct()
                            .OrderBy(pn => pn, StringComparer.OrdinalIgnoreCase)
                            .ToList(),

                        // Список Comment с удалением дубликатов и сортировкой
                        Comments = deviceGroup
                            .Select(m => m.Comment)
                            .Where(c => !string.IsNullOrWhiteSpace(c))
                            .Distinct()
                            .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                            .ToList(),

                        // Первые значения остальных свойств
                        FirstMarkerName = deviceGroup.First().MarkerName,
                        FirstTypeModel = deviceGroup.First().TypeModel,
                        FirstArticleNumber = deviceGroup.First().ArticleNumber,
                        FirstVendor = deviceGroup.First().Vendor,
                        FirstUnit = deviceGroup.First().Unit,
                        FirstUnitMass = deviceGroup.First().UnitMass,
                        FirstFlagSpec = deviceGroup.First().FlagSpec
                    }))
                .ToList();
        }
    }
}