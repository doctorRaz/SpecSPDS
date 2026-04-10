using Abstractions.Enums;
using Abstractions.Factories;
using Abstractions.Services;
using NCad.Services;
using SimpleInjector;
using System;

namespace NCad.Factories
{
    internal class MessageServiceFactory : IMessageServiceFactory
    {
        public MessageServiceFactory(Container container)
        {
            _container = container;
        }

        public IMessageService GetService(MessageServiceType messageType)
        {
            if (messageType == MessageServiceType.CommandLine)
            {
                return _container.GetInstance<CommandLineMessageService>();
            }

            if (messageType == MessageServiceType.Window)
            {
                return _container.GetInstance<WindowMessageService>();
            }

            throw new ArgumentOutOfRangeException(nameof(messageType));
        }

        private readonly Container _container;
    }
}
