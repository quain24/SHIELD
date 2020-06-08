using Shield.Messaging.Modules.PartValidators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shield.Messaging.Modules
{
    public class ModuleInfoCollection : IModuleInfoCollection
    {
        private readonly IEnumerable<IModuleInfo> _moduleInfos;

        public ModuleInfoCollection(IEnumerable<IModuleInfo> moduleInfos)
        {
            _moduleInfos = moduleInfos;
        }

        public bool Contains(string name)
        {
            return _moduleInfos.Any(mi => mi.Name == name);
        }

        public IModuleInfo GetByName(string name) => _moduleInfos.FirstOrDefault(mi => mi.Name == name);
    }
}
