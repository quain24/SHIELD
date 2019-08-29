using Shield.HardwareCom.Enums;

namespace Shield.HardwareCom.Models
{
    public interface ICommandModel
    {
        CommandType CommandType { get; set; }
        string Data { get; set; }
        string CommandTypeString { get; }
    }
}