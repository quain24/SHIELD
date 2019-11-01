using Shield.Enums;
using Shield.HardwareCom.Models;
using System;

namespace Shield.HardwareCom
{
    public class MessageErrorEventArgs : EventArgs
    {
        private IMessageModel _message;
        private MessageErrors _errors = MessageErrors.None;

        public MessageErrorEventArgs(IMessageModel message, MessageErrors errors)
        {
            _message = message;
            _errors = errors;
        }

        public IMessageModel Message { get { return _message; } }
        public MessageErrors Errors { get { return _errors; } }
    }
}