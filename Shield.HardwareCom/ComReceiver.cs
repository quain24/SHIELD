using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//  Obiekty tej klasy powinny dzialac w swoim watku

namespace Shield.HardwareCom
{
    public class ComReceiver : IComReceiver
    {
        private SerialPort _port;
        private List<string> _dataReceived = new List<string>();
        private string _singleCommand = string.Empty;
        private int _commandLength;

        public void Setup(SerialPort port, int messageSizeBytes)
        {
            _port = port;
            _port.DataReceived += new SerialDataReceivedEventHandler(Receive);
            _commandLength = messageSizeBytes;
        }

        // Prowizorycznie, na razie zawsze schwyta wszystko, buforuje po 17 jako jedna wiadomosc i zapisuje do listy,
        // docelowo ma generowac command z bufora i to dodac do bufora comend, ktory udostepnia
        public void Receive(object sender, SerialDataReceivedEventArgs e)
        {
            foreach (var c in _port.ReadExisting())
            {
                 _singleCommand += c;
                if(_singleCommand.Length == _commandLength)
                {
                    _dataReceived.Add(_singleCommand);
                    _singleCommand = string.Empty;
                }
            }
        } 
        
        public List<string> DataReceived
        {
            get{return _dataReceived;}
        }
    }
}