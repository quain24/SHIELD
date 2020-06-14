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
            Target,
            Order,
            Type,
            Data
        }

        public enum PartValidator
        {
            Default,
            IDPart,
            HostIDPart,
            TargetPart,
            OrderPart,
            TypePart,
            DataPart
        }
    }
}