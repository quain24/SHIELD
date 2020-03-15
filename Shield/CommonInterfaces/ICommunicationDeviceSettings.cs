namespace Shield.CommonInterfaces
{
    public interface ICommunicationDeviceSettings : ISetting
    {
        int CompletitionTimeout { get; set; }
        int ConfirmationTimeout { get; set; }
    }
}