/*

У меня не срабатывал, насколько я помню, механизм нескольких определений CommandMethod. Может, потому, что было под АКАД ))

AppSettings я б засунул именно в то, что работает под кадом.

Т.е., если надо, в методы/классы подсовывать не AppSettings.Settings, а именно ApplicationSettings. По крайней мере подменить можно будет при необходимости тестирования

получаю список  свойств маркеров
Ну ты же не напрямую McObject бросаешь, я правильно понимаю? А собственный класс, который маскирует все это дело? А то и абстрактный класс.
 пульну на сборку в таблицу
Отличный вариант написать отдельный тест под это дело ))) Юзер выбрал - вызываем реализацию интефейса типа IConvertMarker, который возвращает коллекцию экземпляров твоего класса.
Эту коллекцию - в метод типа PrepareDataForTable (который может вернуть чуть ли не полный вариант форматирования таблицы, но без привязки к каду).
Результат метода - в чисто кадовский метод CreateMCadTable по твоим же правилам.

Я бы сегодня, наверное, делал так. Сорян, в код не полезу - помимо резюме еще и работы понакидали (((

Насколько это лучше - пока не представляю. Но я в любом случае по максимуму бы отделял то, что без када жить не может, от чисто данных. По крайней мере в сегодняшних реалиях

*/

using drz.SpecSPDS.Core.Settings;
using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Extensions;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Services.DeepSeek;
using dRz.SpecSPDS.Core.Settings;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace dRz.SpecSpdsConsole
{

    public class AllProgs
    {
        static string dat => DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.FFFFF",
                            CultureInfo.InvariantCulture);


        [STAThread]
        static void Main(string[] args)
        {
            var fff=Assembly.GetExecutingAssembly().GetName().Name;

        Lb:
            //https://chatgpt.com/c/69693f7a-fe20-832b-a81b-3dacf5775ffd
            AppPaths.Ensure();

            LogManager.Configuration.Variables["logDir"] = AppPaths.LogDir;
            LogManager.Setup().LoadConfigurationFromFile("nlog.config");

            var log = LogManager.GetCurrentClassLogger();
            log.Info("Addon initialized");

            AppSettings appSettings = new AppSettings();

            appSettings.Save();


            goto Lb;



            #region Чтение запись настроек XML

            PropXml propXml = new PropXml();
            propXml.LoadProps();

            List<DefinitionMarkerProps> props = propXml.Props;

            PropXml propXml2 = new PropXml();
            propXml2.LoadProps();

            List<DefinitionMarkerProps> props2 = propXml2.Props;

            List<DefinitionMarkerProps> propsAll = props.Union(props2).ToList();

            List<GroupedDefinitionMarkerProps> groupedDefinitionMarkerProps = DefinitionMarkerPropsGrouper.GroupAndSortDefinitionMarkers(props);

            var outConsole = new OutConsole(groupedDefinitionMarkerProps);

            GroupAmount groupAmount = new GroupAmount(props);

            #endregion


            #region тест логгера

            TestLogCuctom tst = new TestLogCuctom();

            tst.Test();

            #endregion

            #region получение  файлов из каталогов

            List<string> ff1 = FetchingPatchFiles.GetFiles(Space.SubFolder);

            List<string> ff = FetchingPatchFiles.GetFiles(Space.Folder);

            #endregion

            #region тест вывода результата в консоль

            new OutResult();

            #endregion

            #region Окончания слова

            List<string> list = new List<string> { "Маркер", "Маркера", "Маркеров" };

            for (int i = 0; i < 35; ++i)
            {
                Console.WriteLine($"{i} {list.Declens(i)}");

            }

            #endregion

            #region Сборный класс типа настройки

            TestClass newClass = new TestClass();
            newClass.Stats._tmrID = "10";

            #endregion

            #region Передача по ссылке

            OutAddList oal = new OutAddList();

            string[] str = { "1", "2", "10" };

            List<string> lstr = new List<string> { "zz", "fd", "sd" };

            oal.RefAdd(str, ref lstr);

            List<string> dd = lstr;

            #endregion












            Console.ReadKey();
        }
    }
}
