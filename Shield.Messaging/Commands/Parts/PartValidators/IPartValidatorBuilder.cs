namespace Shield.Messaging.Commands.Parts.PartValidators
{
    public interface IPartValidatorBuilder
    {
        IPartValidatorBuilder AllowOnlyAlphaNumeric();

        IPartValidatorBuilder AllwaysValidateAsBad();

        IPartValidatorBuilder AllwaysValidateAsGood();

        IPartValidatorBuilder ForbidChars(params char[] forbiddenCharacters);

        IPartValidatorBuilder MaximumLength(int length);

        IPartValidatorBuilder MinimumLength(int length);

        IPartValidatorBuilder Reset();

        IPartValidator Build();
    }
}