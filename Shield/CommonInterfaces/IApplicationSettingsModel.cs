namespace Shield.CommonInterfaces
{
    public interface IApplicationSettingsModel : ISetting
    {
        int DataSize { get; set; }
        int IdSize { get; set; }
        int CommandTypeSize { get; set; }
        char Separator { get; set; }
        char Filler { get; set; }
        int HostIdSize { get; set; }
        string HostId { get; set; }
    }
}