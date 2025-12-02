using dRz.SpecSPDS.Core.Models;
using dRz.SpecSPDS.Core.Settings;
using dRz.SpecSPDS.Enums;
using Multicad;
using Multicad.DatabaseServices;
using Multicad.Symbols;
using Multicad.Symbols.Tables;

namespace dRz.SpecSPDS
{
    /// <summary>
    /// получаем универсальный маркер
    /// выбором на чертеже
    /// по текущему пространству
    /// со всего документа
    /// с нескольких документов (когданить потом)
    /// </summary>
    public partial class McUMarkerProps

    {

        public McUMarkerProps(Space spase)
        {
            if (spase == Space.All)
            {
                All();
            }
            else if (spase == Space.Layout)
            {

            }
            else if (spase == Space.Select)
            {

            }

            _spase = spase;

        }

        void All()
        {
            AppSettings appSettings = new AppSettings();

            FieldNameSettings fieldName = appSettings.Settings.FieldName;

            string McUmarkerName = appSettings.Settings.MarkerName;

            //McTableResult mcTableResult = new McTableResult();

            //McObjectId[] idSelecteds = ObjectFilter.Create(false).AddDoc(McDocument.ActiveDocument). AddType(McTable.TypeID).GetObjects().ToArray();//get mcTable all doc
            McObjectId[] idSelecteds = ObjectFilter.Create(false).AddDoc(McDocument.WorkingDocument).AddType(McUMarker.TypeID).GetObjects().ToArray();//get mcTable all doc

            int countTbl = 0;

            if (idSelecteds == null || idSelecteds.Length == 0)//not found
            {
                //mcTableResult.ResultString = $"There are no tables with this name: \"{McUmarkerName}\"";
                //return mcTableResult;
            }
            else
            {
                for (int i = 0; i < idSelecteds.Length; i++)
                {
                    DefinitionMarkerProps MarkerProp = new DefinitionMarkerProps();
                    McUMarker? tempTbl = McObjectManager.GetObject(idSelecteds[i]) as McUMarker;

                    //nc23 not Implement property "Title"
                    //nc25 string title=tempTbl.Title
                    //Title or Name одно и то же?
                    string tempTitle = tempTbl?.DbEntity.ObjectProperties.GetValueEx("Name", "").ToString();
                    //string tempName = tempTbl?.DbEntity.ObjectProperties.GetValueEx("Name", "").ToString();



                    if (tempTitle.IndexOf(McUmarkerName, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        MarkerProp.FlagSpecsRaw = tempTbl?.DbEntity.ObjectProperties.GetValueEx(fieldName.FlagSpec,"0").ToString();
                        
                        var amount = tempTbl?.DbEntity.ObjectProperties.GetValueEx(fieldName.Amount, -1);


                        MarkerProp.AmountRaw = tempTbl?.DbEntity.ObjectProperties.GetValueEx(fieldName.Amount, "").ToString();
                       
                        //countTbl++;

                        //mcTableResult.McTable = tempTbl;
                    }

                }
                if (countTbl > 1)
                {
                    //mcTableResult.ResultString = $"Several tables with this name: \"{McUmarkerName}\"";
                    //return mcTableResult;
                }
                else if (countTbl == 1)
                {
                    //mcTableResult.ResultString = $"A table with the name was found: \"{mcTableResult.McTable?.DbEntity.ObjectProperties.GetValueEx("Title", "").ToString()}\"";
                    //mcTableResult.IsOk = true;
                    //return mcTableResult;
                }
                else
                {
                    //mcTableResult.ResultString = $"There are no tables with this name: \"{McUmarkerName}\"";
                    //return mcTableResult;
                }
            }

        }

        void Layout()
        {

        }

        void Select()
        {

        }
        public List<DefinitionMarkerProps> MarkerProps { get; set; } = null;

        //тип пространства откуда брать
        Space _spase;
    }
}
