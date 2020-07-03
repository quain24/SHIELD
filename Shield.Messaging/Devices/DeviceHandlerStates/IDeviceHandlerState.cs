using Shield.Messaging.Commands;
using System;
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

        Task<bool> SendAsync(ICommand command);
    }
}