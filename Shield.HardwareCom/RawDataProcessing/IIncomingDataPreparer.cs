using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Shield.HardwareCom.RawDataProcessing
{
    public interface IIncomingDataPreparer
    {
        List<string> DataSearch(string data);
    }
}