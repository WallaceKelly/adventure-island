using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureIsland
{
    // Used to insure case insensitive comparisons
    // Is there better way?
    [Serializable]
    public class GameName
    {
        readonly string name;
        public GameName(string name)
        {
            this.name = name;
        }
        public override string ToString()
        {
            return name.ToString();
        }
        public override bool Equals(object obj)
        {
            GameName other = null;
            if (obj == null) return false;
            else if (obj is string) other = new GameName(obj as string);
            else if (obj is GameName) other = obj as GameName;
            else return false;
            return name.ToLower().Trim().Equals(other.name.ToLower().Trim());
        }
        public override int GetHashCode()
        {
            return name.ToLower().GetHashCode();
        }
        public int Length
        {
            get { return name.Length; }
        }
        public static bool operator ==(GameName o1, object o2)
        {
            if (Object.ReferenceEquals(o1, null))
                return (Object.ReferenceEquals(o2, null));
            return o1.Equals(o2);
        }
        public static bool operator !=(GameName o1, object o2)
        {
            return !(o1 == o2);
        }
    }

    [Serializable]
    public class GameObject
    {
        public GameObject() { }
        public GameObject(GameName name) { this.name = name; }
        public GameObject(GameName name, GameName legalVerb) { this.name = name; this.legalVerb = legalVerb; }
        public GameObject(GameName name, GameName legalVerb, Effect effect) { this.name = name; this.legalVerb = legalVerb; this.effectSuccess = effect; }
        public GameObject(string name) { this.name = new GameName(name); }
        public GameObject(string name, string legalVerb) { this.name = new GameName(name); this.legalVerb = new GameName(legalVerb); }
        public GameObject(string name, string legalVerb, Effect effect) { this.name = new GameName(name); this.legalVerb = new GameName(legalVerb); this.effectSuccess = effect; }

        GameName name;
        public GameName Name
        {
            get { return name; }
            set { name = value; }
        }

        GameName legalVerb;
        public GameName LegalVerb
        {
            get { return legalVerb; }
            set { legalVerb = value; }
        }

        object state;
        public object State
        {
            get { return state; }
            set { state = value; }
        }

        public GameName InventoryPrerequisite = new GameName("");

        Effect effectSuccess;
        public Effect EffectSuccess
        {
            get { return effectSuccess; }
            set { effectSuccess = value; }
        }

        Effect effectFailure;
        public Effect EffectFailure
        {
            get { return effectFailure; }
            set { effectFailure = value; }
        }
    }
}
