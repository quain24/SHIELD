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

        public ApplicationSettingsModel()
        {
            SetDefaults();
        }

        public int DataSize
        {
            get { return _dataSize; }
            set => _dataSize = value >= 1 ? value : 1;
        }

        public int IdSize
        {
            get { return _idSize; }
            set => _idSize = value >= 4 ? value : 4;
        }

        public int CommandTypeSize
        {
            get { return _commandTypeSize; }
            set => _commandTypeSize = value >= 4 ? value : 4;
        }

        public char Separator
        {
            get { return _separator; }
            set { _separator = value; }
        }

        public char Filler
        {
            get { return _filler; }
            set { _filler = value; }
        }

        public SettingsType Type { get; set; }

        public void SetDefaults()
        {
            _dataSize = 10;
            _idSize = 4;
            _commandTypeSize = 4;
            _separator = '*';
            _filler = '.';
            Type = SettingsType.Application;
        }

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            SetDefaults();
        }
    }
}