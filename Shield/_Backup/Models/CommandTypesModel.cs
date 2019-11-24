using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    [DataContract(Name = "CommandTypes")]
    public class CommandTypesModel : ICommandTypesModel
    {
        private bool _commandListChanged;

        [NonSerialized]
        private Dictionary<string, int> _basicCommandTypes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        [DataMember]
        [OptionalField]
        private Dictionary<string, int> _additionalCommandTypes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);

        [NonSerialized]
        private Dictionary<string, int> _commandTypes;

        public CommandTypesModel()
        {
            SetBasicCommands();
        }

        public Dictionary<string, int> CommandTypes
        {
            get
            {
                if (_commandListChanged == false)
                {
                    return _commandTypes;
                }
                _commandTypes = _basicCommandTypes.Concat(_additionalCommandTypes)
                    .ToLookup(x => x.Key, x => x.Value)
                    .ToDictionary(x => x.Key, y => y.First());
                _commandListChanged = false;
                return _commandTypes;
            }
        }

        public bool AddCommand(string command)
        {
            if (_basicCommandTypes.ContainsKey(command) ||
                _additionalCommandTypes.ContainsKey(command) ||
                string.IsNullOrEmpty(command) ||
                string.IsNullOrWhiteSpace(command) ||
                command.Contains(" ")
                )
                return false;
            else
            {
                int newCommandIndex = _basicCommandTypes.Count + _additionalCommandTypes.Count;
                if (command.Length > 1)
                    command = command.First().ToString().ToUpper(CultureInfo.InvariantCulture) + command.Substring(1);
                else
                    command = command.ToUpper(CultureInfo.InvariantCulture);

                _additionalCommandTypes.Add(command, newCommandIndex);
                return true;
            }
        }

        public bool RemoveCommand(string command)
        {
            if (_additionalCommandTypes.ContainsKey(command))
            {
                _additionalCommandTypes.Remove(command);
                return true;
            }

            return false;
        }

        private void SetBasicCommands()
        {
            _basicCommandTypes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
            {
                {"Empty", 0 },
                {"HandShake", 1 },
                {"Master", 2},
                {"Slave", 3},
                {"Confirmation", 4},
                {"EndMessage", 5},
                {"ReceivedAsCorrect", 6},
                {"ReceivedAsError", 7},
                {"ReceivedAsUnknown", 8},
                {"ReceivedAsPartial", 9},
                {"Error", 10},
                {"Unknown", 11},
                {"Partial", 12},
                {"Confirm", 13},
                {"Cancel", 14},
                {"RetryLast", 15},
                {"Data", 16}
            };

            _commandTypes = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            _commandListChanged = true;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            SetBasicCommands();
        }
    }
}