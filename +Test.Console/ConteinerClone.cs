using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using drz.AddOnRuntime;
using drz.Lib_B;
using System;

namespace drz.SpecSPDS.Test
{
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
            string msg = $"{nameof(ConteinerCloneRun)} Init";

            //-------------------------
            //-------------------------
            var dd = AddOnCtx.Services.Get<IAddOnInfoRegistry>();
            var фв = AddOnCtx.Services.Get<IAddOnInfo>();
            var k = dd.GetKeys();



            var dd1 = AddOnCtx.Services.Get<IAddOnInfoRegistry>();
            var фв1 = AddOnCtx.Services.Get<IAddOnInfo>();

            var k1 = dd.GetKeys();

            IAddOnInfo info = dd1.GetOrAdd(typeof(ContainerTransfer).Assembly);

            IAddOnInfo info2;
            bool b = dd1.TryGet(typeof(ContainerTransfer).Assembly.Location, out /*IAddOnInfo*/ info2);

            foreach (var kkk in k1)
            {
                bool b0 = dd1.TryGet(kkk, out /*IAddOnInfo*/ info);


            }
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.AddOnInfo.InstalledVersion.ToString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.AddOnInfo.InstalledVersion.ToString());


            //------------------------
            msg = $"{nameof(ConteinerCloneRun)} end";
            _logger.Info(msg);
            _msgCmd.InfoMessage(msg);
        }

        public static void Run()
        {
            ConteinerClone conteinerClone = new ConteinerClone();
            conteinerClone.ConteinerCloneRun();
        }
    }
}