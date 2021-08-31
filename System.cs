using System;
using System.Collections.Generic;
using System.Text;

namespace ECS
{
    public abstract class System
    {
        public abstract bool Match(IComponent[] entityComponents);
        public abstract void Run(KeyValuePair<int, IComponent[]> entity, int delta, int time, World world);
    }
}
