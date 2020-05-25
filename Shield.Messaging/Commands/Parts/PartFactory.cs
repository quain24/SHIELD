using Shield.Commands.Parts;
using Shield.Messaging.Commands.Parts.CommandPartValidators;
using static Shield.Command;

namespace Shield.Messaging.Commands.Parts
{
    public class PartFactory
    {
        private readonly IReadOnlyPartValidatorCollection _validatorAssignment;

        public PartFactory(IReadOnlyPartValidatorCollection validatorAssignment)
        {
            _validatorAssignment = validatorAssignment;
        }

        public IPart GetIDPart(string data) => new IDPart(data, GetValidatorFor(PartType.ID));

        public IPart GetHostIDPart(string data) => new HosIDPart(data, GetValidatorFor(PartType.HostID));

        public IPart GetTypePart(string data) => new TypePart(data, GetValidatorFor(PartType.Type));

        public IPart GetDataPart(string data) => new DataPart(data, GetValidatorFor(PartType.Data));

        public IPart GetEmptyPart() => new EmptyPart(GetValidatorFor(PartType.Empty));

        private IPartValidator GetValidatorFor(PartType part)
        {
            return _validatorAssignment.GetValidatorForOrDefault(part);
        }
    }
}