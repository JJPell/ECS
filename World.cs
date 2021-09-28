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
        public Dictionary<int, IComponent[]> Entities = new Dictionary<int, IComponent[]>();

        public Dictionary<Type, System> Systems = new Dictionary<Type, System>();

        private int lastId = 0;

        public int CreateEntity(IComponent[] components)
        {
            var id = this.CreateId();
            this.Entities[id] = components;
            return id;
        }

        public bool IsEntity(int entity)
        {
            return this.Entities.ContainsKey(entity);
        }

        public bool ComponentsContainComponent<T>(IComponent[] components)
        {
            foreach (var component in components)
            {
                if (component.GetType() == typeof(T))
                {
                    return true;
                }
            }

            return false;
        }

        public IComponent[] GetComponents(int entity)
        {
            IComponent[] components;

            if (!this.Entities.TryGetValue(entity, out components))
            {
                throw new Exception("Entity doesn't exist");
            }

            return components;
        }

        /// <summary>
        /// Gets a component from an entiry by type
        /// </summary>
        /// <typeparam name="T">Component Type</typeparam>
        /// <param name="entity">Entity ID</param>
        /// <returns>A tuple with the first value being if the component exists on the entity and the second being the component</returns>
        public (bool, T) GetComponentByType<T>(int entity)
        {
            var components = this.GetComponents(entity);

            foreach (var component in components)
            {
                var type1 = component.GetType();
                var type2 = typeof(T);

                if (type1 == type2)
                {
                    return (true, (T)component);
                }
            }

            return (false, default(T));
        }

        /// <summary>
        /// Gets a component from an entiry by type
        /// </summary>
        /// <typeparam name="T">Component Type</typeparam>
        /// <param name="components">Components</param>
        /// <returns>A tuple with the first value being if the component exists on the entity and the second being the component</returns>
        public (bool, T) GetComponentByType<T>(IComponent[] components)
        {
            foreach (var component in components)
            {
                var type1 = component.GetType();
                var type2 = typeof(T);

                if (type1 == type2)
                {
                    return (true, (T)component);
                }
            }

            return (false, default(T));
        }

        public Dictionary<int, IComponent[]> GetEntitiesWithComponent<T>()
        {
            var entitiesWithComponent = new Dictionary<int, IComponent[]>();

            foreach (var item in Entities)
            {
                var components = item.Value;
                var containsComponent = ComponentsContainComponent<T>(components);

                if (containsComponent)
                {
                    entitiesWithComponent.Add(item.Key, components);
                }
            }

            return entitiesWithComponent;
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

        public void RemoveEntity(int entity)
        {
            Entities.Remove(entity);
        }

        public void ReplaceComponent(int entity, IComponent component)
        {
            IComponent[] components = this.GetComponents(entity);

            bool updatedComponent = false;

            for (int i = 0; i < components.Length; i++)
            {
                var iterationComponent = components[i];

                if (iterationComponent.GetType() == component.GetType())
                {
                    components[i] = component;
                    updatedComponent = true;
                    break;
                }
            }

            if (!updatedComponent)
            {
                components[components.Length] = component;
            }

            this.Entities[entity] = components;
        }

        public void ReplaceComponentsOfSameType<T>(int entity, IComponent[] replaceComponents)
        {
            var components = this.GetComponents(entity).ToList();
            components.RemoveAll((component) => component.GetType() == typeof(T));
            var concat = components.Concat(replaceComponents);
            this.Entities[entity] = (IComponent[])concat.ToArray();
        }

        public void Run(int delta, int time)
        {
            foreach (var system in this.Systems)
            {
                var entities = this.Entities.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value
                );

                foreach (var entity in entities)
                {
                    var components = entity.Value;
                    var isMatch = system.Value.Match(components);
                    if (!isMatch) continue;
                    system.Value.Run(entity, delta, time, this);
                }
            }
        }

        private int CreateId()
        {
            this.lastId++;
            return this.lastId;
        }
    }
}
