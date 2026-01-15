#if DEBUG
using Multicad;
using Multicad.DatabaseServices;
using Teigha.DatabaseServices;
using Teigha.Runtime;

namespace dRz.SpecSPDS.Cad.TestCommand
{
    /// <summary>
    /// мультикад чтение и редактирование файла в скрытом режиме
    /// </summary>
    public class McReadWriteHiddenDoc
    {
        [CommandMethod("PaintFile")]
        static public void PaintFile()
        {
            //https://developer.nanocad.ru/redmine/boards/4/topics/1681?r=1683#message-1683
            string filName = @"d:\@Developers\Programmers\!NET\!SpecSPDS\res\test.dwg";
            McDocument pDoc = McDocumentsManager.OpenDocument(filName, false, true);

            using (Database db0 = new Database(false, false))
            {
                db0.ReadDwgFile(filName, FileOpenMode.OpenForReadAndAllShare, false, "", false);

            }
            McDocument pOldWD = McDocument.WorkingDocument;

            McDocument.WorkingDocument = pDoc;
            ObjectFilter of = ObjectFilter.Create(false).AddDoc(pDoc);
            of.AllObjects = true;
            List<McObjectId> ids = of.GetObjects();
            foreach (McObjectId id in ids)
            {
                McDbEntity pEnt = id.GetObject().Cast<McDbEntity>();
                if (pEnt == null)
                    continue;
                pEnt.Color = Color.Red;
            }
            McObjectManager.UpdateAll();
            pDoc.SaveAs(filName);
            pDoc.Close();
            McDocument.WorkingDocument = pOldWD;
        }

    }
}

#endif