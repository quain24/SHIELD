using System;
using System.Threading.Tasks;
using Shield.Messaging.Commands;

namespace Shield.Messaging.DeviceHandler.States
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