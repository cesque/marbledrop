using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarbleDropEditor
{
    public class ComponentDefinition
    {
        public string Type;
        public int? Width;
        public int? Height;
        public List<ComponentPortDefinition> Ports;

        public ComponentDefinition()
        {

        }
    }

    public class ComponentPortDefinition
    {
        public string Name;
        public string Type;
        public string ResourceType;
        public int X;
        public int Y;
    }
}
