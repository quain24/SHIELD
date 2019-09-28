using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Extensions
{
    public static class StringExtension
    {
        public static string RemoveASCIIChars(this string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (c >= 32 && c <= 175)
                    sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
