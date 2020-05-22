using Xunit;
using Shield.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Diagnostics;
using Shield.Commands;

namespace Shield.Messaging.Commands.Tests
{
    public class TimestampTests
    {
        [Fact()]
        public void testTest()
        {

            var a = new Timestamp(5);
            Timestamp b = new Timestamp(5);
            Assert.True(a.Equals(b));
        }

        [Fact]
        public void Timestamp_comparable_test_should_return_true_given_proper_assumtions()
        {
            var a = new Timestamp(20);
            var b = new Timestamp(20);
            var c = new Timestamp(10);
            var lis = new List<Timestamp>() { a, b, c, c};
            lis.Sort();
            lis.ForEach(cc => Debug.WriteLine(cc.ToString()));



            var list = new List<Timestamp>();

            Debug.WriteLine(TimestampFactory.Timestamp.ToString());

            for(int i = 0 ; i <= 10000 ; i++)
            {
                list.Add(TimestampFactory.Timestamp);
            }

            Debug.WriteLine(TimestampFactory.Timestamp.ToString());



            

            Assert.True(lis[2] == b );

            Assert.True(a == b);
            Assert.True(a > c);
            Assert.True(c < b);

        }
    }
}