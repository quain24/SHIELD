namespace Shield.Messaging.Commands.Parts.CommandPartValidators
{
    public interface IValidatorAssingnmentFactory
    {
        IReadOnlyPartValidatorCollection GetValidatorAssignments();
    }
}