using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace dRz.Application.Services
{
    /// <summary>
    /// Обертка над Microsoft.WindowsAPICodePack.Dialogs
    /// <br>для отлова эксепшн в WPF</br>
    /// </summary>
    public class COFD : IEnumerable
    {
        /// <summary> CommonOpenFileDialog конструктор </summary>
        public COFD()
        {
            FSD = new CommonOpenFileDialog();
        }

        /// <summary>CommonOpenFileDialog</summary>
        public CommonOpenFileDialog FSD { get; set; }

        /// <summary>Заголовок диалогового окна.</summary>
        public string Title
        {
            get => FSD.Title;
            set => FSD.Title = value;
        }

        /// <summary>Определяет, разрешён ли множественный выбор.</summary>
        public bool Multiselect
        {
            get => FSD.Multiselect;
            set => FSD.Multiselect = value;
        }

        /// <summary>Определяет, восстанавливается ли текущий каталог после закрытия диалога.</summary>
        public bool RestoreDirectory
        {
            get => FSD.RestoreDirectory;
            set => FSD.RestoreDirectory = value;
        }

        /// <summary>Начальный каталог диалога.</summary>
        public string InitialDirectory
        {
            get => FSD.InitialDirectory;
            set => FSD.InitialDirectory = value;
        }

        /// <summary>Определяет, используется ли диалог для выбора папки.</summary>
        public bool IsFolderPicker
        {
            get => FSD.IsFolderPicker;
            set => FSD.IsFolderPicker = value;
        }

        /// <summary>Имя выбранного файла.</summary>
        public string FileName
        {
            get => FSD.FileName;
        }

        /// <summary>Имена выбранных файлов.</summary>
        public IEnumerable<string> FileNames
        {
            get => FSD.FileNames;
        }

        /// <summary>Показывает диалог и возвращает результат выбора.</summary>
        /// <returns><see langword="true"/>, если пользователь подтвердил выбор; иначе <see langword="false"/>.</returns>
        public bool ShowDialog()
        {
            if (FSD.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Показывает диалог с указанным владельцем окна.</summary>
        /// <param name="wpfHandle">Дескриптор окна-владельца.</param>
        /// <returns><see langword="true"/>, если пользователь подтвердил выбор; иначе <see langword="false"/>.</returns>
        public bool ShowDialog(IntPtr wpfHandle)
        {
            if (FSD.ShowDialog(wpfHandle) == CommonFileDialogResult.Ok)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>Получает перечислитель для объекта.</summary>
        /// <returns>Перечислитель; реализация в текущем классе не поддерживается.</returns>
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}