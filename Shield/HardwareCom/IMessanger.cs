using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IMessanger
    {
        Task<bool> SendAsync(ICommandModel comand);
        Task<bool> SendAsync(IMessageModel message);
        bool Send(IMessageModel message);
        bool Setup(DeviceType type);

        void Open();
        void Close();
        Task ConstantReceiveAsync();
    }
}