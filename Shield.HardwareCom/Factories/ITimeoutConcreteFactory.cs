using Shield.HardwareCom.MessageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.Factories
{
    public interface ITimeoutConcreteFactory
    {
        ITimeout CreateTimeoutWith(int milliseconds);
    }
}
