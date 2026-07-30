global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using drz.AddOnRuntime;
using drz.Clone_B;
using System;
using System.Reflection;

namespace drz.Clone_A
{
    public class ConteinerCloneA
    {
        #region Private Fields

        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger? _logger;
        private readonly ICommandLineMessageService? _msgCmd;

        #endregion Private Fields

        public ConteinerCloneA()
        {
            try
            {
                if (_isAddOnCompositionRoot) return;

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ConteinerCloneA).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                string msg = $"{nameof(ConteinerCloneA)} Init";

                //логер
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ConteinerCloneA));
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

        public void ConteinerCloneA_Run()
        {
            string msg = $"{nameof(ConteinerCloneA)} Running";
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
            Type typeFake = typeof(ContextMarshalException);
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

            ConteinerCloneB.Run();
            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToString());
        }

        public static void Run()
        {
            ConteinerCloneA conteinerCloneA = new ConteinerCloneA();
            conteinerCloneA.ConteinerCloneA_Run();
        }
    }
}