﻿using Shield.HardwareCom.CommandProcessing;
using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.Factories
{
    public class CompletitionTimeoutCheckerFactory : ICompletitionTimeoutCheckerFactory
    {
        public ICompletitionTimeoutChecker GetCheckerUsing(ICommandIngester ingesterToWorkWith, ITimeout completitionTimeoutChecker) =>
            new CompletitionTimeoutChecker(ingesterToWorkWith, completitionTimeoutChecker);
    }
}