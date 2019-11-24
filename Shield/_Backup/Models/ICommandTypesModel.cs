using Shield.CommonInterfaces;
using System.Collections.Generic;

namespace Shield.Data.Models
{
    public interface ICommandTypesModel : ISetting
    {
        Dictionary<string, int> CommandTypes { get; }

        bool AddCommand(string command);
        bool RemoveCommand(string command);
    }
}