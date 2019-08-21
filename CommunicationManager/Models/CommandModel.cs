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
        private Type _type;
        private string _command;

        public CommandModel(Type commandType)
        {
            _type = commandType;
            Command = "";
        }

        public CommandModel(Type commandType, string command = "")
        {
            _type = commandType;
            Command = command;
        }
        
        public string Command
        {
            get { return _command; }
            set { _command = EncodeToASCII(value); }
        }


        public string GetMessage()
        {
            return _command;
        }
        public enum Type
        {
            HandShake,
            Confirm,
            Sending,
            StartSending,
            StopSending,
            Receiving,
            StartReceiving,
            StopReceiving,
            Completed,
            Correct,
            Error,
            Data
        }

        private string EncodeToASCII(string message)
        {
            byte[] bytes = Encoding.Default.GetBytes(message);
            return  Encoding.ASCII.GetString(bytes);
        }

        private string TranslateToMashineCommand(Type type)
        {
            // TODO or property!
            return "";
        }

        // TODO or co powyzej
        public Type CommandType
        {
            get;
            set;
        }
    }
}
