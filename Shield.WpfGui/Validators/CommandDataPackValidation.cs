using System.Collections.Generic;
using System.Linq;

namespace Shield.WpfGui.Validators
{
    public class CommandDataPackValidation
    {
        private char _filler;
        private char _separator;

        public CommandDataPackValidation(char separator, char filler)
        {
            _separator = separator;
            _filler = filler;
        }

        public bool ValidateDataPack(string input, out ICollection<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (input is null || input.Length == 0)
                return true;
            if (input.Contains(_filler))
            {
                validationErrors.Add($@"There cannot be a 'filler' ({_filler}) symbol inside data pack.");
            }

            if (input.Contains(_separator))
            {
                validationErrors.Add($@"There cannot be a 'separator' ({_separator}) symbol inside data pack.");
            }

            if (input.Contains(" "))
            {
                validationErrors.Add($@"There cannot be a 'white space' symbol inside data pack.");
            }
            return validationErrors.Count == 0;
        }
    }
}