using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using drz.AddOnRuntime;
using drz.Clone_A;
using System;
using System.Reflection;

namespace drz.SpecSPDS.Test
{
    /// <summary>
    /// что будет если в одном процессе создать нескольтко контейнеров?
    /// IAddOnServices должен зарегистрировать  все IAddOnInfo<br/>
    /// и отдавать  их каждый своей библиотеке<br/>
    /// есть  доступ ко всем зарегистрированным IAddOnInfo
    /// </summary>
    internal class ConteinerClone
    {
        #region Private Fields

        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger? _logger;
        private readonly ICommandLineMessageService? _msgCmd;

        #endregion Private Fields

        #region Internal Constructors

        /// <summary>Initializes a new instance of the <see cref="ConteinerClone"/> class.</summary>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot initialization failed</exception>
        internal ConteinerClone()
        {
            try
            {
                if (_isAddOnCompositionRoot) return;

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ConteinerClone).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                string msg = $"{nameof(ConteinerClone)} Init";

                //логер
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ConteinerClone));
                _logger.InfoCaller(msg);

                //ком строка
                _msgCmd = AddOnCtx.MsgCmd;
                _msgCmd.InfoMessage(msg);

                _isAddOnCompositionRoot = true;//сервис поднялся
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        #endregion Internal Constructors

        internal void ConteinerCloneRun()
        {
            string msg = $"{nameof(ConteinerCloneRun)} Running";
            _msgCmd.InfoMessage(msg);

            IAddOnInfoRegistry addonRegistreds = AddOnCtx.AddOnInfoRegistry;

            //get count IaddOnInfo
            _msgCmd.InfoMessage($"addonRegistreds.Count: {addonRegistreds.Count}");
            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToString());
            //get registrator


            //*******************
            // Так делать не надо, интерфейс из контейнера не должен торчать
            //*******************
            //add other lib info
            Type typeFake = typeof(ConteinerCloneA);
            addonRegistreds.GetOrAdd(typeFake);
            AddOnCompositionRoot root = new AddOnCompositionRoot(typeFake.Assembly);

            //*********************
            _msgCmd.InfoMessage($"addonRegistreds.Count: {addonRegistreds.Count}");

            //get container lib info
            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToString());

            //get from key lib info
            IAddOnInfo addonFake;
            Assembly keyFake = typeFake.Assembly;

            if (addonRegistreds.TryGet(keyFake, out addonFake))
            {
                _msgCmd.InfoMessage(addonFake.ToString());
            }

            if (addonRegistreds.TryGet(typeof(ConteinerCloneA), out addonFake))
            {
                _msgCmd.InfoMessage(addonFake.ToString());
            }

            if (addonRegistreds.TryGet<ConteinerCloneA>(out addonFake))
            {
                _msgCmd.InfoMessage(addonFake.ToString());
            }

            var ser = AddOnCtx.Services;
            //console all addons
            foreach (var addon in addonRegistreds.GetValues())
            {
                _msgCmd.InfoMessage(addon.ToString());
            }
            ConteinerCloneA.Run();

            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToString());
            _msgCmd.InfoMessage($"addonRegistreds.Count: {addonRegistreds.Count}");
            //-------------------------

            /*
 ConteinerClone conteinerClone = new ConteinerClone();
                _msgCmd = AddOnCtx.MsgCmd;
                _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToLongString());

                var services = AddOnCtx.Services;

                var addonRegistreds = services.Get<IAddOnInfoRegistry>();

                IAddOnInfo libA = addonRegistreds.GetOrAdd<ContainerTransferA>();
                _msgCmd.InfoMessage(libA.ToLongString());

                IAddOnInfo lib = addonRegistreds.GetOrAdd<ContainerTransferA>();
            //-------------------------
            var dd = AddOnCtx.Services.Get<IAddOnInfoRegistry>();
            var фв = AddOnCtx.Services.Get<IAddOnInfo>();
            var k = dd.GetKeys();

            var dd1 = AddOnCtx.Services.Get<IAddOnInfoRegistry>();
            var фв1 = AddOnCtx.Services.Get<IAddOnInfo>();

            var k1 = dd.GetKeys();

            IAddOnInfo info = dd1.GetOrAdd(typeof(ContainerTransfer).Assembly);

            IAddOnInfo info2;
            bool b = dd1.TryGet(typeof(ContainerTransfer).Assembly.Location, out info2);

            foreach (var kkk in k1)
            {
                bool b0 = dd1.TryGet(kkk, out   info);
            }
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.AddOnInfo.InstalledVersion.ToString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.AddOnInfo.InstalledVersion.ToString());

            //------------------------
            msg = $"{nameof(ConteinerCloneRun)} end";
            _logger.Info(msg);
            _msgCmd.InfoMessage(msg);
*/
        }

        public static void Run()
        {
            ConteinerClone conteinerClone = new ConteinerClone();
            conteinerClone.ConteinerCloneRun();
        }
    }
}