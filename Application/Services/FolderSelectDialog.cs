// Source - https://stackoverflow.com/a
// Posted by IRJSK
// Retrieved 2025-12-15, License - CC BY-SA 3.0

using System;
using System.Reflection;
using System.Windows.Forms;

namespace dRz.Application.Services
{
    /// <summary>Предоставляет диалог выбора папки на основе стандартного диалога выбора файла.</summary>
    public class FolderSelectDialog
    {
        private OpenFileDialog ofd = null;

        /// <summary>Создаёт диалог выбора папки.</summary>
        public FolderSelectDialog()
        {
            ofd = new OpenFileDialog();
            ofd.Filter = "Folders|\n";
            ofd.AddExtension = false;
            ofd.CheckFileExists = false;
            ofd.DereferenceLinks = true;
            ofd.Multiselect = Multiselect;
        }

        /// <summary>Начальный каталог диалога.</summary>
        public string InitialDirectory
        {
            get { return ofd.InitialDirectory; }
            set { ofd.InitialDirectory = value == null || value.Length == 0 ? Environment.CurrentDirectory : value; }
        }

        /// <summary>Заголовок диалога.</summary>
        public string Title
        {
            get { return ofd.Title; }
            set { ofd.Title = value == null ? "Select a folder" : value; }
        }

        /// <summary>Определяет, разрешён ли выбор нескольких папок.</summary>
        public bool Multiselect
        {
            get { return ofd.Multiselect; }
            set { ofd.Multiselect = value == false ? false : value; }
        }

        /// <summary>Путь к выбранной папке.</summary>
        public string FileName
        {
            get { return ofd.FileName; }
        }

        /// <summary>Пути к выбранным папкам.</summary>
        public string[] FileNames
        {
            get { return ofd.FileNames; }
        }

        /// <summary>Показывает диалог выбора папки.</summary>
        /// <returns><see langword="true"/>, если выбор подтверждён.</returns>
        public bool ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }

        /// <summary>Показывает диалог выбора папки с указанным владельцем.</summary>
        /// <param name="hWndOwner">Дескриптор окна-владельца.</param>
        /// <returns><see langword="true"/>, если выбор подтверждён.</returns>
        public bool ShowDialog(IntPtr hWndOwner)
        {
            bool flag = false;

            if (Environment.OSVersion.Version.Major >= 6)
            {
                Reflector r = new Reflector("System.Windows.Forms");
                uint num = 0;
                Type typeIFileDialog = r.GetType("FileDialogNative.IFileDialog");
                object dialog = r.Call(ofd, "CreateVistaDialog");
                r.Call(ofd, "OnBeforeVistaDialog", dialog);
                uint options = (uint)r.CallAs(typeof(FileDialog), ofd, "GetOptions");
                options |= (uint)r.GetEnum("FileDialogNative.FOS", "FOS_PICKFOLDERS");
                r.CallAs(typeIFileDialog, dialog, "SetOptions", options);
                object pfde = r.New("FileDialog.VistaDialogEvents", ofd);
                object[] parameters = new object[] { pfde, num };
                r.CallAs2(typeIFileDialog, dialog, "Advise", parameters);
                num = (uint)parameters[1];
                try
                {
                    int num2 = (int)r.CallAs(typeIFileDialog, dialog, "Show", hWndOwner);
                    flag = 0 == num2;
                }
                finally
                {
                    r.CallAs(typeIFileDialog, dialog, "Unadvise", num);
                    GC.KeepAlive(pfde);
                }
            }
            else
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.Description = Title;
                fbd.SelectedPath = InitialDirectory;
                fbd.ShowNewFolderButton = false;
                if (fbd.ShowDialog(new WindowWrapper(hWndOwner)) != DialogResult.OK)
                {
                    return false;
                }

                ofd.FileName = fbd.SelectedPath;
                flag = true;
            }
            return flag;
        }

        /// <summary>Обёртка дескриптора окна для интерфейса <see cref="IWin32Window"/>.</summary>
        public class WindowWrapper : IWin32Window
        {
            /// <summary>Создаёт обёртку окна.</summary>
            /// <param name="handle">Дескриптор окна.</param>
            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }

            /// <summary>Дескриптор окна.</summary>
            public IntPtr Handle
            {
                get { return _hwnd; }
            }

            private IntPtr _hwnd;
        }

        /// <summary>Выполняет вызовы членов типов через рефлексию.</summary>
        public class Reflector
        {
            private string m_ns;
            private Assembly m_asmb;

            /// <summary>Создаёт рефлектор для указанного пространства имён и сборки.</summary>
            /// <param name="ns">Пространство имён.</param>
            public Reflector(string ns)
                : this(ns, ns)
            { }

            /// <summary>Создаёт рефлектор для указанной сборки и пространства имён.</summary>
            /// <param name="an">Начало имени сборки для поиска.</param>
            /// <param name="ns">Пространство имён.</param>
            public Reflector(string an, string ns)
            {
                m_ns = ns;
                m_asmb = null;
                foreach (AssemblyName aN in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
                {
                    if (aN.FullName.StartsWith(an))
                    {
                        m_asmb = Assembly.Load(aN);
                        break;
                    }
                }
            }

            /// <summary>Находит тип по имени внутри заданного пространства имён.</summary>
            /// <param name="typeName">Имя типа, включая вложенные типы.</param>
            /// <returns>Найденный тип.</returns>
            public Type GetType(string typeName)
            {
                Type type = null;
                string[] names = typeName.Split('.');

                if (names.Length > 0)
                {
                    type = m_asmb.GetType(m_ns + "." + names[0]);
                }

                for (int i = 1; i < names.Length; ++i)
                {
                    type = type.GetNestedType(names[i], BindingFlags.NonPublic);
                }
                return type;
            }

            /// <summary>Создаёт экземпляр типа, перебирая доступные конструкторы.</summary>
            /// <param name="name">Имя типа.</param>
            /// <param name="parameters">Аргументы конструктора.</param>
            /// <returns>Созданный объект или <see langword="null"/>, если создать его не удалось.</returns>
            public object New(string name, params object[] parameters)
            {
                Type type = GetType(name);
                ConstructorInfo[] ctorInfos = type.GetConstructors();
                foreach (ConstructorInfo ci in ctorInfos)
                {
                    try
                    {
                        return ci.Invoke(parameters);
                    }
                    catch { }
                }

                return null;
            }

            /// <summary>Вызывает метод объекта по имени.</summary>
            /// <param name="obj">Объект, на котором вызывается метод.</param>
            /// <param name="func">Имя метода.</param>
            /// <param name="parameters">Аргументы метода.</param>
            /// <returns>Результат вызова метода.</returns>
            public object Call(object obj, string func, params object[] parameters)
            {
                return Call2(obj, func, parameters);
            }

            /// <summary>Вызывает метод объекта с массивом аргументов.</summary>
            /// <param name="obj">Объект, на котором вызывается метод.</param>
            /// <param name="func">Имя метода.</param>
            /// <param name="parameters">Аргументы метода.</param>
            /// <returns>Результат вызова метода.</returns>
            public object Call2(object obj, string func, object[] parameters)
            {
                return CallAs2(obj.GetType(), obj, func, parameters);
            }

            /// <summary>Вызывает метод, найденный в указанном типе.</summary>
            /// <param name="type">Тип, в котором выполняется поиск метода.</param>
            /// <param name="obj">Объект-владелец метода.</param>
            /// <param name="func">Имя метода.</param>
            /// <param name="parameters">Аргументы метода.</param>
            /// <returns>Результат вызова метода.</returns>
            public object CallAs(Type type, object obj, string func, params object[] parameters)
            {
                return CallAs2(type, obj, func, parameters);
            }

            /// <summary>Вызывает метод указанного типа с массивом аргументов.</summary>
            /// <param name="type">Тип, в котором выполняется поиск метода.</param>
            /// <param name="obj">Объект-владелец метода.</param>
            /// <param name="func">Имя метода.</param>
            /// <param name="parameters">Аргументы метода.</param>
            /// <returns>Результат вызова метода.</returns>
            public object CallAs2(Type type, object obj, string func, object[] parameters)
            {
                MethodInfo methInfo = type.GetMethod(func, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return methInfo.Invoke(obj, parameters);
            }

            /// <summary>Получает значение свойства объекта.</summary>
            /// <param name="obj">Объект, у которого читается свойство.</param>
            /// <param name="prop">Имя свойства.</param>
            /// <returns>Значение свойства.</returns>
            public object Get(object obj, string prop)
            {
                return GetAs(obj.GetType(), obj, prop);
            }

            /// <summary>Получает значение свойства, найденного в указанном типе.</summary>
            /// <param name="type">Тип, в котором выполняется поиск свойства.</param>
            /// <param name="obj">Объект-владелец свойства.</param>
            /// <param name="prop">Имя свойства.</param>
            /// <returns>Значение свойства.</returns>
            public object GetAs(Type type, object obj, string prop)
            {
                PropertyInfo propInfo = type.GetProperty(prop, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return propInfo.GetValue(obj, null);
            }

            /// <summary>Получает значение поля перечисления по имени типа и поля.</summary>
            /// <param name="typeName">Имя типа перечисления.</param>
            /// <param name="name">Имя поля.</param>
            /// <returns>Значение поля.</returns>
            public object GetEnum(string typeName, string name)
            {
                Type type = GetType(typeName);
                FieldInfo fieldInfo = type.GetField(name);
                return fieldInfo.GetValue(null);
            }
        }
    }

    /// <summary>Форма вспомогательного выбора папок.</summary>
    public partial class Form1 : Form
    {
        /// <summary>Создаёт форму.</summary>
        public Form1()
        {
            //InitializeComponent();
        }

        /// <summary>Открывает диалог выбора папок и возвращает выбранные пути.</summary>
        /// <param name="multiselect">Разрешает выбор нескольких папок.</param>
        /// <returns>Пути к выбранным папкам, разделённые запятыми, либо путь к одной папке.</returns>
        public static string OpenFileBrowserDialog(bool multiselect)
        {
            FolderSelectDialog fbd = new FolderSelectDialog();
            fbd.Multiselect = multiselect;

            fbd.ShowDialog();

            string selected_folders = "";
            if (multiselect)
            {
                string[] names = fbd.FileNames;
                selected_folders = string.Join(",", names);
            }
            else
            {
                selected_folders = fbd.FileName;
            }

            return selected_folders;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string name = OpenFileBrowserDialog(true);

            MessageBox.Show(name);
            Environment.Exit(0);
        }
    }
}