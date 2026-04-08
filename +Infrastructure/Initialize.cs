using drz.SpecSPDS.Abstractions.Infrastructure;
using drz.SpecSPDS.Abstractions.Services;
using drz.SpecSPDS.Infrastructure.Infrastructure;
using drz.SpecSPDS.Infrastructure.Services;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;
using SimpleInjector;
using System;
using Rtm = Teigha.Runtime;

namespace drz.SpecSPDS.Infrastructure
{
    public class CadPlugin : Rtm.IExtensionApplication
    {
#if DEBUG
        [Rtm.CommandMethod("инит-сингл")]
        //[Description("ручной инит загрузчика")]
#endif
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
                Init();
            }
            catch (Exception ex)
            {
                string message = $"Initialize error : {ex.Message}"
                                 + $"\n{ex.StackTrace}";

                Application.ShowAlertDialog(message);

                Document doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    return;
                }

                Editor ed = doc.Editor;

                ed.WriteMessage(message);
            }
        }

        /// <summary>
        /// Если этот код будет в Initialize напрямую <br/>
        /// то при любом сбое, например нет SimpleInjector.dll или не та версия <br/>
        /// любой вызов может привести к ошибке<br/>
        /// сложно диагностировать <br/>
        /// возникнет "неотлавливаемое" исключение <br/>
        /// try не сработает <br/>
        /// --------------------<br/>
        /// убедиться легко, собери свой вариант <br/>
        /// подключись отладкой, поставь бряк в line 19<br/>
        /// загрузи сборку <br/>
        /// бряк не сработает <br/>
        /// эксепшен то же не будет <br/>
        /// </summary>
        private void Init()
        {

            Container = new Container();
            ConfigureContainer(Container);
            Container.Verify();
        }

        public void Terminate()
        {
            Container?.Dispose();
        }

        public static Container Container { get; private set; }

        private void ConfigureContainer(Container container)
        {
            container.Register<IApplicationInfo, ApplicationInfo>(Lifestyle.Singleton);

            container.Register<IMessageService>(() =>
            {
                IApplicationInfo applicationInfo = container.GetInstance<IApplicationInfo>();
                return new MessageService(applicationInfo);
            },
                Lifestyle.Singleton);

            container.Register<IDocumentService, DocumentService>(Lifestyle.Transient);
        }
    }
}
