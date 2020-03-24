using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.CommonInterfaces;
using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.RawDataProcessing;

namespace Shield.HardwareCom.Factories
{
    public class MessengerFactory : IMessengerFactory
    {
        private readonly Func<ICommandTranslator> _commandTranslatorAutoFac;
        private readonly Func<IIncomingDataPreparer> _incomingDataPreparerAutoFac;

        public MessengerFactory(Func<ICommandTranslator> commandTranslatorAutoFac, Func<IIncomingDataPreparer> incomingDataPreparerAutoFac)
        {
            // Auto-factories because every produced messenger has to have new instance of dependencies
            _commandTranslatorAutoFac = commandTranslatorAutoFac ?? throw new ArgumentNullException(nameof(commandTranslatorAutoFac));
            _incomingDataPreparerAutoFac = incomingDataPreparerAutoFac ?? throw new ArgumentNullException(nameof(incomingDataPreparerAutoFac));
        }

        public IMessenger CreateMessangerUsing(ICommunicationDevice device)
        {
            _ = device ?? throw new ArgumentNullException(nameof(device));
            return new Messenger(device, _commandTranslatorAutoFac(), _incomingDataPreparerAutoFac());
        }
    }
}
