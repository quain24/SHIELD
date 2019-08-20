using System.IO.Ports;
using System.Text;

namespace CommunicationManager
{
    public interface IComPortManager
    {
        Encoding Encoding { get; set; }
        int ReadTimeout { get; set; }
        int WriteTimeout { get; set; }

        void Create(int portNumber = 5, int baudRate = 19200, int dataBits = 8, Parity parity = Parity.None, StopBits stopBits = StopBits.One);
        SerialPort GetPort();
        void Clear();
    }
}