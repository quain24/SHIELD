using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunicationManager;

namespace TemporaryConsoleUI
{
    public class Application : IApplication
    {
        ITestClass _testClass;

        public Application(ITestClass testClass)
        {
            _testClass = testClass;
        }
        public void Run()
        {
            _testClass.Test();
        }
    }
}
