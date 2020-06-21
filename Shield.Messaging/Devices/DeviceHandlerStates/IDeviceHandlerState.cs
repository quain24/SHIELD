using Shield.Messaging.Commands;
using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Devices.DeviceHandlerStates
{
    public interface IDeviceHandlerState
    {
        event EventHandler<ICommand> CommandReceived;
        void EnterState(DeviceHandlerContext context);
        void Open();
        void Close();
        Task StartListeningAsync();
        Task StopListeningAsync();
        Task<bool> SendAsync(RawCommand command);
    }
}
