using Shield.Commands.Parts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shield.Command;

namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public class DefaultValidatorAssingnmentFactory : IValidatorAssingnmentFactory
    {
        private readonly int _idLength;
        private readonly int _hostIDlength;
        private readonly int _typePartLength;
        private readonly int _dataPartLength;
        private readonly char[] _dataPartForbiddenChars;

        public DefaultValidatorAssingnmentFactory(int idLength, int hostIDlength, int typePartLength, int dataPartLength, params char[] dataPartForbiddenChars)
        {
            _idLength = idLength;
            _hostIDlength = hostIDlength;
            _typePartLength = typePartLength;
            _dataPartLength = dataPartLength;
            _dataPartForbiddenChars = dataPartForbiddenChars;
        }

        public IReadOnlyPartValidatorCollection GetValidatorAssignments()
        {
            var output = new Dictionary<PartType, IPartValidator>()
            {
                [PartType.ID] = new AllAlphanumericAllowedValidator(_idLength),
                [PartType.HostID] = new AllAlphanumericAllowedValidator(_hostIDlength),
                [PartType.Type] = new AllAlphanumericAllowedValidator(_typePartLength),
                [PartType.Data] = new DataPartValidator(_dataPartLength, _dataPartForbiddenChars),
                [PartType.Empty] = new AllwaysGoodValidator()
            };

            return new ReadOnlyPartValidatorCollection(output, new AllwaysGoodValidator());
        }
    }
}
