﻿using Shield.HardwareCom.Models;
using System.Collections.Concurrent;

namespace Shield.HardwareCom.MessageProcessing
{
    public interface IIncomingMessageProcessor
    {
        bool IsProcessingMessages { get; }

        void AddMessageToProcess(IMessageModel message);

        BlockingCollection<IMessageModel> GetProcessedMessages();

        void StartProcessingMessages();

        void StartProcessingMessagesContinous();

        void StopProcessingMessages();

        void SwitchSourceCollectionTo(BlockingCollection<IMessageModel> newSourceCollection);

        bool TryProcess(IMessageModel messageToProcess, out IMessageModel processedMessage);
    }
}