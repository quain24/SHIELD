using Shield.Messaging.Commands.States;
using Shield.Messaging.Protocol;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Shield.Messaging.Invoker
{
    public class Invoker
    {
        private readonly ProtocolHandler _handler;
        private readonly IDictionary<string, (Delegate method, bool isGeneratingReply)> _buffer = new Dictionary<string, (Delegate callingMethod, bool isGeneratingReply)>();

        public Invoker(ProtocolHandler handler)
        {
            _handler = handler;
        }

        public void RegisterInvokableMethod(Func<Order, Task<Reply>> method)
        {
        }

        public void RegisterInvokableMethod(Func<Order, Reply> method)
        {
        }

        public void RegisterInvokableMethod(Func<Task<Reply>> method)
        {
        }

        public void RegisterInvokableMethod(Func<Task> method)
        {
        }

        public void RegisterInvokableMethod(Action<Order> method)
        {
        }

        public void RegisterInvokableMethod(Action method)
        {
        }

        //public async Task NonReplyingHandler<T, U>(T order, Func<T, Task<U>> executingMethod) where T : Order where U : IConvertible
        //{
        //    string rawReplyData = string.Empty;

        //    var d = await executingMethod.Invoke(order).ConfigureAwait(false);

        //    var reply = Reply.Create(order.Target, new StringDataPack(order.Data));
        //    await _handler.SendAsync(reply).ConfigureAwait(false);
        //    if (await _handler.Order().WasConfirmedInTimeAsync(order).ConfigureAwait(false))

        //}

        private void RegisterMethod<T>(T methodCall, bool isReplying) where T : Delegate
        {
            _ = methodCall ??
                throw new ArgumentNullException(nameof(methodCall), "Passed null instead of proper delegate.");

            if (_buffer.ContainsKey(methodCall.GetMethodInfo().Name))
                throw new ArgumentOutOfRangeException(nameof(methodCall), $"Method {methodCall.GetMethodInfo().Name} was already registered in this {nameof(Invoker)}. Overloading is not supported.");

            _buffer.Add(methodCall.GetMethodInfo().Name, (methodCall, isReplying));
        }

        public async Task Invoke(Order order)
        {
            _ = order ?? throw new ArgumentNullException(nameof(order),
                "Tried to pass a null object instead of correct order for invocation.");

            switch (_buffer[order.Target].method)
            {
                case Action target:
                    InvokeRegisteredAction(target);
                    break;

                case Action<string> target:
                    InvokeRegisteredAction(target, order.Data);
                    break;

                case Func<string> target:
                    InvokeRegisteredFunc(target);
                    break;

                case Func<Task<string>> target:
                    await InvokeRegisteredFuncAsync(target).ConfigureAwait(false);
                    break;

                case Func<Task> target:
                    await InvokeRegisteredFuncAsync(target).ConfigureAwait(false);
                    break;

                case Func<string, string> target:
                    InvokeRegisteredFunc(target, order.Data);
                    break;

                case Func<string, Task<string>> target:
                    //await InvokeRegisteredFunc(target, order.Data);
                    break;

                case null:
                    await HandleMissingMethodAsync(order).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentException($"Tried to invoke method type that has no handler in this {nameof(Invoker)} instance.");
            }
        }

        private Task InvokeRegisteredFuncAsync(Func<Task> target)
        {
            return target?.Invoke();
        }

        private async Task InvokeRegisteredFuncAsync(Func<Task<string>> target, Order order)
        {
            var replyData = await target.Invoke().ConfigureAwait(false);
            //await _handler.SendAsync(Reply.Create(order.ID, new StringDataPack(replyData))).ConfigureAwait(false);
        }

        private Task<bool> HandleMissingMethodAsync(Order order)
        {
            var confirmation = Confirmation.Create(order.ID, ErrorState.Unchecked().OrderDoesNotExist());
            return _handler.SendAsync(confirmation);
        }

        private void InvokeRegisteredAction(Action action)
        {
            action?.Invoke();
        }

        private void InvokeRegisteredAction(Action<string> action, string data)
        {
            action?.Invoke(data);
        }

        private void InvokeRegisteredFunc(Func<string> methodCall)
        {
            var replyData = methodCall?.Invoke();
        }

        private void InvokeRegisteredFunc(Func<string, bool> methodCall, string data)
        {
            methodCall?.Invoke(data);
        }

        private void InvokeRegisteredFunc(Func<string, string> methodCall, string data)
        {
            methodCall?.Invoke(data);
        }
    }
}