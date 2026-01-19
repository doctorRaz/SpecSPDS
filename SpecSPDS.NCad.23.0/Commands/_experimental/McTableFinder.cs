#if DEBUG

using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols.Tables;
using System.Collections.Generic;
using Teigha.Runtime;
using App = HostMgd.ApplicationServices;

namespace dRz.SpecSPDS.Cad.Commands.Test
{

    //tests finds McTable
    public class McTableFinder
    {



        /// <summary>
        /// в активном пространстве.
        /// </summary>
        [CommandMethod("ффт")]
        static public void FFindTblF()
        {
            Document doc = App.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            List<McObjectId> idIds = ObjectFilter.Create().AddType(McTable.TypeID).GetObjects();

            ed.WriteMessage($"Во активном пространстве найдено {idIds.Count} таблиц");

        }


        /// <summary>
        /// Documents the find table активный документ.
        /// </summary>
        [CommandMethod("дфт")]
        static public void DocFindTblF()
        {
            Document doc = App.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            List<McObjectId> idIds = ObjectFilter.Create(true).AddType(McTable.TypeID).AddDoc(McDocument.ActiveDocument).GetObjects();

            ed.WriteMessage($"Во всех документах найдено {idIds.Count} таблиц");

        }

        /// <summary>
        /// Finds the table все документы. добавится таблица из буфера.
        /// </summary>
        [CommandMethod("фт")]
        static public void FindTblF()
        {
            Document doc = App.Application.DocumentManager.MdiActiveDocument;
            if (doc == null)
            {
                return;
            }

            Editor ed = doc.Editor;

            List<McObjectId> idIds = ObjectFilter.Create(false).AddType(McTable.TypeID).GetObjects();

            ed.WriteMessage($"Во всех документах и буфере найдено {idIds.Count} таблиц");

        }



    }
}
#endif
