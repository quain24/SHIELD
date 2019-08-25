using Shield.HardwareCom.Factories;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;

//  Metody wysyłania i odbierania sa uproszczone, TODO:
//  -- zmienic receiver na async
//  -- Do wysłania i odbierania ma dostac messagemodel, ktory bedzie obrabial i wywylal pojedyncze commandmodel
//  -- Odbierac ma suche command z receiver, po czym zlaczyc w message i to oddac w razie czego
//
//     Obiekty sender i receiver powinny byc uzywane tylko tu, osobno jedynie do testow

namespace Shield.HardwareCom
{
    public class ComMessanger : IComMessanger
    {
        private SerialPort _port = null;
        private IComSender _comSender;
        private IComReceiver _comReceiver;
        private int _receivedBufferSize;

        public ComMessanger(IComSender comSender, IComReceiver comReceiver, int messageSizeBytes = 17)
        {
            _comSender = comSender;
            _comReceiver = comReceiver;
            _receivedBufferSize = messageSizeBytes;
        }

        public SerialPort Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
                _comSender.Setup(_port);
                _comReceiver.Setup(_port, _receivedBufferSize);
            }
        }

        public List<string> Receive()
        {
            return _comReceiver.DataReceived;
        }

        // Testowa wersja metody async, domyslnie i tak caly com receiver bedzie async!
        public async Task<List<string>> ReceiveAsync()
        {
            return await Task.Run(() => _comReceiver.DataReceived).ConfigureAwait(false);
        }

        public void Send()
        {
            _comSender.Send();
        }

        public Task Close()
        {
            // Close the serial port in a new thread
            Task closeTask = new Task(() =>
            {
                try
                {
                    _port.Close();
                }
                catch (IOException e)
                {
                    // Port was not open
                    Debug.WriteLine("Port was not open! " + e.Message);
                    throw e;
                }
            });
            closeTask.Start();

            return closeTask;

            // odniorca:
            // await serialStream.Close();
        }

        public void AddCommandTemp(ICommand command)
        {
            _comSender.Command(command);
        }       
    }
}