using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public interface IComReceiver
    {
        void Receive(object sender, SerialDataReceivedEventArgs data);       
        void Setup(SerialPort port, int messageSizeBytes);

        List<string> DataReceived {get;}
    }
}