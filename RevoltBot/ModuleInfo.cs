using System;

namespace RevoltBot
{
    public class ModuleInfo
    {
        public Type Type;
        private string _name;

        public string Name
        {
            get
            {
                // todo
                return _name ?? Type.Name;
            }
            set
            {
                _name = value;
            }
        }
        public string Summary;
    }
}