using dRz.SpecSPDS.Core.Extensions;
using System;
using System.Collections.Generic;

namespace dRz.SpecSpdsConsole
{
    internal class OutResult
    {
        internal OutResult()
        {
            McUmarkerProps mcUmarkerProps = new McUmarkerProps();

            List<string> wordsMark = new List<string> { "маркер", "маркера", "маркеров" };
            List<string> wordsDoc = new List<string> { "документ", "документа", "документов" };

            string resultString = $"\nСтатистика:";
            resultString += $"\nФайлов в обработке:";


            if (mcUmarkerProps.CountFilesTotal > 0)
            {
                resultString += $"\n\tВсего: {mcUmarkerProps.CountFilesTotal} {wordsDoc.Declens(mcUmarkerProps.CountFilesTotal)}";
            }

            if (mcUmarkerProps.CountFilesRead > 0)
            {
                resultString += $"\n\tПрочитано: {mcUmarkerProps.CountFilesRead} {wordsDoc.Declens(mcUmarkerProps.CountFilesRead)}";
            }

            if (mcUmarkerProps.BadFilePatchs.Count > 0)
                if (mcUmarkerProps.BadFilePatchs.Count > 0)
                {
                    foreach (string filename in mcUmarkerProps.BadFilePatchs)
                    {
                        resultString += $"\n\t\tДокумент не обработан: {filename}";
                    }
                }
            resultString += $"\nМаркеров:";

            resultString += $"\n\tНайдено всего: {mcUmarkerProps.CountTotal} {wordsMark.Declens(mcUmarkerProps.CountTotal)} за {mcUmarkerProps.ElapsedID}";


            if (mcUmarkerProps.CountFalseName > 0)
            {
                resultString += $"\n\tС неподходящим именем: {mcUmarkerProps.CountFalseName} {wordsMark.Declens(mcUmarkerProps.CountFalseName)}";
            }

            if (mcUmarkerProps.CountNotFlag > 0)
            {
                resultString += $"\n\tБез признака включения в спецификацию: {mcUmarkerProps.CountNotFlag} {wordsMark.Declens(mcUmarkerProps.CountNotFlag)}";
            }

            if (mcUmarkerProps.CountIncorrectData > 0)
            {
                resultString += $"\n\tС некорректными данными: {mcUmarkerProps.CountIncorrectData} {wordsMark.Declens(mcUmarkerProps.CountIncorrectData)}";
            }
            resultString += $"\n\tВключено в набор: {mcUmarkerProps.CountAdded} {wordsMark.Declens(mcUmarkerProps.CountAdded)} за {mcUmarkerProps.ElapsedProp}";

            Console.WriteLine($"{resultString}\n");

        }
    }
    public class McUmarkerProps
    {
        public int CountFilesTotal = 10;
        public int CountFilesRead = 10;
        public int CountTotal = 10;
        public int CountAdded = 10;
        public int CountFalseName = 10;
        public int CountNotFlag = 10;
        public int CountIncorrectData = 10;




        public string ElapsedID = "10.10.10";
        public string ElapsedProp = "10.10.10";


        public List<string> BadFilePatchs = new List<string>() { "dddasdsa", "dasdsa", "ffffff" };
    }


}
