using drz.Abstractions.Services;
using System;
using System.IO;

#if !TEST

using HostMgd.ApplicationServices;

#endif

namespace drz.CadServices.Services
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

        public bool IsActive
        {
            get
            {
#if !TEST
                Document doc = Application.DocumentManager.MdiActiveDocument;
                return doc != null;
#else
                return true;
#endif
            }
        }

        #endregion Public Properties
    }
}