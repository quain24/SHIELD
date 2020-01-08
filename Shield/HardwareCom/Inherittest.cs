using Shield.HardwareCom.MessageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom
{
    public class Inherittest : IInherittest
    {

        ITimeoutCheck _completition;
        ITimeoutCheck _confirmation;


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
