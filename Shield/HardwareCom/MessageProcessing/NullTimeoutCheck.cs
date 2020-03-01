using Shield.HardwareCom.Models;

namespace Shield.HardwareCom.MessageProcessing
{
    internal class NullTimeoutCheck : ITimeoutCheck
    {
        private readonly long _timeout = 0;
        private static NullTimeoutCheck _instance = null;
        private static readonly object _lock = new object();

        private NullTimeoutCheck()
        {
        }
        // TODO - add timeoutCheck factory class and register it in autofac - use only it for getting objects of TimeoutCheck type.
        // It will guarantee null object pattern compliance

        /// <summary>
        /// Null object pattern implementation of ITimeoutCheck interface
        /// </summary>
        /// <returns>Instance of NullTimeoutCheck object (singleton)</returns>
        public NullTimeoutCheck GetInstance()
        {
            if (_instance is null)
                lock (_lock)
                    if (_instance is null)
                        _instance = new NullTimeoutCheck();
            return _instance;
        }

        /// <summary>
        /// Returns timeout value for this instance
        /// </summary>
        public long Timeout => _timeout;

        /// <summary>
        /// Will always return false, its a null pattern implementation dummy object
        /// </summary>
        /// <param name="message">Message in which timeout is checked</param>
        /// <param name="inCompareTo">Optional message that main message timeout will be checked against</param>
        /// <returns></returns>
        public virtual bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null) => false;
    }
}