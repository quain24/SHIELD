using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationManager
{
    public class TestClass : ITestClass
    {
        public void Test()
        {
            Console.WriteLine($@"Wiadomość testowa");
        }
    }
}
