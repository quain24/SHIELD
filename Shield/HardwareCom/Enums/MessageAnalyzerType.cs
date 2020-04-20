namespace Shield.HardwareCom.Enums
{
    /// <summary>
    /// Contains types of <see cref="Shield.HardwareCom.MessageProcessing.IMessageAnalyzer"/> available to choose from when analyzing <see cref="Shield.HardwareCom.Models.IMessageModel"/>
    /// </summary>
    public enum MessageAnalyzerType
    {
        TypeDetector,
        Pattern,
        Decoding
    }
}