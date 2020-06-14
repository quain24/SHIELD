using Shield.Enums;
using System.Runtime.Serialization;
using Shield.CommonInterfaces;

namespace Shield.Persistance.Models
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
        private string _hostId;

        [DataMember]
        [OptionalField]
        private int _hostIdlength;

        [DataMember]
        [OptionalField]
        private char _separator;

        [DataMember]
        [OptionalField]
        private char _filler;

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
            get => _dataSize;
            set => _dataSize = value >= 1 ? value : 1;
        }

        /// <summary>
        /// Size of unique ID generated for every message - should always be the same for all devices
        /// </summary>
        public int IdSize
        {
            get => _idSize;
            set => _idSize = value >= 4 ? value : 4;
        }

        public int HostIdSize
        {
            get => _hostIdlength;
            set => _hostIdlength = value >= 4 ? value : 4;
        }

        public string HostId
        {
            get => _hostId;
            set => _hostId = value;
        }

        /// <summary>
        /// Size of CommandType value portion of every message - minimum value of 4. Should always be the same for all devices
        /// </summary>
        public int CommandTypeSize
        {
            get => _commandTypeSize;
            set => _commandTypeSize = value >= 4 ? value : 4;
        }

        /// <summary>
        /// Separator symbol - will be used to separate message parts, like id and command type - cannot be used in data pack - should always be the same for all devices
        /// </summary>
        public char Separator
        {
            get => _separator;
            set => _separator = value;
        }

        /// <summary>
        /// Filler is used for completing data packs that have less then required symbols - cannot be used inside datapack - should always be the same for all devices.
        /// </summary>
        public char Filler
        {
            get => _filler;
            set => _filler = value;
        }

        public SettingsType Type { get => _type; }

        public void SetDefaults()
        {
            _dataSize = 10;
            _idSize = 4;
            _commandTypeSize = 4;
            _hostId = "home";
            _hostIdlength = 4;
            _separator = '*';
            _filler = '.';
            _type = SettingsType.Application;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }
    }
}