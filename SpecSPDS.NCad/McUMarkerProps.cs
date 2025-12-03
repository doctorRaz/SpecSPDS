using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Settings;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;

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

            AppSettings appSettings = new AppSettings();//настройки

            _fieldName = appSettings.Settings.FieldNames;//имена полей

            _mcUmarkerName = appSettings.Settings.MarkerName;//имя-название маркера

            _isSpec = appSettings.Settings.IsSpec;//флаг что собирать false- собирать все

            int counMinus = 0;//маркеры с отрицательной суммой
            int counNotFlag = 0;//маркеров без признака включения в спеку
            int counFalseName = 0;//маркеров с не тем именем

            foreach (McObjectId idSelected in _idSelecteds)//по собранным ID маркеров
            {

                DefinitionMarkerProps MarkerProp = new DefinitionMarkerProps();
                McUMarker? tempUmark = McObjectManager.GetObject(idSelected) as McUMarker;

                MarkerProp.MarkerName = tempUmark?.DbEntity.ObjectProperties.GetValueEx("Name", "").ToString();


                if (string.IsNullOrWhiteSpace(MarkerProp.MarkerName)
                    || MarkerProp.MarkerName.IndexOf(_mcUmarkerName,
                                                     StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    counFalseName++;
                    continue;
                }

                McProperties? allProp = tempUmark?.DbEntity.ObjectProperties;

                MarkerProp.FlagSpecRaw = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.FlagSpec, "").ToString()?.Trim();

                if (_isSpec)//учитывать признак спецификации
                {
                    if (!MarkerProp.FlagSpec)//признака спец нет
                    {
                        counNotFlag++;
                        continue;
                    }
                }

                MarkerProp.AmountRaw = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Amount, "").ToString()?.Trim();

                //проверка на некорректное значение количества, если минус, то не включать в набор
                if (MarkerProp.Amount < 0)//маркеры с количеством меньше нуля не включаем в набор
                {
                    counMinus++;
                    continue;
                }

                MarkerProp.Section = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Section, "").ToString()?.Trim();
                MarkerProp.PositionNumber = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.PositionNumber, "").ToString()?.Trim();
                MarkerProp.DeviceName = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.DeviceName, "").ToString()?.Trim();
                MarkerProp.TypeModel = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.TypeModel, "").ToString()?.Trim();
                MarkerProp.ArticleNumber = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.ArticleNumber, "").ToString()?.Trim();
                MarkerProp.Vendor = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Vendor, "").ToString()?.Trim();
                MarkerProp.Unit = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Unit, "").ToString()?.Trim();
                MarkerProp.UnitMass = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.UnitMass, "").ToString()?.Trim();
                MarkerProp.Comment = tempUmark?.DbEntity.ObjectProperties.GetValueEx(_fieldName.Comment, "").ToString()?.Trim();


                MarkerProps.Add(MarkerProp);
            }
            if (MarkerProps.Count > 0)
            {
                IsOk = true;
            }

            ResultString = $"\nМаркеры";
            ResultString += $"\nВыбрано всего: {_idSelecteds.Count}";
            ResultString += $"\nВключено в набор: {MarkerProps.Count} маркеров";

            if (counFalseName > 0)
            {
                ResultString += $"\nС неподходящим именем: {counFalseName}";
            }

            if (counNotFlag > 0)
            {
                ResultString += $"\nБез признака включения в спецификацию: {counNotFlag}";
            }

            if (counMinus > 0)
            {
                ResultString += $"\nС некорректной суммой (столбец \"Количество\"): {counMinus}";
            }

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
