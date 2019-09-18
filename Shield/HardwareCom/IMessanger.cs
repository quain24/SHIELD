﻿using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.Enums;
using Shield.HardwareCom.Models;

namespace Shield.HardwareCom
{
    public interface IMessanger
    {
        Task<bool> SendAsync(ICommandModel comand);
        bool Send(ICommandModel command);
        bool Setup(DeviceType type);

        void Open();
        void Close();
        Task ConstantReceiveAsync();
    }
}