namespace ShieldTests.Messaging
{
    public static class DefaultProtocolValues
    {
        public static char Splitter => '#';
        public static char Separator => '*';
        public static int IDLength => 4;

        public static int ConfirmationTimeout = 1000;
        public static int ReplyTimeout = 1000;
    }
}