﻿using Shield.HardwareCom.Enums;
using System.Collections.Generic;

namespace Shield.HardwareCom.Models
{
    public interface IMessageModel : IEnumerable<ICommandModel>
    {
        List<ICommandModel> Commands { get; }
        bool IsCompleted { get; set; }
        bool IsConfirmed { get; set; }
        bool IsCorrect { get; }
        Direction Direction { get; set; }
        Errors Errors { get; set; }
        string Id { get; set; }
        long Timestamp { get; set; }
        bool IsTransfered { get; set; }
        MessageType Type { get; set; }
        int CommandCount { get; }
        string HostId { get; set; }

        bool Add(ICommandModel command);

        string AssaignID(string id);

        IEnumerator<ICommandModel> GetEnumerator();

        bool Remove(ICommandModel command);

        bool Replace(ICommandModel target, ICommandModel replacement);
        void AssighHostID(string id);
    }
}