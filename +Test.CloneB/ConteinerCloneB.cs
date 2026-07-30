global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using drz.AddOnRuntime;
using drz.Infrastructure.Infrastructure;
using System;
using System.Reflection;

namespace drz.Clone_B
{
    public class ConteinerCloneB
    {
        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger? _logger;
        private readonly ICommandLineMessageService? _msgCmd;

        public ConteinerCloneB()
        {
            try
            {
                if (_isAddOnCompositionRoot) return;

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ConteinerCloneB).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                string msg = $"{nameof(ConteinerCloneB)} Init";

                //логер
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ConteinerCloneB));
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

        public static void Run()
        {
            ConteinerCloneB conteinerCloneB = new ConteinerCloneB();
            conteinerCloneB.ConteinerCloneB_Run();
        }

        private void ConteinerCloneB_Run()
        {
            string msg = $"{nameof(ConteinerCloneB)} Running";
            _msgCmd.InfoMessage(msg);

            IAddOnInfoRegistry addonRegistreds = AddOnCtx.AddOnInfoRegistry;
            //get count IaddOnInfo
            _msgCmd.InfoMessage($"addonRegistreds.Count: {addonRegistreds.Count}");
            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToString());
            //get registrator

            //плохо есть доступ к созданию в словаре аддон инфо
            IAddOnInfo addOnInfoBad = AddOnInfoRegistry.Get(typeof(ContextStaticAttribute).Assembly);
            _msgCmd.InfoMessage($"addonRegistreds.Count: {addonRegistreds.Count}");

            //хорошо отдельный экземпляр, но только в сборке подключенной к AddOnCompositionRoot
            IAddOnInfo addOnInfoGood = new AddOnInfo(typeof(ConteinerCloneB).Assembly);
            _msgCmd.InfoMessage($"addonRegistreds.Count: {addonRegistreds.Count}");

            //*******************
            // Так делать не надо, интерфейс из контейнера не должен торчать
            //*******************
            //add other lib info
            Type typeFake = typeof(Convert);
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

            if (addonRegistreds.TryGet(typeof(ConteinerCloneB), out addonFake))
            {
                _msgCmd.InfoMessage(addonFake.ToString());
            }

            if (addonRegistreds.TryGet<ConteinerCloneB>(out addonFake))
            {
                _msgCmd.InfoMessage(addonFake.ToString());
            }

            var ser = AddOnCtx.Services;
            //console all addons
            foreach (var addon in addonRegistreds.GetValues())
            {
                _msgCmd.InfoMessage(addon.ToString());
            }

            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToString());
        }
    }
}