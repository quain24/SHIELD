using Shield.Enums;
using System.Runtime.Serialization;

namespace Shield.Data.Models
{
    /// <summary>
    /// Holds serializable applications configuration data - all of application specific options are held here:
    ///
    /// - MessageSize - size of a single command, header included, in chars. *0001*AS42*123456789 - example for data size of 9, ID legth of 4 and command type length of 4
    /// - IdSize - length of id thats gonna be created and injected into commands
    /// - CommandTypeSize - length of the 'type of command' block translated from enum value
    /// </summary>

    [DataContract(Name = "ApplicationSettings")]
    public class ApplicationSettingsModel : IApplicationSettingsModel
    {
        [DataMember]
        [OptionalField]
        private int _dataSize;

        [DataMember]
        [OptionalField]
        private int _idSize;

        [DataMember]
        [OptionalField]
        private int _commandTypeSize;

        [DataMember]
        [OptionalField]
        private char _separator;

        [DataMember]
        [OptionalField]
        private char _filler;

        [DataMember]
        [OptionalField]
        private int _completitionTimeout;

        [DataMember]
        [OptionalField]
        private int _confirmationTimeout;

        private SettingsType _type;

        public ApplicationSettingsModel()
        {
            SetDefaults();
        }

        /// <summary>
        /// Determines size of data pack in sent messages - should always be the same on every device
        /// </summary>
        public int DataSize
        {
            get { return _dataSize; }
            set => _dataSize = value >= 1 ? value : 1;
        }

        /// <summary>
        /// Size of unique ID generated for every message - should always be the same for all devices
        /// </summary>
        public int IdSize
        {
            get { return _idSize; }
            set => _idSize = value >= 4 ? value : 4;
        }

        /// <summary>
        /// Size of CommandType value portion of every message - minimum value of 4. Should always be the same for all devices
        /// </summary>
        public int CommandTypeSize
        {
            get { return _commandTypeSize; }
            set => _commandTypeSize = value >= 4 ? value : 4;
        }

        /// <summary>
        /// Separator symbol - will be used to separate message parts, like id and command type - cannot be used in data pack - should always be the same for all devices
        /// </summary>
        public char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        /// <summary>
        /// Filler is used for completing data packs that have less then required symbols - cannot be used inside datapack - should always be the same for all devices.
        /// </summary>
        public char Filler
        {
            get { return _filler; }
            set { _filler = value; }
        }

        /// <summary>
        /// How long a message should be held before it is marked as error because of being incomplete (in milliseconds).
        /// </summary>
        public int ConfirmationTimeout
        {
            get { return _confirmationTimeout; }
            set { _confirmationTimeout = value; }
        }

        /// <summary>
        /// how long should application wait for message confirmation (in milliseconds)
        /// </summary>
        public int CompletitionTimeout
        {
            get { return _completitionTimeout; }
            set { _completitionTimeout = value; }
        }

        public SettingsType Type { get => _type; }

        public void SetDefaults()
        {
            _dataSize = 10;
            _idSize = 4;
            _commandTypeSize = 4;
            _separator = '*';
            _filler = '.';
            _confirmationTimeout = 0;
            _completitionTimeout = 1000;
            _type = SettingsType.Application;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }
    }
}