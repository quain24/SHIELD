using Xunit;
using Shield.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Commands.Tests
{
    public class TimestampTests
    {
        [Fact()]
        public void testTest()
        {

            var a = new Timestamp(5);
            ITimestamp b = new Timestamp(5);
            Assert.True(a.Equals(b));
        }

        [Fact()]
        public async Task IsExceededTest()
        {
            ITimestamp a = TimestampGenerator.Timestamp;
            await Task.Delay(10);
            ITimestamp timeout = new Timestamp(8);
            Assert.True(a.IsExceeded(timeout));
        }
    }
}