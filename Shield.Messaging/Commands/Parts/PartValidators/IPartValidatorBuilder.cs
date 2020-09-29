namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public interface IPartValidatorBuilder
    {
        IPartValidatorBuilder AllowOnlyAlphaNumeric();

        IPartValidatorBuilder AlwaysValidateAsBad();

        IPartValidatorBuilder AlwaysValidateAsGood();

        IPartValidatorBuilder ForbidChars(params char[] forbiddenCharacters);

        IPartValidatorBuilder MaximumLength(int length);

        IPartValidatorBuilder MinimumLength(int length);

        IPartValidatorBuilder Reset();

        IPartValidator Build();
    }
}