using System.Text;

namespace Shield.HardwareCom
{
    public class IncomingDataPreparer
    {
        private StringBuilder _buffer = new StringBuilder();

        //public async Task StartReceivingAsync()
        //{
        //        if (_port.Encoding.CodePage == Encoding.ASCII.CodePage)
        //            _receivedBuffer.Append(rawData.RemoveASCIIChars());
        //        else
        //            _receivedBuffer.Append(rawData);

        //        if (_receivedBuffer.Length >= _completeCommandSizeWithSep)
        //        {
        //            string workPiece = _receivedBuffer.ToString(0, _completeCommandSizeWithSep);
        //            int whereToCut = CheckRawData(workPiece);

        //            if (whereToCut > 0)
        //            {
        //                _receivedBuffer.Remove(0, whereToCut);
        //                workPiece = workPiece.Substring(0, whereToCut);
        //            }

        //            // All good or all bad
        //            else
        //                _receivedBuffer.Remove(0, _completeCommandSizeWithSep);

        //            DataReceived?.Invoke(this, workPiece);
        //        }
        //        else
        //        {
        //            continue;
        //        }
        //    }
        //    _isLissening = false;
        //}

        //private int CheckRawData(string data)
        //{
        //    Match match = CommandPattern.Match(data);
        //    if (match.Success)
        //        return match.Index;

        //    int firsIndexOfSepprarator = data.IndexOf(SEPARATOR);

        //    if (firsIndexOfSepprarator == -1)
        //        return firsIndexOfSepprarator;

        //    if (firsIndexOfSepprarator == 0)
        //        return 1;
        //    else
        //        return firsIndexOfSepprarator;
        //}
    }
}