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
    }
}