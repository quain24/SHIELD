using Shield.Data;
using Shield.Data.Models;
using Shield.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Contains list of commands that will be sent or were received
/// Single message is preffered unit of communication between devices
/// </summary>

namespace Shield.HardwareCom.Models
{
    [Serializable]
    public class MessageModel : IMessageModel
    {
        private string _messageId;
        private IAppSettings _appSettings;
        private IApplicationSettingsModel _applicationSettingsModel;

        private Dictionary<int, ICommandModel> _commands = new Dictionary<int, ICommandModel>();

        public MessageModel(IAppSettings appSettings)
        {
            _appSettings = appSettings;
            _applicationSettingsModel = (IApplicationSettingsModel)_appSettings.GetSettingsFor(Enums.SettingsType.Application);
        }

        public bool IsBeingSent { get; set; } = false;
        public bool IsBeingReceived { get; set; } = false;
        public bool IsIncoming { get; set; } = false;
        public bool IsOutgoing { get; set; } = false;
        public bool IsTransmissionCompleted { get; set; } = false;
        public int CommandCount { get { return _commands.Count(); } }

        public string AssaignID(string id)
        {
            if (string.IsNullOrEmpty(id))
                _messageId = IdGenerator.GetId(_applicationSettingsModel.IdSize);
            else
                _messageId = id;
            return _messageId;
        }

        public void Add(ICommandModel command)
        {
            int id;
            if (_commands.Count == 0)
                id = 0;
            else
                id = _commands.Keys.Max() + 1;
            command.Id = _messageId;
            _commands.Add(id, command);
        }

        public bool Remove(int id)
        {
            if (_commands.Remove(id))
            {
                ResortCommands(id);
                return true;
            }
            return false;
        }

        public bool Remove(ICommandModel command)
        {
            if (_commands.Values.Any(x => x == command))
            {
                var commandToRemove = _commands.First(cmd => cmd.Value == command);
                _commands.Remove(commandToRemove.Key);
                ResortCommands(commandToRemove.Key);
                return true;
            }
            return false;
        }

        #region internal helpers

        private void ResortCommands(int fromWhichKey)
        {
            if (_commands.Count == 0 || fromWhichKey > _commands.Keys.Max())
                return;
            for (int i = fromWhichKey + 1; i < _commands.Count(); i++)
            {
                ICommandModel tmpCommand = _commands[i];
                _commands.Remove(i);
                _commands.Add(i - 1, tmpCommand);
            }
        }

        #endregion internal helpers

        #region IEnumerable implementation

        public IEnumerator<ICommandModel> GetEnumerator()
        {
            for (int i = 0; i < _commands.Count; i++)
                yield return _commands[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable implementation
    }
}