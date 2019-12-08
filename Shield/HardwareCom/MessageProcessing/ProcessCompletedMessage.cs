using Shield.HardwareCom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shield.HardwareCom.MessageProcessing;

namespace Shield.HardwareCom.MessageProcessing
{
    public class ProcessCompletedMessage
    {
        IPattern _pattern;
        IDecoding _decoding;
        ITypeDetector _typeDetector;

        public ProcessCompletedMessage(IPattern pattern, IDecoding decoding, ITypeDetector typeDetector)
        {
            _pattern = pattern;
            _decoding = decoding;
            _typeDetector = typeDetector;
        }
        public IMessageHWComModel Process(IMessageHWComModel message)
        {            
            if(_pattern.IsCorrect(message) == false)
                message.Errors = message.Errors | Enums.Errors.BadMessagePattern;

            Enums.Errors decodingErrors = _decoding.Check(message);
            if(decodingErrors != Enums.Errors.None)
                message.Errors = message.Errors | decodingErrors;

            message.Type = _typeDetector.DetectTypeOf(message);
            if(message.Type == Enums.MessageType.Unknown)
                message.Errors = message.Errors | Enums.Errors.UndeterminedType;

            return message;
        }
    }
}
