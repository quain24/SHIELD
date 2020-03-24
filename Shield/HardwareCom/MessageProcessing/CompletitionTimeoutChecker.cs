using Shield.HardwareCom.MessageProcessing;
using Shield.HardwareCom.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shield.HardwareCom.MessageProcessing
{
    public class CompletitionTimeoutChecker : ICompletitionTimeoutChecker
    {
        private readonly ICommandIngesterAlt _ingesterToWorkWith;
        private readonly ITimeoutCheck _completitionTimeoutChecker;
        private readonly ConcurrentDictionary<string, IMessageModel> _workingSet;

        private bool _isTimeoutChecking = false;
        private int _parallelTreshold = 250;

        private readonly object _timeoutCheckLock = new object();

        private CancellationTokenSource _cancelTimeoutCheckCTS = new CancellationTokenSource();

        public CompletitionTimeoutChecker(ICommandIngesterAlt ingesterToWorkWith, ITimeoutCheck completitionTimeoutChecker)
        {
            _ingesterToWorkWith = ingesterToWorkWith ?? throw new ArgumentNullException(nameof(ingesterToWorkWith));
            _completitionTimeoutChecker = completitionTimeoutChecker ?? throw new ArgumentNullException(nameof(completitionTimeoutChecker));
            _workingSet = _ingesterToWorkWith.GetIncompletedMessages();
        }

        private void MarkAsTimeoutAndComplete(IMessageModel message)
        {
            message.Errors |= Enums.Errors.CompletitionTimeout;
            message.IsCompleted = true;
            _ingesterToWorkWith.PushFromIncompleteToProcessed(message);
        }

        public async Task StartTimeoutCheckAsync()
        {
            try
            {
                if (!CanStartTiemoutCheck())
                    return;

                int checkInterval = CalculateCheckInterval(_completitionTimeoutChecker.Timeout);

                while (true)
                {
                    _cancelTimeoutCheckCTS.Token.ThrowIfCancellationRequested();
                    ProcessTimeouts();
                    await Task.Delay(checkInterval, _cancelTimeoutCheckCTS.Token).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                if (!IsTimeoutCheckCorrectlyCancelled(e))
                    throw;
            }
        }

        private bool CanStartTiemoutCheck()
        {
            if (_completitionTimeoutChecker.Timeout == 0)
                return false;

            lock (_timeoutCheckLock)
            {
                if (!_isTimeoutChecking)
                    return _isTimeoutChecking = true;
                else
                    return false;
            }
        }

        private int CalculateCheckInterval(int timeout)
        {
            switch (timeout)
            {
                case var t when t <= 1000:
                    return 250;

                case var t when t <= 3000:
                    return 500;

                case var t when t >= 5000:
                    return 1000;

                default:
                    return _completitionTimeoutChecker.Timeout;
            }
        }

        private void ProcessTimeouts()
        {
            if (_workingSet.Count > _parallelTreshold)
                HandleTimeoutsParallel(GetlistOfUnconfirmedMessagesParallel());
            else
                HandleTimeouts(GetlistOfUnconfirmedMessages());
        }

        private List<IMessageModel> GetlistOfUnconfirmedMessages()
        {
            return _workingSet
                 .Select(kvp => kvp.Value)
                 .Where(m => _completitionTimeoutChecker.IsExceeded(m))
                 .ToList();
        }

        private List<IMessageModel> GetlistOfUnconfirmedMessagesParallel()
        {
            return _workingSet
                 .AsParallel()
                 .Select(kvp => kvp.Value)
                 .Where(m => _completitionTimeoutChecker.IsExceeded(m))
                 .ToList();
        }

        private void HandleTimeoutsParallel(List<IMessageModel> listOfTimeouts)
        {
            listOfTimeouts.AsParallel().ForAll(m => HandleMessageTimeout(m));
        }

        private void HandleTimeouts(List<IMessageModel> listOfTimeouts)
        {
            listOfTimeouts.ForEach(m => HandleMessageTimeout(m));
        }

        private bool IsTimeoutCheckCorrectlyCancelled(Exception e)
        {
            lock (_timeoutCheckLock)
                _isTimeoutChecking = false;

            return (e is TaskCanceledException || e is OperationCanceledException) ? true : false;
        }

        public void StopTimeoutCheck()
        {
            _cancelTimeoutCheckCTS.Cancel();
            _cancelTimeoutCheckCTS = new CancellationTokenSource();
        }

        private void HandleMessageTimeout(IMessageModel message)
        {
            if (message is null) return;
            MarkAsTimeoutAndComplete(message);
        }
    }
}