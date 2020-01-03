using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    class TimeoutChecker
    {
        private BlockingCollection<IMessageHWComModel> _storage = new BlockingCollection<IMessageHWComModel>();
        private IConfirmationTimeout _timeoutChecker;
        private long _timeout = 0;

        public TimeoutChecker(IConfirmationTimeout timeoutChecker)
        {
            _timeoutChecker = timeoutChecker;
        }

        public void SetTimeout(long timeout)
        {
            if(_timeoutChecker is null)
                return;
            _timeoutChecker.Timeout = timeout;
        }











    }
}
