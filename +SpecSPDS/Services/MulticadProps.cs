using drz.SpecSPDS.Core.Enums;
using drz.SpecSPDS.Core.Models;
using drz.SpecSPDS.Core.Settings;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

//using CAD = HostMgd.ApplicationServices.Application;
using Db = Teigha.DatabaseServices;

namespace drz.SpecSPDS.Services;

/// <summary>
/// получаем универсальный маркер
/// выбором на чертеже
/// по текущему пространству
/// со всего документа
/// с нескольких документов (когданить потом)
/// </summary>
public partial class MultiCadProps

{

    private static readonly ILogger log = LogManager.GetCurrentClassLogger();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="settings"></param>
    public MultiCadProps(ApplicationSettings settings)
    {

        _guid = new Guid(settings.Guid);//какой объект работаем

        _fieldName = settings.FieldNames;//имена полей

        _mcUmarkerName = settings.MarkerName;//имя-название маркера

        _isSpec = settings.IsSpec;//признак сбора в спецификацию -true, только с флагом сбора
                                  //false - собирать все

        #region Logger

        Version _version = HostMgd.ApplicationServices.Application.Version;

        string _appProductName = System.Windows.Forms.Application.ProductName;
        string _sender = $"{_appProductName}_{_version.Major.ToString()}.{_version.Minor.ToString()}";


        #endregion

    }

    public List<DefinitionMarkerProps> GetPropsTG(List<string> filenames)
    {


        log.Trace($"Total files: {filenames.Count}");

        List<DefinitionMarkerProps> markerProps = new List<DefinitionMarkerProps>();


        List<McObjectId> mcObjectIds = new List<McObjectId>();

        ObjectFilter of = new ObjectFilter(/*true*/);// новый фильтр дольше чем его сбросить

        _countFilesTotal = filenames.Count;


        //запомним рабочий документ на всякий
        McDocument pOldWD = McDocument.WorkingDocument;
        //!старая база запомним
        Db.Database dbCarent = Db.HostApplicationServices.WorkingDatabase;

        int count = 0;

        foreach (string filename in filenames)
        {
            //_stwID.Start();

            count++;

            log.Trace($"{count} OPEN {filename}");

            //если открыт то не нулл
            //McDocument mcDocument = McDocumentsManager.GetDocument(filename);
            //if (mcDocument == null)
            //{
            // открываем файл в тайге поставить тру?
            using (Db.Database db0 = new Db.Database(false, false))
            {
                try
                {
                    // Ни одна из перегрузок метода "ReadDwgFile" не принимает 5 аргументов.PlotSPDSa21

                    db0.ReadDwgFile(filename, Db.FileOpenMode.OpenForReadAndAllShare, false, "", false);

                    Db.HostApplicationServices.WorkingDatabase = db0;

                    log.Trace($"\tREAD");
                    //перекидываем в мультикад
                    //mcDocument = McDocumentsManager.GetDocument(filename);


                    //if (mcDocument == null)  //проверка на нулл, если нулл то пропуск и записать в лог, что файл пропущен
                    //{
                    //    _badFilePatchs.Add(filename);
                    //    logger.Log($"{count} NULL {filename}");
                    //    continue;
                    //}

                    /*▬
                    //если не нулл работаем его
                    _countFilesRead++;

                    of.Reset();//сброс фильтра иначе в цикле добавляет документы 
                    of.AddType(guid);
                    of.AddDoc(mcDocument);

                    //получаем с него ID
                    mcObjectIds = of.GetObjects();

                    _stwID.Stop();
                    _stwProp.Start();
                    _countTotal += mcObjectIds.Count;//всего получено

                    logger.Log($"\t\tprops");
                    //дергаем сбор свойств
                    ExtractNamedProps(mcObjectIds, ref markerProps);

                    _stwProp.Stop();
                    _stwID.Start();


                    logger.Log($"\t\told props {markerProps.Count}");
                    */

                }
                catch (Exception ex)
                {
                    _badFilePatchs.Add($"{filename} {ex.Message}");
                    log.Trace($"{count} ERR {filename}\n{ex.Message}");
                    continue;
                }
            }

            //_stwID.Stop();

            log.Trace($"{count} CLOSE {filename}");


            _countFilesRead++;

            //}
            //else
            //{
            //    logger.Log($"{count} ОТКРЫТ!!!! {filename}");
            //    _countFilesRead++;

            //    of.Reset();//сброс фильтра иначе в цикле добавляет документы 
            //    of.AddType(guid);
            //    of.AddDoc(mcDocument);

            //    //получаем с него ID
            //    mcObjectIds = of.GetObjects();

            //    //_stwID.Stop();
            //    //_stwProp.Start();
            //    _countTotal += mcObjectIds.Count;//всего получено

            //    logger.Log($"\t\tprops");
            //    //дергаем сбор свойств
            //    ExtractNamedProps(mcObjectIds, ref markerProps);

            //    //_stwProp.Stop();
            //    //_stwID.Start();


            //    logger.Log($"\t\told props");

            //    //после обработки закрываем
            //    if (mcDocument.IsHidden) mcDocument.Close();//если не открывали не закрывать

            //    logger.Log($"{count} CLOSE {filename}");
            //    logger.Log($"-----------");

            //    //_stwID.Stop();

            //}


        }

        _elapsedProp = _stwProp.Elapsed.ToString();
        _elapsedID = _stwID.Elapsed.ToString();
        _countAdded = markerProps.Count;//добавлено всего маркеров

        //McDocument.WorkingDocument = pOldWD;
        //Db.HostApplicationServices.WorkingDatabase = dbCarent;//возвращаем базу

        return markerProps;
    }


    /// <summary>
    /// Gets the props.Multicad
    /// </summary>
    /// <param name="filenames">список путей к файлам чертежей</param>
    /// <returns>Список свойств мультикад объектов</returns>
    public List<DefinitionMarkerProps> GetPropsMC(List<string> filenames)
    {

        log.Trace($"Total files: {filenames.Count}");

        List<DefinitionMarkerProps> markerProps = new List<DefinitionMarkerProps>();

        List<McObjectId> mcObjectIds = new List<McObjectId>();

        ObjectFilter of = new ObjectFilter(/*true*/);// новый фильтр долльше чем его сбросить

        _countFilesTotal = filenames.Count;

        //запомним рабочий документ на всякий
        McDocument pOldWD = McDocument.WorkingDocument;
        int count = 0;

        foreach (string filename in filenames)
        {
            _stwID.Start();

            count++;

            log.Trace($"{count} OPEN {filename}");
            //если открыт то не нулл
            McDocument mcDocument = McDocumentsManager.GetDocument(filename);
            if (mcDocument == null)
            {
                // открываем файл в скрытом режиме
                mcDocument = McDocumentsManager.OpenDocument(filename, false, true);
                if (mcDocument == null)  //проверка на нулл, если нулл то пропуск и записать в лог, что файл пропущен
                {
                    _badFilePatchs.Add(filename);
                    continue;
                }

            }

            _countFilesRead++;

            of.Reset();//сброс фильтра иначе в цикле добавляет документы 
            of.AddType(guid);
            of.AddDoc(mcDocument);

            //получаем с него ID
            mcObjectIds = of.GetObjects();

            _stwID.Stop();
            _stwProp.Start();
            _countTotal += mcObjectIds.Count;//всего получено

            log.Trace($"\t\tprops");
            //дергаем сбор свойств
            ExtractNamedProps(mcObjectIds, ref markerProps);

            _stwProp.Stop();
            _stwID.Start();


            log.Trace($"\t\told props");

            //после обработки закрываем
            if (mcDocument.IsHidden)
            {
                mcDocument.Close();//если не открывали не закрывать
            }

            log.Trace($"{count} CLOSE {filename}");
            log.Trace($"-----------");

            _stwID.Stop();
        }

        _elapsedProp = _stwProp.Elapsed.ToString();
        _elapsedID = _stwID.Elapsed.ToString();
        _countAdded = markerProps.Count;//добавлено всего маркеров

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
        List<DefinitionMarkerProps> markerProps = new List<DefinitionMarkerProps>();

        List<McObjectId> mcObjectIds = new List<McObjectId>();


        ObjectFilter of = new ObjectFilter(true);

        of.AddType(guid);

        _stwID.Start();

        //с открытых документов
        if (space == Space.All)
        {
            _countFilesTotal = _countFilesRead = McDocumentsManager.GetDocuments().Count;

            of.AddDocs(McDocumentsManager.GetDocuments());//открытые документы

            mcObjectIds = of.GetObjects();
        }
        //с активного документа
        else if (space == Space.Document)
        {
            _countFilesTotal = _countFilesRead = 1;

            of.AddDoc(McDocument.WorkingDocument);

            mcObjectIds = of.GetObjects();
        }
        //с активного пространства
        else if (space == Space.Layout)
        {
            _countFilesTotal = _countFilesRead = 1;

            mcObjectIds = of.GetObjects();
        }
        // выбором
        else if (space == Space.Select)
        {
            _countFilesTotal = _countFilesRead = 1;

            mcObjectIds = McObjectManager.SelectObjects("Выберите McUmarkers <Esc -- Cansel>", false, guid /*McUMarker.TypeID*/).ToList();
        }

        _stwID.Stop();
        _stwProp.Start();
        _countTotal = mcObjectIds.Count;//всего получено

        ExtractNamedProps(mcObjectIds, ref markerProps);

        _stwProp.Stop();

        _elapsedID = _stwID.Elapsed.ToString();
        _elapsedProp = _stwProp.Elapsed.ToString();

        _countAdded = markerProps.Count;//добавлено всего маркеров

        return markerProps;
    }
    private void ExtractNamedProps(List<McObjectId> mcObjectIds, ref List<DefinitionMarkerProps> markerProps)
    {
        /*
        tempUmark. DbEntity.Document тут IsModel, Name - имя листа, Path - путь документа
        DbEntity слой тип линий масштабы и прочее

        */

        foreach (McObjectId mcObjectId in mcObjectIds)//по собранным ID маркеров
        {

            DefinitionMarkerProps markerProp = new DefinitionMarkerProps();
            McUMarker? tempUmark = McObjectManager.GetObject(mcObjectId) as McUMarker;

            if (tempUmark == null)//не маркер
            {
                _countTotal = -1;//не маркер, не может такого быть но мало ли, на всякий случай
                continue;
            }

            //список свойств
            McProperties properties = McPropertySource.GetPropertySource(tempUmark).ObjectProperties;

            //имя маркера
            markerProp.MarkerName = properties.GetValueEx(_fieldName.Name, "").ToString()?.Trim();

            //имени нет или не то
            if (string.IsNullOrWhiteSpace(markerProp.MarkerName)
                || markerProp.MarkerName.IndexOf(_mcUmarkerName,
                                                 StringComparison.InvariantCultureIgnoreCase) < 0)
            {
                _countFalseName++;
                continue;
            }

            //флаг спецификации
            markerProp.FlagSpecRaw = properties.GetValueEx(_fieldName.FlagSpec, "").ToString()?.Trim();

            //учитывать признак спецификации
            if (_isSpec)
            {
                if (!markerProp.FlagSpec)//признака спец нет
                {
                    _countNotFlag++;
                    continue;
                }
            }

            //количество строка
            markerProp.AmountRaw = properties.GetValueEx(_fieldName.Amount, "").ToString()?.Trim();

            //наименование
            markerProp.DeviceName = properties.GetValueEx(_fieldName.DeviceName, "").ToString()?.Trim();

            //проверка на некорректные данные, если количество минус или наименование пустое, то не включать в набор
            if (markerProp.Amount < 0 || string.IsNullOrWhiteSpace(markerProp.DeviceName))
            {
                _countIncorrectData++;
                continue;
            }

            markerProp.Section = properties.GetValueEx(_fieldName.Section, "").ToString()?.Trim();
            markerProp.PositionNumber = properties.GetValueEx(_fieldName.PositionNumber, "").ToString()?.Trim();
            markerProp.TypeModel = properties.GetValueEx(_fieldName.TypeModel, "").ToString()?.Trim();
            markerProp.ArticleNumber = properties.GetValueEx(_fieldName.ArticleNumber, "").ToString()?.Trim();
            markerProp.Vendor = properties.GetValueEx(_fieldName.Vendor, "").ToString()?.Trim();
            markerProp.Unit = properties.GetValueEx(_fieldName.Unit, "").ToString()?.Trim();
            markerProp.UnitMass = properties.GetValueEx(_fieldName.UnitMass, "").ToString()?.Trim();
            markerProp.Comment = properties.GetValueEx(_fieldName.Comment, "").ToString()?.Trim();

            markerProps.Add(markerProp);
        }

    }

    /// <summary>
    ////таймер получения ID
    /// </summary>
    private Stopwatch _stwID { get; set; } = new Stopwatch();

    /// <summary>
    /// таймер получения свойств
    /// </summary>
    private Stopwatch _stwProp { get; set; } = new Stopwatch();

    /// <summary>
    /// общее время
    /// </summary>
    private Stopwatch _stwTotal { get; set; } = new Stopwatch();

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
    private FieldNameSettings _fieldName { get; set; }
    private string _mcUmarkerName { get; set; }

    /// <summary>
    /// ОБрабатывать маркеры с флагом включения в спеку
    /// </summary>
    /// <value>
    ///   <c>true</c> обрабатывать маркеры только с флагом включения в спеку; otherwise, <c>false</c>.
    /// </value>
    private bool _isSpec { get; set; }

    private Guid guid => _guid;
    private Guid _guid;


    #region Statistics


    public int CountIncorrectData => _countIncorrectData;//маркеры с отрицательной суммой, некорректными данными
    private int _countIncorrectData = 0;//маркеры с отрицательной суммой, некорректными данными

    public int CountNotFlag => _countNotFlag;//маркеров без признака включения в спеку
    private int _countNotFlag = 0;//маркеров без признака включения в спеку

    public int CountFalseName => _countFalseName;//маркеров с не тем именем
    private int _countFalseName = 0;//маркеров с не тем именем

    public int CountAdded => _countAdded;//маркеров добавлено
    private int _countAdded = 0;//маркеров добавлено

    public int CountTotal => _countTotal;//маркеров всего
    private int _countTotal = 0;//маркеров всего

    /// <summary>
    /// файлов всего обработано
    /// </summary>
    public int CountFilesTotal => _countFilesTotal;//
    private int _countFilesTotal = 0;//файлов всего

    /// <summary>
    /// успешно файлов прочитано
    /// </summary>
    public int CountFilesRead => _countFilesRead;// 
    private int _countFilesRead = 0;//файлов всего


    /// <summary>
    /// Gets the TMR identifier.
    /// </summary>
    /// <value>
    /// The TMR identifier.
    /// </value>
    public string ElapsedID => _elapsedID;
    private string _elapsedID = "";



    /// <summary>
    /// Gets the TMR property.
    /// </summary>
    /// <value>
    /// The TMR property.
    /// </value>
    public string ElapsedProp => _elapsedProp;
    private string _elapsedProp = ""; //todo подумать как закрыть изменение из других классов

    public List<string> BadFilePatchs => _badFilePatchs;
    private List<string> _badFilePatchs = new List<string>();


    /// <summary>
    /// Gets or sets a value indicating whether this instance is ok.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is ok; otherwise, <c>false</c>.
    /// </value>
    public bool IsOk { get; set; }

    #endregion

}
