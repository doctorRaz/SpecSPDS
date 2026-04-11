using drz.Abstractions.Services;
using System;
using System.IO;

namespace Test.Services
{
    public class DocumentService : IDocumentService
    {
        public bool IsActive
        {
            get
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                return doc != null;
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

                Document doc = Application.DocumentManager.MdiActiveDocument;
                return doc.Name;
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
