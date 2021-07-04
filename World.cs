using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECS;

namespace ECS
{
    public class World
    {
        public Dictionary<Guid, IComponent[]> Entities = new Dictionary<Guid, IComponent[]>();

        public Dictionary<Type, System> Systems = new Dictionary<Type, System>();

        public World()
        {

        }

        public Guid CreateEntity(IComponent[] components)
        {
            Guid id = Guid.NewGuid();
            this.Entities[id] = components;
            return id;
        }

        public IComponent[] GetComponents(Guid entity)
        {
            IComponent[] components;

            if (!this.Entities.TryGetValue(entity, out components))
            {
                throw new Exception("Entity doesn't exist");
            }

            return components;
        }

        public Nullable<T> GetComponentByType<T>(Guid entity)
        {
            var components = this.GetComponents(entity);

            foreach (var component in components)
            {
                if (component.GetType() == typeof(T))
                {
                    return (T?)component;
                }
            }

            return null;
        }

        public void RegisterSystem(System system)
        {
            var type = system.GetType();

            if (this.Systems.ContainsKey(type))
            {
                throw new Exception("System already registered");
            }

            this.Systems[type] = system;
        }

        public void ReplaceComponent(Guid entity, IComponent component)
        {
            var components = this.GetComponents(entity);

            components.Append(component);

            this.Entities[entity] = components;
        }

        public void Run(int delta, int time)
        {
            foreach (var system in this.Systems)
            {
                foreach (var entity in this.Entities)
                {
                    var components = entity.Value;
                    var isMatch = system.Value.Match(components);
                    if (!isMatch) continue;
                    system.Value.Run(entity, delta, time);
                }
            }
        }
    }
}
