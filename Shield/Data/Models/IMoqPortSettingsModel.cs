namespace Shield.Data.Models
{
    public interface IMoqPortSettingsModel : ICommunicationDeviceSettings
    {
        int PortNumber { get; set; }
    }
}