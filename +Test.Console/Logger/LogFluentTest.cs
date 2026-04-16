using drz.Abstractions.Logger;
using drz.Src.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace drz.SpecSpds.Test.Logger
{
    internal class LogFluentTest
    {
        #region Private Fields

        private IDrzLogger log = LoggerProvider.For<LogFluentTest>();

        #endregion Private Fields

        #region Internal Methods

        internal void Test()
        {
            log.Fatal("----------LogFluentTest------------");
            var userId = "user_ID";
            var documentPath = "documentPath";
            var value = 42;

            int b = 0;

            int calcs()
            {
                int a = 10;

                return a + b;
            }
            b = calcs();

            if (log.IsDebugEnabled)
            {
                log.Warn($"This is a message from {calcs()}");
            }

            TestClass t = new TestClass();

            try
            {
                if (log.IsDebugEnabled)
                {
                    log.ForDebugEvent()
                       .Message("Начало работы")
                       .Property("prop1", calcs())
                       .Property("prop2", 123)
                       .Log();
                }

                log.ForTraceEvent()
                       .Message("Класс Продолжение работы")
                       .Property("prop10", calcs())
                       .Property("класс", t)
                       .Log();

                int e = 0;

                log.Trace($"Сообщение  1, 2");

                int ii = 10 / e;
            }
            catch (Exception ex)
            {
                // Dictionary
                var props = new Dictionary<string, object>
                        {
                            { "UserId",   userId        },
                            { "Action",   "SaveDocument" },
                            { "Path",     documentPath  },
                            { "Value",    value         }
                        };

                log.ForErrorEvent()
                    .Message("Ошибка обработки Properties")
                    .Properties(props)
                    .Exception(ex)
                    .Log();

                //********************************

                // Список объектов через LINQ

                var elements = new List<CadElement>
                        {
                            new CadElement { Id = 1042, Name = "Колонна",  Layer = "Конструкции", Type = "Column", Length = 3600 },
                            new CadElement { Id = 1078, Name = "Балка",    Layer = "Конструкции", Type = "Beam",   Length = 6000 },
                            new CadElement { Id = 1091, Name = "Плита",    Layer = "Перекрытия",  Type = "Slab",   Length = 9000 },
                        };

                log.ForErrorEvent()
                                   .Message("Найдены элементы на чертеже Properties")
                                   .Property("TotalCount", elements.Count)
                                   .Properties(elements.Select((el, i) =>
                                                                   new KeyValuePair<string, object>(
                                                                   $"Element_{i}",
                                                                   $"{el.Name} [{el.Id}] Layer={el.Layer} Type={el.Type} L={el.Length}mm")))
                                    .Exception(ex)
                                    .Log();

                //********************************

                string val = null;

                log.ForErrorEvent()
                    .Message("Properties is null")
                    .Property("name", val)
                    .Properties(null)
                    .Exception(ex)
                    .Log();

                log.ForErrorEvent()
                   .Exception(ex)
                   .Property("prop1", 50000)
                   .Property("prop2", 123)
                   .Exception(ex)
                   .Log();

                log.Error(ex);
                log.Error("messag do", ex);
                log.Error(ex, "messag posle");
                log.Info("Продолжение работы после ошибки");
            }
            finally
            {
            }
        }

        #endregion Internal Methods

        #region Internal Classes

        private class TestClass
        {
            #region Public Fields

            public string g = "10";
            public string g1 = "10";
            public string g2 = "10";

            #endregion Public Fields
        }

        #endregion Internal Classes

        #region Private Classes

        private class CadElement
        {
            #region Public Properties

            public int Id { get; set; }
            public string Layer { get; set; }
            public double Length { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }

            #endregion Public Properties
        }

        #endregion Private Classes
    }
}