using System;
using System.Threading.Tasks;
using Shield.Messaging.Commands;

namespace Shield.Messaging.DeviceHandler
{
    public interface IDeviceHandler
    {
        event EventHandler<ICommand> CommandReceived;
        event EventHandler<ICommand> CommandSent;
        event EventHandler<ICommand> CommandSendingFailed;
        string Name { get; }
        bool IsReady { get; }
        bool IsOpen { get; }
        bool IsConnected { get; }
        void Open();
        void Close();
        Task StartListeningAsync();
        Task StopListeningAsync();
        Task<bool> SendAsync(ICommand command);
    }
}