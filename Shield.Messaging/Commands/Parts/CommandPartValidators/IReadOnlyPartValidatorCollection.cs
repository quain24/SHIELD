using System.Collections.Generic;

namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public interface IReadOnlyPartValidatorCollection : IEnumerable<IPartValidator>
    {
        IPartValidator GetDefaultValidator();

        IPartValidator GetValidatorForOrDefault(Shield.Command.PartType type);
    }
}