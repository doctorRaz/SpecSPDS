using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Settings;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace dRz.SpecSPDS.NCad.Services
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
        Guid guid => _guid;
        Guid _guid;

        public MultiCadProps(ApplicationSettings settings)
        {
            _stw = new Stopwatch();//часики

            _guid = (new Guid(settings.Guid));//какой объект работаем

            _fieldName = settings.FieldNames;//имена полей

            _mcUmarkerName = settings.MarkerName;//имя-название маркера

            _isSpec = settings.IsSpec;//флаг что собирать false- собирать все

            //mcObjectIds = new List<McObjectId>();//на всякий

        }

        // todo переделать на возвращаемое значение???? в конструктор пихать settings в метод пихать или спасе или список файлов

        /// <summary>
        /// Gets the props.
        /// </summary>
        /// <param name="filenames">список путей к файлам чертежей</param>
        /// <returns>Список свойств мультикад объектов</returns>
        public List<DefinitionMarkerProps> GetProps(List<string> filenames)
        {
            _stw.Start();//todo счетчики переделать все, отдельно на IDs отдельно на props

            List<DefinitionMarkerProps> markerProps = new List<DefinitionMarkerProps>();

            List<McObjectId> mcObjectIds = new List<McObjectId>();

            Statistics counter = new Statistics();

            ObjectFilter of = new ObjectFilter(/*true*/);// новый фильтр долльше чем его сбросить

            //запомним рабочий документ на всякий
            McDocument pOldWD = McDocument.WorkingDocument;

            foreach (string filename in filenames)
            {


                //если открыт то не нулл
                McDocument mcDocument = McDocumentsManager.GetDocument(filename);
                if (mcDocument == null)
                {
                    // открываем файл в скрытом режиме
                    mcDocument = McDocumentsManager.OpenDocument(filename, false, true);
                }

                of.Reset();
                of.AddType(guid);
                of.AddDoc(mcDocument);

                //получаем с него ID
                mcObjectIds = of.GetObjects();
                //дергаем сбор свойств
                ExtractNamedProps(mcObjectIds, ref markerProps, ref counter);

                //после обработки закрываем
                if (mcDocument.IsHidden) mcDocument.Close();//todo если не открывали не закрывать
            }

            _stw.Stop();
            _tmrProp = _stw.Elapsed.ToString();
            //вернем рабочий документ мало ли
            McDocument.WorkingDocument = pOldWD;

            return markerProps;

        }


        /// <summary>
        /// Gets the props.
        /// </summary>
        /// <param name="space">С какого пространства получать</param>
        /// <returns>Список свойств мультикад объектов</returns>
        public List<DefinitionMarkerProps> GetProps(Space space)
        {
            /*
            //_stw = new Stopwatch();
            //_space = space;

            //_fieldName = settings.FieldNames;//имена полей



            //_mcUmarkerName = settings.MarkerName;//имя-название маркера

            //_isSpec = settings.IsSpec;//флаг что собирать false- собирать все

            //mcObjectIds = new List<McObjectId>();//на всякий

            */

            _stw.Start();

            List<DefinitionMarkerProps> markerProps = new List<DefinitionMarkerProps>();

            List<McObjectId> mcObjectIds = new List<McObjectId>();

            Statistics counter = new Statistics();

            ObjectFilter of = new ObjectFilter(true);

            of.AddType(guid);

            //с открытых документов
            if (space == Space.All)
            {
                of.AddDocs(McDocumentsManager.GetDocuments());//открытые документы

                mcObjectIds = of.GetObjects();
            }
            //с активного документа
            else if (space == Space.Document)
            {
                of.AddDoc(McDocument.WorkingDocument);

                mcObjectIds = of.GetObjects();
            }
            //с активного пространства
            else if (space == Space.Layout)
            {
                mcObjectIds = of.GetObjects();
            }
            // выбором
            else if (space == Space.Select)
            {
                mcObjectIds = McObjectManager.SelectObjects("Выберите McUmarkers <Esc -- Cansel>", false, guid /*McUMarker.TypeID*/).ToList();
            }

            _stw.Stop();
            _tmrID = _stw.Elapsed.ToString();
            _stw.Restart();

            ExtractNamedProps(mcObjectIds, ref markerProps, ref counter);

            return markerProps;
        }
        void ExtractNamedProps(List<McObjectId> mcObjectIds, ref List<DefinitionMarkerProps> markerProps, ref Statistics counter)
        {
            /*
            tempUmark. DbEntity.Document тут IsModel, Name - имя листа, Path - genm ljrevtynf
            DbEntity слой тип линий масштабы и прочее

            */
            //int countIncorrectData = 0;//маркеры с отрицательной суммой, некорректными данными
            //int countNotFlag = 0;//маркеров без признака включения в спеку
            //int countFalseName = 0;//маркеров с не тем именем

            foreach (McObjectId mcObjectId in mcObjectIds)//по собранным ID маркеров
            {

                DefinitionMarkerProps MarkerProp = new DefinitionMarkerProps();
                McUMarker? tempUmark = McObjectManager.GetObject(mcObjectId) as McUMarker;

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
                    counter.countFalseName++;
                    continue;
                }

                //флаг спецификации
                MarkerProp.FlagSpecRaw = properties.GetValueEx(_fieldName.FlagSpec, "").ToString()?.Trim();

                //учитывать признак спецификации
                if (_isSpec)
                {
                    if (!MarkerProp.FlagSpec)//признака спец нет
                    {
                        counter.countNotFlag++;
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
                    counter.countIncorrectData++;
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

                markerProps.Add(MarkerProp);
            }
            //todo вынести статистику в класс
            /*
            #region Вынести счетчики в класс


            _stw.Stop();
            _tmrProp = _stw.Elapsed.ToString();

            if (markerProps.Count > 0)
            {
               counter. IsOk = true;
            }

            ResultString = $"\nМаркеры:";
            ResultString += $"\nНайдено всего: {mcObjectIds.Count} за {TmrID}";
            ResultString += $"\nВключено в набор: {markerProps.Count} маркеров за {TmrProp}";

            if (counter.countFalseName > 0)
            {
                ResultString += $"\nС неподходящим именем: {counter.countFalseName}";
            }

            if (counter.countNotFlag > 0)
            {
                ResultString += $"\nБез признака включения в спецификацию: {counter.countNotFlag}";
            }

            if (counter.countIncorrectData > 0)
            {
                ResultString += $"\nС некорректными данными: {counter.countIncorrectData}";
            }

            #endregion
            */
        }

        Stopwatch _stw { get; set; }


        List<Dictionary<string, object>> _mprops;
        public List<Dictionary<string, object>> Mprops => _mprops;
        public string TmrGetProps => _tmrGetProps;

        string _tmrGetProps;
        public string TmrPropSourse => _tmrPopsSource;

        string _tmrPopsSource;


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


        //List<McObjectId> mcObjectIds;

        //List<McObjectId> mcObjectIds => mcObjectIds;

        /// <summary>
        /// Gets or sets the result string.
        /// </summary>
        /// <value>
        /// The result string.
        /// </value>
        public string ResultString { get; set; } = "";



        //public List<DefinitionMarkerProps> MarkerProps { get; set; } = new List<DefinitionMarkerProps>();

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

    internal class Statistics
    {

        internal int countIncorrectData = 0;//маркеры с отрицательной суммой, некорректными данными
        internal int countNotFlag = 0;//маркеров без признака включения в спеку
        internal int countFalseName = 0;//маркеров с не тем именем

        /// <summary>
        /// Gets or sets a value indicating whether this instance is ok.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ok; otherwise, <c>false</c>.
        /// </value>
        internal bool IsOk { get; set; }

    }

}
