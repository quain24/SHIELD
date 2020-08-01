using System;
using Xunit.Abstractions;

namespace ShieldTests.Extensions
{
    public static class IOutputHelperExtensions
    {
        public static void AddMessageFrom(this ITestOutputHelper helper, Exception exception, string additionalInfo = "")
        {
            var message = $"Given message: {exception?.Message ?? "Exception message was NULL - possibly exception was also NULL"}" +
                          $"\n\nException type: {exception?.GetType().ToString() ?? "NULL"} ";
            message = string.IsNullOrEmpty(additionalInfo) ? message : message + $"\n\nAdditional info: {additionalInfo}";

            helper.WriteLine(message);
        }
    }
}