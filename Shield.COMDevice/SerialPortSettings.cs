using Shield.CommonInterfaces;
using Shield.Enums;
using System.IO.Ports;

namespace Shield.COMDevice
{
    public class SerialPortSettings : ICommunicationDeviceSettings
    {
        public int PortNumber { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public Parity Parity { get; set; }

        public StopBits StopBits { get; set; }

        public int ReadTimeout { get; set; }

        public int WriteTimeout { get; set; }

        public int Encoding { get; set; }

        /// <summary>
        /// how long should application wait for message confirmation (in milliseconds)
        /// </summary>
        public int CompletitionTimeout { get; set; }

        /// <summary>
        /// How long a message should be held before it is marked as error because of being incomplete (in milliseconds).
        /// </summary>
        public int ConfirmationTimeout { get; set; }

        public string Name { get => $"COM{PortNumber}"; private set { } }

        public SettingsType Type { get; }
    }
}