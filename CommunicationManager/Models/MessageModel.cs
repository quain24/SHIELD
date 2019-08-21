using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationManager.Models
{
    public class MessageModel : IMessageModel
    {
        


        public MessageModel()
        {
        }       

        public string GetACommand()
        {
            return "temp";
        }
    }
}
