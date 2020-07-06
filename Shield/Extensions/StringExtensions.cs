using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shield.Extensions
{
    public static class StringExtensions
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

        /// <summary>
        /// Splits given <see cref="string"/> into <see cref="IEnumerable{String}">IEnumerable&lt;string&gt;</see> collection
        /// using provided <see cref="char"/>.
        /// </summary>
        /// <param name="value"><see cref="string"/> to be splitted</param>
        /// <param name="splitter"><see cref="char"/> with which given <see cref="string"/> will be splitted</param>
        /// <returns><see cref="IEnumerable{String}">IEnumerable&lt;string&gt;</see> filled with splitted substrings or empty one if none could be extracted</returns>
        public static IEnumerable<string> SplitBy(this string value, char splitter)
        {
            return value?.Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        }

        /// <summary>
        /// Checks if given <paramref name="data"/> is a alphanumerical string
        /// </summary>
        /// <param name="data">String to be checked</param>
        /// <returns>True if checked string is alphanumerical</returns>
        public static bool IsAlphanumeric(this string data) => !data.Any(c => !char.IsLetterOrDigit(c) || c >= 128); // Char equal or higher than 128 is out of ASCII spec
    }
}