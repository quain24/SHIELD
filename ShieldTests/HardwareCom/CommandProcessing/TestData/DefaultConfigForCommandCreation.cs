namespace ShieldTests.HardwareCom.CommandProcessing.TestData
{
    public class DefaultConfigForCommandCreation
    {
        public int CommandLength { get; } = 4;
        public int IDLength { get; } = 4;
        public int HostIDLength { get; } = 4;
        public string HostId { get; } = "unit";
        public int DataPackLength { get; } = 10;
        public char Separator { get; } = '*';
        public char Filler { get; } = '.';
    }
}