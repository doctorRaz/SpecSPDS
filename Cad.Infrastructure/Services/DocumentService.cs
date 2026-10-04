using dRz.Abstractions.Services;
using System;
using System.IO;

#if !TEST

using HostMgd.ApplicationServices;

#endif

namespace dRz.n.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {
        #region Public Properties

        public string FileName
        {
            get
            {
                return Path.GetFileName(FullPath);
            }
        }

        public string FileNameNoExtension
        {
            get
            {
                return Path.GetFileNameWithoutExtension(FullPath);
            }
        }

        public string FullPath
        {
            get
            {
                if (!IsActive)
                {
                    throw new NullReferenceException("Нет активного документа");
                }
#if !TEST
                Document doc = Application.DocumentManager.MdiActiveDocument;
                return doc.Name;
#else
                return "Doc Name TEST";
#endif
            }
        }

#if !TEST
        public bool IsActive
        {
            get
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                return doc != null;
            }
        }
#else

        /// <summary>The is active</summary>
        private bool _isActive;

        /// <summary>Активен ли документ
        /// меняем руками  через свойства
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }

#endif

        #endregion Public Properties
    }
}