using drz.Abstractions.Services;
using System;
using System.IO;

//todo все связанное с HostMgd в отдельную сборку CadInfrastructure
#if !TEST
using HostMgd.ApplicationServices;
#endif

namespace drz.Infrastructure.Services
{
    public class DocumentService : IDocumentService
    {

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
    }
}