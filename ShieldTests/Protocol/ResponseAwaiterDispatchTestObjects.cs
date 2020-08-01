using System.Collections.Generic;
using Shield.Messaging.Commands;
using Shield.Messaging.Protocol;
using Shield.Timestamps;

namespace ShieldTests.Protocol
{
    public static class ResponseAwaiterDispatchTestObjects
    {
        public static Dictionary<ResponseType, ResponseAwaiter> GetProperAwaitersDictionary()
        {
            var responseAwaiterDictionary = new Dictionary<ResponseType, ResponseAwaiter>();
            responseAwaiterDictionary.Add(ResponseType.Confirmation, ResponseAwaiter.GetNewInstance(new Timeout(ProtocolTestObjects.ConfirmationTimeout)));
            responseAwaiterDictionary.Add(ResponseType.Reply, ResponseAwaiter.GetNewInstance(new Timeout(ProtocolTestObjects.ReplyTimeout)));

            return responseAwaiterDictionary;
        }

        public static Dictionary<ResponseType, ResponseAwaiter> GetIAwatersDictionaryWithoutReply()
        {
            var output = new Dictionary<ResponseType, ResponseAwaiter>(GetProperAwaitersDictionary());
            output.Remove(ResponseType.Reply);
            return output;
        }

        public static IResponseMessage GetResponseMessageOfUnknownType()
        {
            return new UnknownTypeResponseMessage();
        }

        class UnknownTypeResponseMessage : IResponseMessage
        {
            public string Target => "UnknownTypeResponseTarget";
            public Timestamp Timestamp => TimestampFactory.Timestamp;
        }

    }
}