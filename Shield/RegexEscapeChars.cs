namespace Shield
{
    public static class RegexEscapeChars
    {
        public static string AddEscapeCharsTo(string data)
        {
            if (string.IsNullOrEmpty(data)) return string.Empty;

            return data.Replace(@"\", @"\\");
        }
    }
}