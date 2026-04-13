using drz.Abstractions.Services;
using System;
using System.IO;

namespace drz.Infrastructure.Infrastructure
{
    public class DocumentService : IDocumentService
    {
        public bool IsActive
        {
            get
            {
                //Document doc = Application.DocumentManager.MdiActiveDocument;
                return true;// doc != null;
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

                //Document doc = Application.DocumentManager.MdiActiveDocument;
                return @"d:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\_res\сборка маркеров.dwg";
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