using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public class NullTimeout : ITimeout
    {
        private readonly int _timeout = 0;

        /// <summary>
        /// Null object pattern implementation of ITimeoutCheck interface
        /// </summary>
        /// <returns>Instance of NullTimeoutCheck object (singleton)</returns>
        public NullTimeout()
        {
        }

        /// <summary>
        /// Returns timeout value for this instance
        /// </summary>
        public int TimeoutValue => _timeout;

        /// <summary>
        /// Will always return false, its a null pattern implementation dummy object
        /// </summary>
        /// <param name="message">Message in which timeout is checked</param>
        /// <param name="inCompareTo">Optional message that main message timeout will be checked against</param>
        /// <returns></returns>
        public virtual bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null) => false;
    }
}