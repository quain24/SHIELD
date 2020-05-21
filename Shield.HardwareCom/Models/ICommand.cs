using Shield.HardwareCom.Enums;
using System;

namespace Shield.HardwareCom.Models
{
    public interface ICommand : IEquatable<ICommand>
    {
        CommandType CommandType { get; }
        string HostId { get; }
        string Id { get; }
        string Data { get; }
        long TimeStamp { get; }
    }
}