using Shield.HardwareCom.MessageProcessing;
using System;

namespace Shield.HardwareCom
{
    public class Inherittest : IInherittest
    {
        private ITimeoutCheck _completition;
        private ITimeoutCheck _confirmation;

        public Inherittest(ITimeoutCheck completitionCheck, ITimeoutCheck confirmationCheck)
        {
            _completition = completitionCheck;
            _confirmation = confirmationCheck;
        }

        public void GetValues()
        {
            Console.WriteLine(_completition.Timeout);
            Console.WriteLine(_completition.NoTimeoutValue);
            _completition.Timeout = 100;
            Console.WriteLine(_completition.Timeout);
            _completition.Timeout = 8;
            Console.WriteLine(_completition.Timeout);
        }
    }
}