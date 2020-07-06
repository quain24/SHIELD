using System.Text.RegularExpressions;

namespace Shield.HardwareCom.RawDataProcessing
{
    public interface ICommandConfiguration
    {
        int CommandTypeLength { get; }
        int IDLength { get; }
        int HostIDLength { get; }
        int DataPackLength { get; }
        int CommandLength { get; }
        int CommandLengthWithDataPack { get; }
        Regex CommandPattern { get; }
        char Separator { get; }
        char Filler { get; }
        object CommandTypeIndex { get; }
    }
}