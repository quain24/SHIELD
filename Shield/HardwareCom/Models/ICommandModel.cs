﻿using Shield.Enums;

namespace Shield.HardwareCom.Models
{
    public interface ICommandModel : System.IEquatable<ICommandModel>
    {
        long TimeStamp { get; set; }
        string Id { get; set; }
        CommandType CommandType { get; set; }
        string Data { get; set; }
        string CommandTypeString { get; }
    }
}