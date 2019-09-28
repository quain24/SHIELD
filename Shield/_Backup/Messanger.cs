using Shield.HardwareCom.Factories;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
using Shield.HardwareCom.Models;
using Shield.HardwareCom.CommonInterfaces;
using Shield.Enums;

//  Metody wysyłania i odbierania sa uproszczone, TODO:
//  -- zmienic receiver na async
//  -- Do wysłania i odbierania ma dostac messagemodel, ktory bedzie obrabial i wywylal pojedyncze commandmodel
//  -- Odbierac ma suche command z receiver, po czym zlaczyc w message i to oddac w razie czego
//
//     Obiekty sender i receiver powinny byc uzywane tylko tu, osobno jedynie do testow - lub zmiana koncepcji

// zmienic przyjety serial port na ICommunicationDevice - zmodyfikować sender i receiver,
// bądź ogólnie przmyśleć tą koncepcję - zlikwidować?

//  Jak zmienic typ na dostep do dowolnego portu (serial, usb, podobne), tak, by nie trzeba bylo zmianaic tej klasy, a jedynie jej skladowe takie
// jak sender i receiver wysokopozioma obsluga wiadomosci powinna pozostac taka sama

namespace Shield.HardwareCom
{
    public class Messanger : IMessanger
    {
        private SerialPort _port = null;
        private ICommunicationDeviceFactory _communicationDeviceFactory;
        private IComSender _comSender;
        private IComReceiver _comReceiver;
        private int _receivedBufferSize;
        private ICommunicationDevice _device;

        public Messanger(ICommunicationDeviceFactory communicationDeviceFactory, IComSender comSender, IComReceiver comReceiver, int messageSizeBytes = 17)
        {
            _communicationDeviceFactory = communicationDeviceFactory;
            _comSender = comSender;
            _comReceiver = comReceiver;
            _receivedBufferSize = messageSizeBytes;
        }

        public bool Setup(DeviceType type, int additionalparameter)
        {
            _device = _communicationDeviceFactory.Device(type, additionalparameter);
            if (_device != null)
            {
                _comSender.Setup(_device);
                return true;
            }
            return false;
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

        public void Send(ICommandModel command)
        {
            _comSender.Send(command);
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

        public void AddCommandTemp(ICommandModel command)
        {
            _comSender.Command(command);
        }
    }
}