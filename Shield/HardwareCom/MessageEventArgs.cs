using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom
{
    public class MessageEventArgs : EventArgs
    {
        private IMessageModel _message;

        public MessageEventArgs(IMessageModel message)
        {
            _message = message;
        }

        public IMessageModel Message { get { return _message; } }
    }
}