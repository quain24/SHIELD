using Shield.HardwareCom.Adapters;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Factories
{
    public class MoqAdapterFactory : IMoqAdapterFactory
    {        
        private MoqAdapter _moqPort;
        private List<string> _availiblePortList = new List<string> { "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8" };
        public bool Create(int portNumber = 5)
        {
            string portName = "COM" + portNumber;

            if (!AvailablePorts.Contains(portName))
                return false;

            _moqPort = new MoqAdapter(portName);

            return true;
        }

        public MoqAdapter GivePort => _moqPort;

        public List<string> AvailablePorts => _availiblePortList;

        private void Clear()
        {
            _moqPort = null;
        }
    }
}