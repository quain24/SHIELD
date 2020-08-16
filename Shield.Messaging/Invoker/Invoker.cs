using Shield.Messaging.Commands.States;
using Shield.Messaging.Protocol;
using System;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

namespace Shield.Messaging.Invoker
{
    public class Invoker
    {
        private readonly ProtocolHandler _handler;
        private readonly Hashtable _buffer = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        public Invoker(ProtocolHandler handler)
        {
            _handler = handler;
        }

        public void RegisterReplyingMethod<T>(T methodCall) where T : Delegate
        {
            if (methodCall?.Method.ReturnType == typeof(void))
                throw new ArgumentOutOfRangeException(nameof(methodCall), "Cannot register method without return type as a replying type.");
            RegisterMethod(methodCall);
        }

        public void RegisterMethod<T>(T methodCall) where T : Delegate
        {
            _ = methodCall ??
                throw new ArgumentNullException(nameof(methodCall), "Passed null instead of proper delegate.");
            if(_buffer.ContainsKey(methodCall.GetMethodInfo().Name))
                throw new ArgumentOutOfRangeException(nameof(methodCall), $"Method {methodCall.GetMethodInfo().Name} was already registered in this {nameof(Invoker)}.");

            _buffer.Add(methodCall.GetMethodInfo().Name, methodCall);
        }

        private T GetMethodInvoker<T>(Type type) where T : Delegate
        {
            return _buffer[type] as T;
        }

        public async Task Invoke(Order order)
        {
            _ = order ?? throw new ArgumentNullException(nameof(order),
                "Tried to pass a null object instead of correct order for invocation.");

            switch (_buffer[order.Target])
            {
                case Action target:
                    InvokeRegisteredAction(target);
                    break;

                case Action<string> target:
                    InvokeRegisteredAction(target, order.Data);
                    break;

                case Func<string, bool> target:
                    InvokeRegisteredFunc(target, order.Data);
                    break;

                case Func<Task<string>> target:
                    await InvokeRegisteredFuncAsync(target, order).ConfigureAwait(false);
                    break;

                case Func<Task> target:
                    await InvokeRegisteredFuncAsync(target).ConfigureAwait(false);
                    break;

                case Func<string, string> target:
                    InvokeRegisteredFunc(target, order.Data);
                    break;

                case null:
                    await HandleMissingMethodAsync(order).ConfigureAwait(false);
                    break;

                default:
                    throw new ArgumentException($"Tried to invoke method that has no handler in this {nameof(Invoker)} instance.");
            }
        }

        private Task InvokeRegisteredFuncAsync(Func<Task> target)
        {
            return target?.Invoke();
        }

        private async Task InvokeRegisteredFuncAsync(Func<Task<string>> target, Order order)
        {
            var replyData = await target.Invoke().ConfigureAwait(false);
            await _handler.SendAsync(Reply.Create(order.ID, replyData)).ConfigureAwait(false);
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