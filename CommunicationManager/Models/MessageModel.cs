using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationManager.Models
{
    public class MessageModel : IMessageModel
    {
        private ICommandModel _commandModel;


        //public MessageModel()
        //{
        //}

        //public MessageModel(params ICommandModel[] commands)
        //{
        //    //_commandModel = commands[0];
        //}

        public MessageModel(ICommandModel command)
        {
            _commandModel = command;
        }

        public string GetACommand()
        {
            return _commandModel.GetMessage();
        }
    }
}
