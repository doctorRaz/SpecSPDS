using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Settings;
using dRz.SpecSPDS.Enums;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using Multicad.Symbols.Tables;
using System.Security.Cryptography.X509Certificates;

namespace dRz.SpecSPDS
{
    /// <summary>
    /// получаем универсальный маркер
    /// выбором на чертеже
    /// по текущему пространству
    /// со всего документа
    /// с нескольких документов (когданить потом)
    /// </summary>
    public partial class McUmarkerProps

    {

        public McUmarkerProps(Space space)
        {
            _space = space;



            //todo вынести в метод?
            if (_space == Space.All)
            {
                _idSelecteds = ObjectFilter.Create(false).AddDoc(McDocument.WorkingDocument).AddType(McUMarker.TypeID).GetObjects();//get McUMarker all space curent doc
            }
            else if (_space == Space.Layout)
            {
                //_idSelecteds = ObjectFilter.Create(true).AddDoc(McDocument.WorkingDocument).AddType(McUMarker.TypeID).GetObjects().ToArray();//get McUMarker current space
                _idSelecteds = ObjectFilter.Create(true).AddType(McUMarker.TypeID).GetObjects();//get McUMarker current space
            }
            else if (_space == Space.Select)
            {
                //select McUmarkers
                _idSelecteds = McObjectManager.SelectObjects("Выберите McUmarkers <Esc -- Cansel>", false, McUMarker.TypeID).ToList();
            }


            if (_idSelecteds == null || _idSelecteds.Count == 0)//not found
            {
                ResultString = $"Не найден ни один маркер!";
                return;
            }

            AppSettings appSettings = new AppSettings();

            _fieldName = appSettings.Settings.FieldNames;

            _mcUmarkerName = appSettings.Settings.MarkerName;

            _isSpec = appSettings.Settings.IsSpec;

            //MarkerProps = new List<DefinitionMarkerProps>();


            foreach (McObjectId idSelected in _idSelecteds)
            {

                DefinitionMarkerProps MarkerProp = new DefinitionMarkerProps();
                McUMarker? tempTbl = McObjectManager.GetObject(idSelected) as McUMarker;

                string tempTitle = tempTbl?.DbEntity.ObjectProperties.GetValueEx("Name", "").ToString();


                if (string.IsNullOrWhiteSpace(tempTitle) || tempTitle.IndexOf(_mcUmarkerName, StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    continue;
                }

                McProperties? allProp = tempTbl?.DbEntity.ObjectProperties;

                MarkerProp.FlagSpecRaw = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.FlagSpec, "").ToString()?.Trim();

                if (_isSpec)
                {
                    if (!MarkerProp.FlagSpec)
                    {
                        continue;
                    }
                }
                //todo проверку на некорректное значение количества, если минус, то не включать в набор


                MarkerProp.Section = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Section, "").ToString()?.Trim();
                MarkerProp.PositionNumber = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.PositionNumber, "").ToString()?.Trim();
                MarkerProp.DeviceName = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.DeviceName, "").ToString()?.Trim();
                MarkerProp.TypeModel = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.TypeModel, "").ToString()?.Trim();
                MarkerProp.ArticleNumber = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.ArticleNumber, "").ToString()?.Trim();
                MarkerProp.Vendor = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Vendor, "").ToString()?.Trim();
                MarkerProp.Unit = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Unit, "").ToString()?.Trim();
                MarkerProp.AmountRaw = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Amount, "").ToString()?.Trim();
                MarkerProp.UnitMass = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.UnitMass, "").ToString()?.Trim();
                MarkerProp.Comment = tempTbl?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Comment, "").ToString()?.Trim();


                MarkerProps.Add(MarkerProp);
            }
            if (MarkerProps.Count > 0)
            {
                IsOk = true;
            }

            ResultString = $"Найдено {MarkerProps.Count} маркеров";

        }


        /// <summary>
        /// Gets or sets the result string.
        /// </summary>
        /// <value>
        /// The result string.
        /// </value>
        public string ResultString { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ok.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ok; otherwise, <c>false</c>.
        /// </value>
        public bool IsOk { get; set; }

        public List<DefinitionMarkerProps> MarkerProps { get; set; } = new List<DefinitionMarkerProps>();

        /// <summary>
        /// Gets or sets the name of the field.
        /// </summary>
        /// <value>
        /// The name of the field.
        /// </value>
        FieldNameSettings _fieldName { get; set; }
        string _mcUmarkerName { get; set; }

        /// <summary>
        /// ОБрабатывать маркеры с флагом включения в спеку
        /// </summary>
        /// <value>
        ///   <c>true</c>обрабатывать маркеры только с флагом включения в спеку; otherwise, <c>false</c>.
        /// </value>
        bool _isSpec { get; set; }
        List<McObjectId> _idSelecteds { get; set; }

        //DefinitionMarkerProps MarkerProp { get; set; }

        /// <summary>
        /// тип пространства откуда брать
        /// </summary>
        Space _space { get; set; }
    }
}
