using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Core.Models
{
    /// <summary>
    /// значения полей маркера
    /// </summary>
    public class DefinitionMarkerProps
    {
        public DefinitionMarkerProps() { }

        /// <summary>
        /// Имя маркера
        /// </summary>
        public string MarkerName { get; set; }  

        /// <summary>
        /// Раздел спецификации
        /// </summary>
        public string Section { get; set; } 

        /// <summary>
        /// Позиция
        /// </summary>
        public string PositionNumber { get; set; }  

        /// <summary>
        /// Наименование и техническая характеристика
        /// </summary>
        public string DeviceName { get; set; }  

        /// <summary>
        /// Тип, марка, обозначение документа, опросного листа
        /// </summary>
        public string TypeModel { get; set; }  

        /// <summary>
        /// Код продукции
        /// </summary>
        public string ArticleNumber { get; set; }  

        /// <summary>
        /// Поставщик
        /// </summary>
        public string Vendor { get; set; }  

        /// <summary>
        /// Единица измерения
        /// </summary>
        public string Unit { get; set; } 

        /// <summary>
        /// Количество
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Количество строкой
        /// </summary>
        public string AmountRaw { get; set; } 

        /// <summary>
        /// Масса 1 ед.кг
        /// </summary>
        public string UnitMass { get; set; }  

        /// <summary>
        /// Примечание
        /// </summary>
        public string Comment { get; set; }  

        /// <summary>
        /// Флаг включения в спецификацию 
        /// </summary>         
        public bool FlagSpec { get; set; }

        /// <summary>
        /// Флаг включения в спецификацию строкой
        /// </summary>
        public string FlagSpecRaw { get; set; } 


    }
}
