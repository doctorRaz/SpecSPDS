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


        List<McObjectId> _mcObjectIds;

        List<McObjectId> McObjectIds => _mcObjectIds;

        public MultiCadProps(ApplicationSettings settings)
        {
            _stw = new Stopwatch();


        }

        // todo переделать на возвращаемое значение???? в конструктор пихать settings в метод пихать или спасе или список файлов
        public MultiCadProps(List<string> filenames, ApplicationSettings settings)
        {

            //запомним рабочий документ
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

                // делаем рабочим
                McDocument.WorkingDocument = mcDocument;

                //получаем с него ID

                //дергаем сбор свойств


                //после обработки закрываем
                mcDocument.Close();
            }

            //вернем рабочий документ
            McDocument.WorkingDocument = pOldWD;

        }

        public MultiCadProps(Space space, ApplicationSettings settings)
        {
            _stw = new Stopwatch();
            _space = space;

            _fieldName = settings.FieldNames;//имена полей

            _mcUmarkerName = settings.MarkerName;//имя-название маркера

            _isSpec = settings.IsSpec;//флаг что собирать false- собирать все

            _mcObjectIds = new List<McObjectId>();//на всякий

            _stw.Start();

            ObjectFilter of = new ObjectFilter(true);

            of.AddType(new Guid(settings.Guid));


            //с открытых документов
            if (_space == Space.All)
            {
                of.AddDocs(McDocumentsManager.GetDocuments());//открытые документы

                _mcObjectIds = of.GetObjects();
            }
            //с активного документа
            else if (_space == Space.Document)
            {
                of.AddDoc(McDocument.WorkingDocument);

                _mcObjectIds = of.GetObjects();
            }
            //с активного пространства
            else if (_space == Space.Layout)
            {
                _mcObjectIds = of.GetObjects();
            }
            // выбором
            else if (_space == Space.Select)
            {
                _mcObjectIds = McObjectManager.SelectObjects("Выберите McUmarkers <Esc -- Cansel>", false, McUMarker.TypeID).ToList();
            }

            _stw.Stop();
            _tmrID = _stw.Elapsed.ToString();
            _stw.Restart();

            ExtractNamedProperties();//todo возвращаемое значение


        }


        /// <summary>
        /// Extracts all propsSource. Все маркеры и все их свойства в список словарей
        /// собирать все свойства в 30 раз медленнее чем выборочно
        /// </summary>
        public void ExtractAllPropertiesGetProps()
        {
            /*
            Найдено всего: 162364 за 00:00:00.3047090
            Включено в набор: 72162 маркеров за 00:00:01.7906102
            С неподходящим именем: 72160
            Без признака включения в спецификацию: 18040
            С некорректными данными: 2
            prop.GetValue() props           162364 in 00:00:35.3129445
            propsSource.GetValueEx props    162364 in 00:00:26.0401367

            propsSource.GetValueEx  ~ в 1,5 раза быстрее чем prop.GetValue()
            ----

            без List<McProperty> props = propsSource.GetProps(); и цикла по props
            ex props
            GetProps Ex props 162364 in 00:00:27.5163433
            Ex props 162364 in 00:00:28.1478998
            ------------
            Ex props 162364 in 00:00:27.8239544
            GetProps Ex props 162364 in 00:00:29.0826902
			
            цикл по propsSource.GetProps() vs propsSource в пределах погрешности


            ----
            пустой цикл
            GetProps props 0 in 00:00:12.9194806
            PropSourse props 0 in 00:00:13.5995537
            ------------
            PropSourse props 0 in 00:00:12.9806616
            GetProps props 0 in 00:00:13.0468145

            с заполнением словарика на 15 сек дольше!!!

            */

            //test prop.Value

            _stw.Restart();

            _mprops = new List<Dictionary<string, object>>();


            foreach (McObjectId mcObjectId in McObjectIds)//по собранным ID маркеров
            {

                //Dictionary<string, object> mprop = new Dictionary<string, object>();

                McUMarker? tempUmark = McObjectManager.GetObject(mcObjectId) as McUMarker;

                if (tempUmark == null)//не маркер
                {
                    continue;
                }

                //список свойств
                McProperties propsSource = McPropertySource.GetPropertySource(tempUmark).ObjectProperties;

                List<McProperty> props = propsSource.GetProps();


                foreach (McProperty prop in /*propsSource*/ props)

                {
                    //mprop.Add(prop.Name, prop.GetValue());
                    //mprop.Add(prop.Name, propsSource.GetValueEx(prop.Name, ""));
                }

                //_mprops.Add(mprop);
            }
            _stw.Stop();
            _tmrGetProps = _stw.Elapsed.ToString();
        }

        public void ExtractAllPropertiesPropsSource()
        {
            //test EX
            _stw.Restart();

            _mprops = new List<Dictionary<string, object>>();

            foreach (McObjectId mcObjectId in McObjectIds)//по собранным ID маркеров
            {

                //Dictionary<string, object> mprop = new Dictionary<string, object>();

                McUMarker? tempUmark = McObjectManager.GetObject(mcObjectId) as McUMarker;

                if (tempUmark == null)//не маркер
                {
                    continue;
                }

                //список свойств
                McProperties propsSource = McPropertySource.GetPropertySource(tempUmark).ObjectProperties;

                //List<McProperty>  props = propsSource.GetProps();


                foreach (McProperty prop in propsSource /*props*/)

                {
                    //самый быстрый, ~ в 1,5 раза быстрее чем 
                    //mprop.Add(prop.Name, propsSource.GetValueEx(prop.Name, ""));

                }

                //_mprops.Add(mprop);
            }

            _stw.Stop();
            _tmrPopsSource = _stw.Elapsed.ToString();
        }

        public void ExtractNamedProperties()
        {

            int countIncorrectData = 0;//маркеры с отрицательной суммой, некорректными данными
            int countNotFlag = 0;//маркеров без признака включения в спеку
            int countFalseName = 0;//маркеров с не тем именем

            foreach (McObjectId mcObjectId in McObjectIds)//по собранным ID маркеров
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
                    countFalseName++;
                    continue;
                }

                //флаг спецификации
                MarkerProp.FlagSpecRaw = properties.GetValueEx(_fieldName.FlagSpec, "").ToString()?.Trim();

                //учитывать признак спецификации
                if (_isSpec)
                {
                    if (!MarkerProp.FlagSpec)//признака спец нет
                    {
                        countNotFlag++;
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
            ResultString += $"\nНайдено всего: {_mcObjectIds.Count} за {TmrID}";
            ResultString += $"\nВключено в набор: {MarkerProps.Count} маркеров за {TmrProp}";

            if (countFalseName > 0)
            {
                ResultString += $"\nС неподходящим именем: {countFalseName}";
            }

            if (countNotFlag > 0)
            {
                ResultString += $"\nБез признака включения в спецификацию: {countNotFlag}";
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
