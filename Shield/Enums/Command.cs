namespace Shield.Enums
{
    public static class Command
    {
        /// <summary>
        /// Enumeration containing all possible types of parts used for command creation
        /// </summary>
        public enum PartType
        {
            Empty,
            ID,
            HostID,
            Type,
            Data
        }

        public enum PartValidator
        {
            Default,
            IDPart,
            HostIDPart,
            TypePart,
            DataPart
        }
    }
}