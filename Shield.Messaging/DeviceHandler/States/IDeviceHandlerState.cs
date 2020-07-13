using Shield.Messaging.Commands;
using System;
using System.Threading.Tasks;

namespace Shield.Messaging.DeviceHandler.States
{
    public interface IDeviceHandlerState
    {
        void EnterState(DeviceHandlerContext context, Action<ICommand> handleReceivedCommandCallback);

        void Open();

        void Close();

        Task StartListeningAsync();

        Task StopListeningAsync();

        Task<bool> SendAsync(ICommand command);
    }
}