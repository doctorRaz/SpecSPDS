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

using drz.Abstractions.Infrastructure;
using drz.Infrastructure.Infrastructure;
using drz.SpecSpds.Test.SimpleInjector;
using System;
using System.Diagnostics;


namespace drz.SpecSpds.Test
{
    public class Start
    {
        #region Private Methods

        [STAThread]
        private static void Main(string[] args)
        {
            Stopwatch swTotal = Stopwatch.StartNew();
                Stopwatch sw = Stopwatch.StartNew();

          


            ICadInfo cadInfo1 = new CadInfo_NEW();

            Console.WriteLine(cadInfo1.ToString()); 
            Console.WriteLine(cadInfo1.ToShortString()); 
            Console.WriteLine(cadInfo1.ToLongString()); 


            for (int i = 0; i < 1; i++)
            {
                 swTotal.Restart();
                 sw.Restart();

             

                /*
                 *00:00:00.0024684 SysInfo_NEW2
                 *00:00:00.1066559 SysInfo_NEW.ToLongString
                 */
                ISysInfo sysInfo_NEW = new SysInfo_NEW();
                Console.WriteLine($"{sw.Elapsed} SysInfo_NEW2");
                Console.WriteLine($"{sysInfo_NEW.GpuInfo}");
                Console.WriteLine($"{sysInfo_NEW.ProcessorName}");
                Console.WriteLine($"{sysInfo_NEW.RamTotalGb}");
                Console.WriteLine($"{sysInfo_NEW.ToLongString()}");

                sw.Restart();

                //var ss = (sysInfo_NEW.ToLongString());
                //Console.WriteLine($"{sw.Elapsed} SysInfo_NEW.ToLongString");
                //sw.Restart();

                IApplicationInfo_NEW  applicationInfo = new ApplicationInfo_NEW(typeof(Start).Assembly);
                Console.WriteLine($"{sw.Elapsed} ApplicationInfo");
                sw.Restart();

                IApplicationInfo_NEW applicationInfoNew = new ApplicationInfo_NEW(typeof(Start).Assembly);
                Console.WriteLine($"{sw.Elapsed} ApplicationInfo_NEW");
                sw.Restart();


                //var ap=applicationInfoNew.ToLongString();
                //Console.WriteLine($"{sw.Elapsed} applicationInfoNew.ToLongString();");
                //sw.Restart();

                ICadInfo cadInfo = new CadInfo_NEW();
                Console.WriteLine($"{sw.Elapsed} CadInfo_NEW");
                sw.Restart();

                TestContainer testContainer = new TestContainer();
                Console.WriteLine($"{sw.Elapsed} new TestContainer();");

                Console.WriteLine($"{swTotal.Elapsed} total");
                Console.WriteLine($"\n--====--");
               
            }
            //Console.ReadKey();

            /* //SimpleInjector */
            DrzSimpleInjector drzSimpleInjector = new DrzSimpleInjector();
            drzSimpleInjector.Start();

            /*//логер
            DrzLogger drzLogger = new DrzLogger();
            drzLogger.Start();
            */

            sw.Stop();
            Console.WriteLine($"Total time: {sw.Elapsed}");

            Console.WriteLine("-=End=-");

            //Thread.Sleep(new TimeSpan(0, 0, 10));
            Console.ReadKey();
        }

        #endregion Private Methods
    }


}