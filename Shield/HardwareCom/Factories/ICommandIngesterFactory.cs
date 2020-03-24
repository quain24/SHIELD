﻿using Shield.HardwareCom.MessageProcessing;
using Shield.Helpers;

namespace Shield.HardwareCom.Factories
{
    public interface ICommandIngesterFactory
    {
        ICommandIngesterAlt GetIngesterUsing(IIdGenerator idGenerator);
    }
}