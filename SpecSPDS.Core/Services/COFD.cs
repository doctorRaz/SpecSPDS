using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections;
using System.Collections.Generic;

namespace dRz.SpecSPDS.Core.Services
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

        public string Title
        {
            get => FSD.Title;
            set => FSD.Title = value;
        }

        public bool Multiselect
        {
            get => FSD.Multiselect;
            set => FSD.Multiselect = value;
        }

        public bool RestoreDirectory
        {
            get => FSD.RestoreDirectory;
            set => FSD.RestoreDirectory = value;
        }

        public string InitialDirectory
        {
            get => FSD.InitialDirectory;
            set => FSD.InitialDirectory = value;
        }

        public bool IsFolderPicker
        {
            get => FSD.IsFolderPicker;
            set => FSD.IsFolderPicker = value;
        }

        public string FileName
        {
            get => FSD.FileName;
        }

        public IEnumerable<string> FileNames
        {
            get => FSD.FileNames;
        }

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

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
