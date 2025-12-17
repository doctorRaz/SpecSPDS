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

using dRz.SpecSPDS.Core.Enums;
using dRz.SpecSPDS.Core.Extensions;
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Services.DeepSeek;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace dRz.SpecSpdsConsole
{

    public class Program
    {
        private static readonly string _logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                                  "logtest.txt");
    static     string   dat => DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.FFFFF",
                        CultureInfo.InvariantCulture);


        [STAThread]
        static void Main(string[] args)
        {
            Stopwatch stw = new Stopwatch();
        Lb:
            Log log = new Log(_logPath);

            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                                                  "logtest100.txt");
            log._path=logPath;

            stw.Start();
            for (int i = 1; i <= 1000000; ++i)
            {

                log.LogWrite($" - {dat} {i} тестовый тест");
            }

            stw.Stop();
            log.LogWrite($"********THE END************");
            log.LogWrite($"Total Time {stw.Elapsed.ToString()}");

            Console.WriteLine($"*********** THE END {stw.Elapsed.ToString()} *****************");
            { }
            goto Lb;


            var ff1 = FetchingPatchFiles.GetFiles(Space.SubFolder);
            new OutResult();

            List<string> list = new List<string> { "Маркер", "Маркера", "Маркеров" };

            for (int i = 0; i < 35; ++i)
            {
                Console.WriteLine($"{i} {list.Declens(i)}");

            }


            NewClass newClass = new NewClass();

            //newClass.Stats._tmrID = "10";




            var oal = new OutAddList();

            string[] str = { "1", "2", "10" };

            List<string> lstr = new List<string> { "zz", "fd", "sd" };

            oal.RefAdd(str, ref lstr);

            var dd = lstr;

            var ff = FetchingPatchFiles.GetFiles(Space.Folder);
            PropXml propXml = new PropXml();
            propXml.LoadProps();

            List<DefinitionMarkerProps> props = propXml.Props;

            PropXml propXml2 = new PropXml();
            propXml2.LoadProps();

            List<DefinitionMarkerProps> props2 = propXml2.Props;


            List<DefinitionMarkerProps> propsAll = props.Union(props2).ToList();

            List<GroupedDefinitionMarkerProps> groupedDefinitionMarkerProps = DefinitionMarkerPropsGrouper.GroupAndSortDefinitionMarkers(props);

            var outConsole = new OutConsole(groupedDefinitionMarkerProps);


            Console.WriteLine("****************************");
            var groupAmount = new GroupAmount(props);







            Console.ReadKey();
        }
    }
}
