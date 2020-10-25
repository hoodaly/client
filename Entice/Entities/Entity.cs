using Entice.Debugging;
using GuildWarsInterface.Datastructures.Agents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Entice.Entities
{
    internal abstract class Entity
    {
        public static readonly Dictionary<Guid, Entity> Entities = new Dictionary<Guid, Entity>();

        public Guid Id { get; private set; }

        public static List<Player> Players
        {
            get { return Entities.Values.OfType<Player>().ToList(); }
        }

        protected abstract void UpdateAttribute(string name, dynamic value);

        public static void UpdateEntity(Guid id, string attribute, dynamic value)
        {
            Entity entity;
            if (!Entities.TryGetValue(id, out entity))
            {
                entity = CreateEntity<UnknownEntity>(id);
            }

            if (entity is UnknownEntity)
            {
                entity = TryToSpecifyType(entity, attribute);
            }

            entity.UpdateAttribute(attribute, value);
        }

        private static Entity TryToSpecifyType(Entity entity, string property)
        {
            switch (property)
            {
                case "appearance":
                    return GetEntity<Player>(entity.Id);

                case "npc":
                    return GetEntity<Npc>(entity.Id);

                case "item":
                    return GetEntity<ItemEntity>(entity.Id);

                default:
                    return entity;
            }
        }

        /// <summary>
        /// Gets an Entity id by an Agent (by. Name)
        /// </summary>
        /// <param name="agent"></param>
        /// <returns>If found the Id of the Entity. Otherwise <see cref="Guid.Empty"/></returns>
        public static Guid GetIdOfAgent(Agent agent)
        {
            Guid entityId;
            entityId = GetIdOfEntityWith<Player>(player => player.Character.Name == agent.Name);
            if (entityId != Guid.Empty)
            {
                return entityId;
            }
            entityId = GetIdOfEntityWith<Npc>(npc => npc.Character.Name == agent.Name);
            if (entityId != Guid.Empty)
            {
                return entityId;
            }
            entityId = GetIdOfEntityWith<ItemEntity>(item => item.DroppedItem == agent);
            if (entityId != Guid.Empty)
            {
                return entityId;
            }

            return Guid.Empty;
        }

        public static Guid GetIdOfEntityWith<T>(Func<T, bool> entitySelector) where T : Entity
        {
            Dictionary<Guid, T> entities =
                Entities.Where(x => (x.Value as T) != null).Select(y => y.Value as T).Where(entitySelector).ToDictionary(p => p.Id);
            
            Guid entityId;
            
            entityId = entities.FirstOrDefault().Key;
            if (entityId != default) return entityId;

            return Guid.Empty;
        }

        public static Creature GetCreature(Guid entityId)
        {
            Type typeOfEntity = Entity.Entities[entityId].GetType();
            if (typeOfEntity == typeof(Player))
            {
                return Entity.GetEntity<Player>(entityId).Character;
            }
            else if (typeOfEntity == typeof(Npc))
            {
                return Entity.GetEntity<Npc>(entityId).Character;
            }

            return default(Creature);
        }

        private static T CreateEntity<T>(Guid id) where T : Entity, new()
        {
            var result = new T { Id = id };
            Entities.Add(result.Id, result);
            result.Initialized();
            return result;
        }

        public static T GetEntity<T>(Guid id) where T : Entity, new()
        {
            T result = null;

            Entity entity;
            if (Entities.TryGetValue(id, out entity))
            {
                if (entity is T)
                {
                    result = (T)entity;
                }
                else
                {
                    var unknownEntity = entity as UnknownEntity;

                    if (unknownEntity != null)
                    {
                        Console.WriteLine("upgrading unknown entity to entity of type {0}", typeof(T));

                        Entities.Remove(entity.Id);
                        result = CreateEntity<T>(entity.Id);
                        unknownEntity.Attributes.ToList().ForEach(a => result.UpdateAttribute(a.Key, a.Value));
                    }
                    else
                    {
                        Debug.Error("requested entity {0} of type {1} as entity of type {2}", entity.Id, entity.GetType(), typeof(T));
                    }
                }
            }
            else
            {
                result = CreateEntity<T>(id);
            }

            return result;
        }

        protected abstract void Initialized();

        public static void RemoveEntity(Guid id)
        {
            Entity entity;
            if (!Entities.TryGetValue(id, out entity)) return;

            entity.Unload();
            Entities.Remove(entity.Id);
        }

        protected abstract void Unload();

        public static Player Reset(Guid playerCharacterId)
        {
            Entities.Clear();

            return CreateEntity<Player>(playerCharacterId);
        }

        private sealed class UnknownEntity : Entity
        {
            internal readonly Dictionary<string, object> Attributes;

            public UnknownEntity()
            {
                Attributes = new Dictionary<string, dynamic>();
            }

            protected override void UpdateAttribute(string name, dynamic value)
            {
                if (Attributes.ContainsKey(name))
                {
                    Attributes[name] = value;
                }
                else
                {
                    Attributes.Add(name, value);
                }
            }

            protected override void Initialized()
            {
            }

            protected override void Unload()
            {
            }
        }
    }
}
