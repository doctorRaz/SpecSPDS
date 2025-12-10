using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Settings;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace dRz.SpecSPDS
{
    /// <summary>
    /// получаем универсальный маркер
    /// выбором на чертеже
    /// по текущему пространству
    /// со всего документа
    /// с нескольких документов (когданить потом)
    /// </summary>
    public partial class MultiCadProps

    {
        Stopwatch _stw { get; set; }

        string _tmrID;

        /// <summary>
        /// Gets the TMR identifier.
        /// </summary>
        /// <value>
        /// The TMR identifier.
        /// </value>
        public string TmrID => _tmrID;
        string _tmrProp;

        /// <summary>
        /// Gets the TMR property.
        /// </summary>
        /// <value>
        /// The TMR property.
        /// </value>
        public string TmrProp => _tmrProp;


        List<McObjectId> _idUmarkers;

        List<McObjectId> IdUmarkers => _idUmarkers;

        public MultiCadProps(Space space, ApplicationSettings settings)
        {
            _stw = new Stopwatch();
            _space = space;

            _fieldName = settings.FieldNames;//имена полей

            _mcUmarkerName = settings.MarkerName;//имя-название маркера

            _isSpec = settings.IsSpec;//флаг что собирать false- собирать все

            _idUmarkers = new List<McObjectId>();//на всякий

            _stw.Start();

            ObjectFilter of = new ObjectFilter(true);

            of.AddType(new Guid(settings.Guid));


            //с открытых документов
            if (_space == Space.All)
            {
                List<McDocument> docs = McDocumentsManager.GetDocuments();

                of.AddDocs(McDocumentsManager.GetDocuments());//открытые документы

                _idUmarkers = of.GetObjects();
            }
            //с активного документа
            else if (_space == Space.Document)
            {
                of.AddDoc(McDocument.WorkingDocument);

                _idUmarkers = of.GetObjects();
            }
            //с активного пространства
            else if (_space == Space.Layout)
            {
                _idUmarkers = of.GetObjects();
            }
            // выбором
            else if (_space == Space.Select)
            {
                _idUmarkers = McObjectManager.SelectObjects("Выберите McUmarkers <Esc -- Cansel>", false, McUMarker.TypeID).ToList();
            }

            _stw.Stop();
            _tmrID = _stw.Elapsed.ToString();
            _stw.Restart();

            ExtractProperties();


        }

        public void ExtractProperties()
        {

            int countIncorrectData = 0;//маркеры с отрицательной суммой
            int counNotFlag = 0;//маркеров без признака включения в спеку
            int counFalseName = 0;//маркеров с не тем именем

            foreach (McObjectId idSelected in _idUmarkers)//по собранным ID маркеров
            {

                DefinitionMarkerProps MarkerProp = new DefinitionMarkerProps();
                McUMarker? tempUmark = McObjectManager.GetObject(idSelected) as McUMarker;

                if (tempUmark == null)//не маркер
                {
                    continue;
                }

                //список свойств
                McProperties properties = McPropertySource.GetPropertySource(tempUmark).ObjectProperties;

                //имя маркера
                MarkerProp.MarkerName = properties.GetValueEx(_fieldName.Name, "").ToString()?.Trim();

                //имени нет или не то
                if (string.IsNullOrWhiteSpace(MarkerProp.MarkerName)
                    || MarkerProp.MarkerName.IndexOf(_mcUmarkerName,
                                                     StringComparison.InvariantCultureIgnoreCase) < 0)
                {
                    counFalseName++;
                    continue;
                }

                //флаг спецификации
                MarkerProp.FlagSpecRaw = properties.GetValueEx(_fieldName.FlagSpec, "").ToString()?.Trim();

                //учитывать признак спецификации
                if (_isSpec)
                {
                    if (!MarkerProp.FlagSpec)//признака спец нет
                    {
                        counNotFlag++;
                        continue;
                    }
                }

                //количество строка
                MarkerProp.AmountRaw = properties.GetValueEx(_fieldName.Amount, "").ToString()?.Trim();

                //наименование
                MarkerProp.DeviceName = properties.GetValueEx(_fieldName.DeviceName, "").ToString()?.Trim();

                //проверка на некорректные данные, если количество минус или наименование пустое, то не включать в набор
                if (MarkerProp.Amount < 0 || string.IsNullOrWhiteSpace(MarkerProp.DeviceName))
                {
                    countIncorrectData++;
                    continue;
                }

                MarkerProp.Section = properties.GetValueEx(_fieldName.Section, "").ToString()?.Trim();
                MarkerProp.PositionNumber = properties.GetValueEx(_fieldName.PositionNumber, "").ToString()?.Trim();
                MarkerProp.TypeModel = properties.GetValueEx(_fieldName.TypeModel, "").ToString()?.Trim();
                MarkerProp.ArticleNumber = properties.GetValueEx(_fieldName.ArticleNumber, "").ToString()?.Trim();
                MarkerProp.Vendor = properties.GetValueEx(_fieldName.Vendor, "").ToString()?.Trim();
                MarkerProp.Unit = properties.GetValueEx(_fieldName.Unit, "").ToString()?.Trim();
                MarkerProp.UnitMass = properties.GetValueEx(_fieldName.UnitMass, "").ToString()?.Trim();
                MarkerProp.Comment = properties.GetValueEx(_fieldName.Comment, "").ToString()?.Trim();

                MarkerProps.Add(MarkerProp);
            }

            if (MarkerProps.Count > 0)
            {
                IsOk = true;
            }

            _stw.Stop();
            _tmrProp = _stw.Elapsed.ToString();

            ResultString = $"\nМаркеры:";
            ResultString += $"\nНайдено всего: {_idUmarkers.Count} за {TmrID}";
            ResultString += $"\nВключено в набор: {MarkerProps.Count} маркеров за {TmrProp}";

            if (counFalseName > 0)
            {
                ResultString += $"\nС неподходящим именем: {counFalseName}";
            }

            if (counNotFlag > 0)
            {
                ResultString += $"\nБез признака включения в спецификацию: {counNotFlag}";
            }

            if (countIncorrectData > 0)
            {
                ResultString += $"\nС некорректными данными: {countIncorrectData}";
            }

        }


        /// <summary>
        /// Gets or sets the result string.
        /// </summary>
        /// <value>
        /// The result string.
        /// </value>
        public string ResultString { get; set; } = "";

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
        ///   <c>true</c> обрабатывать маркеры только с флагом включения в спеку; otherwise, <c>false</c>.
        /// </value>
        bool _isSpec { get; set; }


        //DefinitionMarkerProps MarkerProp { get; set; }

        /// <summary>
        /// тип пространства откуда брать
        /// </summary>
        Space _space { get; set; }
    }
}
