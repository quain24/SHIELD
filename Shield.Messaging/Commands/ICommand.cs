using Shield.Messaging.RawData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Commands
{
    public interface ICommand
    {
        IPart ID { get; }
        IPart HostID { get; }
        IPart Type {  get; }
        IPart Data {  get; }

    }
}
