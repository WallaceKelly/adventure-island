using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureIsland
{
    public interface INamed
    {
        GameName Name { get; set; }
    }

    [Serializable]
    public class SingletonNameFactory<T> where T : INamed, new()
    {
        protected Dictionary<GameName, T> existingObjects = new Dictionary<GameName, T>();
        
        public virtual T CreateInstance(GameName name)
        {
            if(existingObjects.ContainsKey(name))
                throw new ArgumentException("An instance named {0} has already been created.", name.ToString());

            T newObject = new T();
            newObject.Name = name;
            existingObjects[name] = newObject;
            return newObject;
        }

        public virtual T GetInstance(GameName name)
        {
            if (!existingObjects.ContainsKey(name))
                throw new ArgumentException("An instance named {0} has not been created.", name.ToString());
            return existingObjects[name];
        }

        public virtual bool Exists(GameName name)
        {
            return existingObjects.ContainsKey(name);
        }

        public virtual GameName[] GetKeys()
        {
            List<GameName> keys = new List<GameName>();
            foreach (GameName s in existingObjects.Keys)
                keys.Add(s);
            return keys.ToArray();
        }
    }
}
