using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom
{
    public class MessageHWComEventArgs : EventArgs
    {
        private IMessageHWComModel _message;

        public MessageHWComEventArgs(IMessageHWComModel message)
        {
            _message = message;
        }

        public IMessageHWComModel Message { get { return _message; } }
    }
}