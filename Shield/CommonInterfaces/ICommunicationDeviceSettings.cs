namespace Shield.CommonInterfaces
{
    public interface ICommunicationDeviceSettings : ISetting
    {
        string Name { get; }
        int CompletitionTimeout { get; set; }
        int ConfirmationTimeout { get; set; }
    }
}