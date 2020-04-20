using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface IMessagingPipeline
    {
        bool IsOpen { get; }

        event EventHandler<IMessageModel> MessageSent;
        event EventHandler<IMessageModel> ConfirmationSent;
        event EventHandler<IMessageModel> MessageReceived;
        event EventHandler<IMessageModel> ConfirmationReceived;
        event EventHandler<IMessageModel> SendingFailed;

        Task Close();
        void Open();
        Task<bool> SendAsync(IMessageModel message);
    }
}