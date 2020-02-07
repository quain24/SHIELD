using Shield.HardwareCom.Models;
using System.Collections.Generic;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface ITimeoutCheck
    {
        long Timeout { get; set; }
        int NoTimeoutValue { get; }

        List<T> GetTimeoutsFromCollection<T>(IEnumerable<T> source) where T : IMessageModel;
        List<T> GetTimeoutsFromCollection<T>(Dictionary<string, T> source) where T : IMessageModel;
        bool IsExceeded(IMessageModel message, IMessageModel inCompareTo = null);
    }
}