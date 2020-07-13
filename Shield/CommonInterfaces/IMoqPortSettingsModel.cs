namespace Shield.CommonInterfaces
{
    public interface IMoqPortSettingsModel : ICommunicationDeviceSettings
    {
        int PortNumber { get; set; }
    }
}