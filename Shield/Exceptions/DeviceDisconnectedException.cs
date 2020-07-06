using System;
using System.Runtime.Serialization;

namespace Shield.Exceptions
{
    [Serializable]
    public class DeviceDisconnectedException : Exception
    {
#pragma warning disable RCS1187 // Use constant instead of field.
        private static readonly string DefaultMessage = "A communication device has been unsafely physically disconnected while being active / open";
#pragma warning restore RCS1187 // Use constant instead of field.

        public DeviceDisconnectedException() : base(DefaultMessage)
        {
        }

        public DeviceDisconnectedException(string message) : base(message)
        {
        }

        public DeviceDisconnectedException(string message, Exception innerException) : base(message, innerException)
        { }

        // Ensure Exception is Serializable
        protected DeviceDisconnectedException(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
        { }
    }
}