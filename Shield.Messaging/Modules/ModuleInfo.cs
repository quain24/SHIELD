using Shield.Messaging.Modules.PartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Modules
{
    public class ModuleInfo : IModuleInfo
    {
        private readonly IEnumerable<string> _orders;

        public ModuleInfo(string name, IEnumerable<string> orders)
        {
            Name = name;
            _orders = orders;
        }
        public string Name { get; }

        public bool ContainsOrder(string orderType)
        {
            return _orders.Contains(orderType);
        }
    }
}
