using System.Collections.Generic;
using Shield.HardwareCom.Adapters;

namespace Shield.HardwareCom.Factories
{
    public interface IMoqAdapterFactory
    {
        List<string> AvailablePorts { get; }
        MoqAdapter GivePort { get; }

        bool Create(int portNumber = 5);
    }
}