using Shield.HardwareCom.Enums;

namespace Shield.HardwareCom.Models
{
    public interface ICommandModel
    {
        long TimeStamp { get; set; }
        string HostId { get; set; }
        string Id { get; set; }
        CommandType CommandType { get; set; }
        string Data { get; set; }
        string CommandTypeString { get; }
    }
}