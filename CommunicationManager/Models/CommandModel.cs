using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// l10x400x2x0x0n

namespace CommunicationManager.Models
{
    public class CommandModel : ICommandModel
    {
        string aa = "wiadomość z command model - oryginał";
        public CommandModel(string command = "zzz")
        {
            if(command != null && command != "")
            {
                aa = command;
            }
        }

        public string GetMessage()
        {
            return aa;
        }
    }
}
