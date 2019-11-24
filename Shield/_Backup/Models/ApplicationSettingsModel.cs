using System;

namespace Shield.Data.Models
{
    /// <summary>
    /// Holds serializable applications configuration data - all of application specific options are held here:
    ///
    /// - MessageSize - size of a single command, header included, in chars. *0001*AS42*123456789 - example for data size of 9, ID legth of 4 and command type length of 4
    /// - IdSize - length of id thats gonna be created and injected into commands
    /// - CommandTypeSize - length of the 'type of command' block translated from enum value
    /// </summary>

    [Serializable]
    public class ApplicationSettingsModel : IApplicationSettingsModel
    {
        public int DataSize { get; set; } = 30;
        public int IdSize { get; set; } = 4;
        public int CommandTypeSize { get; set; } = 4;
        public char Separator { get; set; } = '*';
        public char Filler { get; set; } = '.';
    }
}