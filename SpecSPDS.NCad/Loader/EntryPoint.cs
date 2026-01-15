/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для 
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */
using System.Reflection;
using dRz.CAD.Loader;


#if AC

using cad = Autodesk.AutoCAD.ApplicationServices.Application;
using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using HostMgd.ApplicationServices;
using Teigha.DatabaseServices;
using Teigha.Geometry;
using Rtm = Teigha.Runtime;
using cad = HostMgd.ApplicationServices.Application;

#endif

[assembly: Rtm.ExtensionApplication(typeof(EntryPoint))]

namespace dRz.CAD.Loader
{
    /// <summary>
    /// Задачей данного класса является поиск и загрузка в AutoCAD наиболее 
    /// подходящей для него версии плагина.
    /// </summary>
    public sealed class EntryPoint : Rtm.IExtensionApplication
    {
        const string netPluginExtension = ".dll";
        static readonly string[] extensions = new string[] { ".arx", ".dvb" };
        static readonly string[] methodNames = new string[] { "LoadArx", "LoadDVB"
    };

        /// <summary>
        /// Код этого метода будет запущен на исполнение при загрузке сборки в 
        /// AutoCAD. В результате его работы происходит попытка найти и загрузить в
        /// AutoCAD наиболее подходящую версию плагина из имеющихся в наличии.
        /// </summary>
        public void Initialize()
        {
            // Для начала извлекаем информацию о текущей версии AutoCAD и ищем
            // соответствующую ей версию файла. Имя такого файла должно 
            // формироваться по правилу: 
            //    ИмяТекущейСборки.Major.Minor[x86|x64].(dll|arx|dvb).
            // Где <Major> и <Minor> - это значения одноимённых свойств объекта 
            // Version, полученного из Application.Version.
            Version version = cad.Version;

            string fileFullName = GetType().Assembly.Location;

            Version minVersion = new Version(23, 0);

            FileInfo targetDllFullName = FindFile(fileFullName, version, minVersion);

            if (targetDllFullName == null)
                return;

            // Если найден файл, соответствующий нашей версии AutoCAD, то 
            // загружаем его.
            Assembly? asm = null;
            try
            {
                if (targetDllFullName.Extension.Equals(netPluginExtension,
                  StringComparison.CurrentCultureIgnoreCase))
                    asm = Assembly.LoadFrom(targetDllFullName.FullName);
                else
                {
                    int index = Array.IndexOf(extensions, targetDllFullName.Extension);

                    if (index >= 0)
                    {
                        object application = cad.AcadApplication;

                        application.GetType().InvokeMember(methodNames[index], BindingFlags
                          .InvokeMethod, null, application, new object[] {
                targetDllFullName.FullName });
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// Получить имя наиболее подходящего файла, для его последующей загрузки в
        /// AutoCAD. Если такой файл не будет найден, то возвращается null.
        /// </summary>
        /// <param name="fileFullName">"Базовое" имя файла, т.е. полное имя 
        /// файла без указания в нём версий ядра и разрядности платформы.</param>
        /// <param name="expectedVersion">Версия AutoCAD, для которой следует 
        /// выполнить поиск соответствующей версии файла.</param>
        /// <param name="minVersion">Наименьшая версия AutoCAD, ниже которой не 
        /// следует выполнять поиск.</param>
        /// <returns>Возвращается FileInfo наиболее подходящего файла, для его 
        /// последующей загрузки в AutoCAD. Если такой файл не будет найден, то 
        /// возвращается null.</returns>
        private FileInfo FindFile(string fileFullName, Version expectedVersion,
          Version minVersion)
        {

            if (fileFullName == null)
                throw new ArgumentNullException("fileFullName");

            if (fileFullName.Trim() == string.Empty)
                throw new ArgumentException(
                  "fileFullName.Trim() == String.Empty");

            if (expectedVersion < minVersion)
                throw new ArgumentException(
                  "expectedVersion < minVersion");

            int major = expectedVersion.Major;
            //if (major != 26)
            //{
            //    major = 23;
            //}
            int minor = expectedVersion.Minor;

            string? directory = Path.GetDirectoryName(fileFullName);
            string fileName = Path.GetFileNameWithoutExtension(fileFullName);

            String coreString = String.Format("{0}.{1}", major.ToString(),
              minor.ToString());
            //string coreString = string.Format("{0}", major.ToString());

            string subDirectoryName = "R" + coreString;
            string subDirectoryName_xPlatform = subDirectoryName + (IntPtr.Size == 4
              ? "x86" : "x64");

            string targetFileName = string.Empty;
            string targetFileName_xPlatform = string.Empty;
            string targetFileFullName = string.Empty;
            string targetFileFullName_xPlatform = string.Empty;

            List<string> items = new List<string>(extensions);
            items.Insert(0, netPluginExtension);

            string name = string.Empty;

            foreach (string extension in items)
            {

                targetFileName = string.Format("{0}.{1}{2}", fileName, coreString,
                  extension);
                targetFileName_xPlatform = string.Format("{0}.{1}{2}{3}", fileName,
                  coreString, IntPtr.Size == 4 ? "x86" : "x64", extension);

                // Сначала выполняем поиск в текущем каталоге
                targetFileFullName = Path.Combine(directory, targetFileName);
                if (File.Exists(targetFileFullName))
                {
                    name = targetFileFullName;
                    break;
                }
                targetFileFullName_xPlatform = Path.Combine(directory,
                  targetFileName_xPlatform);
                if (File.Exists(targetFileFullName_xPlatform))
                {
                    name = targetFileFullName_xPlatform;
                    break;
                }

                // Если в текущем каталоге подходящий файл не найден, то продолжаем
                // поиск по соответствующим подкаталогам
                targetFileFullName = directory + "\\" + subDirectoryName +
                  "\\" + targetFileName;
                if (File.Exists(targetFileFullName))
                {
                    name = targetFileFullName;
                    break;
                }

                targetFileFullName_xPlatform = directory + "\\" +
                  subDirectoryName_xPlatform + "\\" + targetFileName_xPlatform;
                if (File.Exists(targetFileFullName_xPlatform))
                {
                    name = targetFileFullName_xPlatform;
                    break;
                }
            }

            // Если найден файл, соответствующий нашей версии AutoCAD, то возвращаем 
            // соответствующий ему объект FileInfo.
            if (File.Exists(name))
            {
                return new FileInfo(name);
            }
            // Если соответствия не найдено, то продолжаем поиск, последовательно 
            // проверяя наличие подходящего файла для более ранних версий AutoCAD
            else
            {
                if (minor == 0)
                {
                    minor = 3;
                    --major;
                }
                else
                {
                    --minor;
                }

                Version version = new Version(major, minor);
                if (version < minVersion)
                    return null;
                FileInfo file = FindFile(fileFullName, new Version(major, minor),
                  minVersion);
                return file;
            }
        }

        /// <summary>
        /// Код данного метода выполняется при завершении работы AutoCAD.
        /// </summary>
        public void Terminate()
        {
        }

#if DEBUG
        //***************

        // Создаём четыре видовых экрана модели,
        // устанавливаем в каждом различные ортографические виды
        // и показываем до границ.
        [Rtm.CommandMethod("SplitMVP")]
        public static void SplitAndSetViewModelViewports()
        {
            Document doc
                = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;

            db.UpdateExt(true);
            Extents3d dbExtent
                = new Extents3d(db.Extmin, db.Extmax);

            using (Transaction tr
                = db.TransactionManager.StartTransaction())
            {
                ViewportTable vt = tr.GetObject(
                    db.ViewportTableId, OpenMode.ForWrite)
                    as ViewportTable;

                ViewportTableRecord vtr1 = tr.GetObject(
                    doc.Editor.ActiveViewportId,
                    OpenMode.ForWrite) as ViewportTableRecord;

                Point2d ll = vtr1.LowerLeftCorner;
                Point2d ur = vtr1.UpperRightCorner;

                vtr1.LowerLeftCorner = ll;
                vtr1.UpperRightCorner = new Point2d(
                    ll.X + (ur.X - ll.X) * 0.5,
                    ll.Y + (ur.Y - ll.Y) * 0.5);
                vtr1.SetViewDirection(OrthographicView.LeftView);//todo тут 
                ZoomExtents(vtr1, dbExtent);

                ViewportTableRecord vtr2 =
                    CreateVTR(vt, vtr1,
                    new Point2d(ll.X + (ur.X - ll.X) * 0.5, ll.Y),
                    new Point2d(ur.X, ll.Y + (ur.Y - ll.Y) * 0.5),
                    dbExtent, OrthographicView.RightView);
                vt.Add(vtr2);
                tr.AddNewlyCreatedDBObject(vtr2, true);

                ViewportTableRecord vtr3 =
                    CreateVTR(vt, vtr1,
                    vtr1.UpperRightCorner, ur,
                    dbExtent, OrthographicView.BottomView);
                vt.Add(vtr3);
                tr.AddNewlyCreatedDBObject(vtr3, true);

                ViewportTableRecord vtr4 =
                    CreateVTR(vt, vtr1,
                    new Point2d(ll.X, ll.Y + (ur.Y - ll.Y) * 0.5),
                    new Point2d(ll.X + (ur.X - ll.X) * 0.5, ur.Y),
                    dbExtent, OrthographicView.TopView);
                vt.Add(vtr4);
                tr.AddNewlyCreatedDBObject(vtr4, true);

                // Обновляем дисплей с новыми неперекрывающимися видовыми экранами
                doc.Editor.UpdateTiledViewportsFromDatabase();

                // Применяем изменения
                tr.Commit();
            }
        }

        // Создаём видовой экран модели, используя параметры исходного видового
        // экрана перед установкой ортографического вида и показа до границ
        public static ViewportTableRecord CreateVTR(
            ViewportTable vt, ViewportTableRecord refVTR,
            Point2d ll, Point2d ur, Extents3d dbExtent,
            OrthographicView ov)
        {
            ViewportTableRecord newVTR = new ViewportTableRecord();

            newVTR.LowerLeftCorner = ll;
            newVTR.UpperRightCorner = ur;
            newVTR.Name = "*Active";

            newVTR.ViewDirection = refVTR.ViewDirection;
            newVTR.ViewTwist = refVTR.ViewTwist;
            newVTR.Target = refVTR.Target;
            newVTR.BackClipEnabled = refVTR.BackClipEnabled;
            newVTR.BackClipDistance = refVTR.BackClipDistance;
            newVTR.FrontClipEnabled = refVTR.FrontClipEnabled;
            newVTR.FrontClipDistance = refVTR.FrontClipDistance;
            newVTR.Elevation = refVTR.Elevation;
            newVTR.SetViewDirection(ov);

            ZoomExtents(newVTR, dbExtent);

            return newVTR;
        }

        public static void ZoomExtents
            (ViewportTableRecord vtr, Extents3d dbExtent)
        {
            // Получаем пропорции видового экрана
            // вычисляем высоту и ширину
            double scrRatio = (vtr.Width / vtr.Height);

            // Готовим матрицу преобразования ДСК (DCS) в МСК (WCS)
            Matrix3d matWCS2DCS
                = Matrix3d.PlaneToWorld(vtr.ViewDirection);

            // Для ДСК целевая точка – начало координат
            matWCS2DCS = Matrix3d.Displacement
                (vtr.Target - Point3d.Origin) * matWCS2DCS;

            // Поворачиваем ось X МСК на угол поворота
            matWCS2DCS = Matrix3d.Rotation(-vtr.ViewTwist,
                                            vtr.ViewDirection,
                                            vtr.Target
                                        ) * matWCS2DCS;

            matWCS2DCS = matWCS2DCS.Inverse();

            // Преобразуем границы в ДСК  
            // определенной по viewdir
            dbExtent.TransformBy(matWCS2DCS);

            // ширина границ в текущем виде
            double width
             = (dbExtent.MaxPoint.X - dbExtent.MinPoint.X);

            // высота границ в текущем виде
            double height
             = (dbExtent.MaxPoint.Y - dbExtent.MinPoint.Y);

            // получаем центр вида
            Point2d center = new Point2d(
             (dbExtent.MaxPoint.X + dbExtent.MinPoint.X) * 0.5,
                   (dbExtent.MaxPoint.Y + dbExtent.MinPoint.Y) * 0.5);

            // проверяем больше ли ширина, чем у текущего окна
            // если нет, тогда получаем новую высоту согласно пропорциям
            // видовых экранов
            if (width > (height * scrRatio))
                height = width / scrRatio;

            vtr.Height = height;
            vtr.Width = height * scrRatio;
            vtr.CenterPoint = center;
        }
#endif
    }
}