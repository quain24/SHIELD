using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom
{
    public class MessageHWComEventArgs : EventArgs
    {
        private IMessageModel _message;

        public MessageHWComEventArgs(IMessageModel message)
        {
            _message = message;
        }

        public IMessageModel Message { get { return _message; } }
    }
}