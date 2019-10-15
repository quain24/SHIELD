using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom
{
    public interface IIncomingDataPreparer
    {
        int CommandLengthWithData { get; }
        int CommandLength { get; }
        Regex CommandPattern { get; set; }
        int CommandTypeLength { get; set; }
        int DataPackLength { get; set; }
        int IDLength { get; set; }
        char Separator { get; set; }

        List<string> DataSearch(string data);
    }
}