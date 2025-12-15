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
using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Services;
using dRz.SpecSPDS.Core.Services.DeepSeek;
using dRz.SpecSPDS.NCad.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace dRz.SpecSpdsConsole
{

    public class Program
    {

        [STAThread]
        static void Main(string[] args)
        {

           CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.IsFolderPicker = true;
            cofd.Multiselect = true;
            cofd.InitialDirectory = "";
            cofd.RestoreDirectory = true;
      

            var cc = cofd.ShowDialog();

            
            List<string> fn = cofd.FileNames.ToList<string>();//.ToList<string>();

              cc = cofd.ShowDialog();

             List<string> fn2 = cofd.FileNames.ToList<string>();

            //https://metanit.com/sharp/tutorial/15.4.php
            List<string> d =fn.Union(fn2).ToList<string>();

                 d.Sort ();
        Lb:
            var ff = FetchingPatchFiles.GetFiles(Space.Files);

            goto Lb;

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
