using System.Reflection;
using System.Runtime.CompilerServices;

namespace Shield.Extensions
{
    public static class MethodInfoExtensions
    {
        public static bool IsAsync(this MethodInfo m)
        {
            var stateMachineAttr = m.GetCustomAttribute<AsyncStateMachineAttribute>();

            var stateMachineType = stateMachineAttr?.StateMachineType;

            return stateMachineType?.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        }
    }
}