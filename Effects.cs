using System;
using System.Reflection;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AdventureIsland
{
    // tags a class as an effect that can be createds
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class EffectAttribute : Attribute
    {
        // should this effect be listed in the menu of effects
        public bool Visible = true;
    }

    // [Serializable]
    public class EffectFactory
    {
        Dictionary<string, Type> effectTypes = new Dictionary<string, Type>();
        public EffectFactory()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    foreach (EffectAttribute attr in t.GetCustomAttributes(typeof(EffectAttribute), false))
                    {
                        if (attr.Visible)
                        {
                            Effect effect = Activator.CreateInstance(t) as Effect;
                            effectTypes.Add(effect.Description, t);
                        }
                    }
                }
            }
        }
        public string[] EffectNames
        {
            get
            {
                string[] names = new string[effectTypes.Keys.Count];
                effectTypes.Keys.CopyTo(names, 0);
                return names;
            }
        }
        public virtual Effect CreateEffect(string name)
        {
            Type t = effectTypes[name];
            return Activator.CreateInstance(t) as Effect;
        }
    }

    public delegate void DesignEffectHandler(Effect effect, WorldInfo worldInfo, UserInfo userInfo);

    [Serializable]
    public abstract class Effect
    {
        public abstract string Description { get; }

        public GameName Author = new GameName("");

        public string EffectOnObjectDescription = "";

        public abstract string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo);

        // override to provide UI-specific implementation
        public abstract void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo);

        //[OnDeserializing]
        //void OnDeserializing(StreamingContext context)
        //{
        //    // setting default values here helps add new members to serialized objects
        //    //EffectOnObjectDescription = new GameName("");
        //    //Author = new GameName("");
        //}

        public virtual string GetFullDescription(string statusMessage)
        {
            StringBuilder description = new StringBuilder();
            string genericDescription = statusMessage;

            if (EffectOnObjectDescription.Length > 0)
            {
                description.Append(EffectOnObjectDescription);
                if (genericDescription.Length > 0)
                    description.Append(String.Format(" ({0})", genericDescription));
            }
            else
                description.Append(genericDescription);

            if (Author.Length > 0)
                description.Append(String.Format(" ({0})", Author));

            return description.ToString();
        }
    }

    [Serializable]
    [Effect(Visible = false)]
    public class EffectGo : Effect
    {
        const string name = "Travel to an adjacent area.";
        public override string Description
        {
            get { return name; }
        }
        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            try
            {
                char directionChar = objectName.ToString().ToUpper()[0];
                AreaDirection dirn = (AreaDirection)Enum.Parse(typeof(AreaDirection), new String(directionChar, 1));
                if(!worldInfo.AreaManager.AreaExists(userInfo.CurrentArea.Coordinate[dirn]))
                {
                    if (!(userInfo.Username.Equals(new GameName("admin"))) && userInfo.Gold != worldInfo.CalculateTotalAvailableGold())
                        return String.Format("No one has explored that direction. You must find all the gold before exploring new areas.");
                    bool response = UserConsole.AskUserYesOrNo("No one has explored that direction. Continue? [y/n]");
                    if (response == false)
                        return String.Format("You decide not to go in that direction.");
                    AreaInfo newArea = new AreaInfo();
                    newArea.Coordinate = userInfo.CurrentArea.Coordinate[dirn];
                    worldInfo.AreaManager.Add(newArea);
                }
                userInfo.CurrentArea = worldInfo.AreaManager[userInfo.CurrentArea.Coordinate[dirn]];
                
                string directionString = "";
                switch (dirn)
                {
                    case AreaDirection.N: directionString = "North"; break;
                    case AreaDirection.E: directionString = "East"; break;
                    case AreaDirection.S: directionString = "South"; break;
                    case AreaDirection.W: directionString = "West"; break;
                    default:
                        throw new ApplicationException("Need to update EffectGo.Execute().");
                }

                return String.Format("Traveled {0}.", directionString);
            }
            catch
            {
                return String.Format("You cannot go {0}.", objectName);
            }
        }
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            throw new ApplicationException("This method should not be called.");
        }
    }

    [Serializable]
    public abstract class EffectMessageBase : Effect
    {
        const string description = "Other (to be described by you).";
        public override string Description { get { return description;  } }
    }

    [Serializable]
    public abstract class EffectDeltaValue : Effect
    {
        int delta;
        public virtual int Delta
        {
            set { delta = value; }
            get { return delta; }
        }

        public override string Description
        {
            get
            {
                string valueNameCap = ValueName.ToUpper()[0] + ValueName.Substring(1);
                string increaseStr = Increase ? "increases" : "decreases";
                return String.Format("{0} {1}.", valueNameCap, increaseStr);
            }
        }
        protected abstract string ValueName { get; }
        protected abstract bool Increase { get; }
    }

    [Serializable]
    public abstract class EffectIncreaseGoldBase : EffectDeltaValue
    {
        public override string Description { get { return "Gold is acquired."; } }
        protected override bool Increase { get { return true; } }
        protected override string ValueName { get { return "gold"; } }

        static readonly int MAXGOLD = 10;
        public override int Delta
        {
            get { return base.Delta; }
            set { base.Delta = Math.Min(MAXGOLD, Math.Max(0, value)); }
        }

        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            if (userInfo.GoldAcquiredObjects.Contains(objectName))
                return String.Format("You already did that.");

            else
            {
                userInfo.Gold += Delta;
                userInfo.GoldAcquiredObjects.Add(objectName);
                worldInfo.Scores[userInfo.Username.ToString()] = userInfo.Gold;
                return String.Format("Gold increased by {0}.", Delta);
            }
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            if (Delta > MAXGOLD) Delta = MAXGOLD;
            if (Delta < 0) Delta = 0;
        }
    }

    [Serializable]
    public abstract class EffectDecreaseHealthBase : EffectDeltaValue
    {
        protected override string ValueName { get { return "health"; } }
        protected override bool Increase { get { return false; } }

        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            userInfo.Health -= Delta;
            userInfo.HealthModified = true;
            return String.Format("Health decreases by {0}.", Delta);
        }
    }

    [Serializable]
    public abstract class EffectIncreaseHealthBase : EffectDeltaValue
    {
        protected override string ValueName { get { return "health"; } }
        protected override bool Increase { get { return true; } }

        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            userInfo.Health += Delta;
            userInfo.HealthModified = true;
            return String.Format("Health increases by {0}.", Delta);
        }
    }

    [Serializable]
    public abstract class EffectDecreaseStrengthBase : EffectDeltaValue
    {
        protected override string ValueName { get { return "strength"; } }
        protected override bool Increase { get { return false; } }

        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            userInfo.Strength -= Delta;
            return String.Format("Strength decreases by {0}.", Delta);
        }
    }

    [Serializable]
    public abstract class EffectIncreaseStrengthBase : EffectDeltaValue
    {
        protected override string ValueName { get { return "strength"; } }
        protected override bool Increase { get { return true; } }

        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            userInfo.Strength += Delta;
            return String.Format("Strength increases by {0}.", Delta);
        }
    }

    [Serializable]
    public abstract class EffectAddToInventoryBase : Effect
    {
        const string description = "An item is added to your inventory.";
        public override string Description
        {
            get { return description; }
        }
        public GameName ItemToAdd = new GameName("");
        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            GameName itemToAdd = ItemToAdd.Length > 0 ? ItemToAdd : objectName;
            if (!userInfo.Inventory.Contains(itemToAdd))
                userInfo.Inventory.Add(itemToAdd);
            return String.Format("The {0} has been added to your inventory.", itemToAdd);
        }
    }

    [Serializable]
    public abstract class EffectFightBase : Effect
    {
        const string description = "You engage in combat.";
        public override string Description
        {
            get { return description; }
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectIncreaseGold : EffectIncreaseGoldBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            Delta = UserConsoleEffectsDesign.DesignEffectDeltaValue(worldInfo, userInfo, ValueName, Increase);
            // worldInfo.TotalAvailableGold += Delta;
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectDecreaseHealth : EffectDecreaseHealthBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            Delta = UserConsoleEffectsDesign.DesignEffectDeltaValue(worldInfo, userInfo, ValueName, Increase);
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectIncreaseHealth : EffectIncreaseHealthBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            Delta = UserConsoleEffectsDesign.DesignEffectDeltaValue(worldInfo, userInfo, ValueName, Increase);
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectDecreaseStrength : EffectDecreaseStrengthBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            Delta = UserConsoleEffectsDesign.DesignEffectDeltaValue(worldInfo, userInfo, ValueName, Increase);
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectIncreaseStrength : EffectIncreaseStrengthBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            Delta = UserConsoleEffectsDesign.DesignEffectDeltaValue(worldInfo, userInfo, ValueName, Increase);
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectAddItemToInventory : EffectAddToInventoryBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            string s = UserConsole.AskUser(String.Format("What item is added to the inventory? (press enter to add the {0})", objectName), false);
            ItemToAdd = new GameName(s);
        }
    }

    [Serializable]
    [Effect(Visible = true)]
    public class EffectMessage : EffectMessageBase
    {
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            // none required
        }
        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            // none required?
            return "";
        }
    }

    [Serializable]
    [Effect(Visible = false)]
    public class EffectExit : Effect
    {
        const string description = "The game exits.";
        public override string Description
        {
            get { return description; }
        }
        public override string Execute(GameName objectString, WorldInfo worldInfo, UserInfo userInfo)
        {
            worldInfo.ExitNow = true;
            DataSerializer.SaveWorldInfo<WorldInfo>(WorldInfo.FILENAME, worldInfo);
            DataSerializer.SaveUserInfo(userInfo);
            return "Exiting Adventure Island.";
        }
        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            throw new Exception("This method should not be called.");
        }
    }

    [Serializable]
    [Effect(Visible = false)]
    public class EffectFight : EffectFightBase
    {
        [NonSerialized]
        static Random rng = new Random();

        [Serializable]
        public struct FightableObjectState
        {
            public int? Strength;
        }

        public override string Execute(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            GameObject gameObject = userInfo.CurrentArea.GetGameObject(objectName);
            if (gameObject == null)
                return String.Format("This area does not have a {0} to fight.", objectName);

            if (gameObject.LegalVerb != null && !gameObject.LegalVerb.Equals("fight"))
                return String.Format("You cannot fight the {0}.", objectName);

            if (gameObject.State == null || gameObject.EffectSuccess == null || gameObject.EffectFailure == null)
            {
                DesignEffect(objectName, worldInfo, userInfo);
                gameObject.LegalVerb = new GameName("fight");
            }

            double userModifiedStrength = (double)(userInfo.Strength / 100.0 * userInfo.Health / 100.0);
            FightableObjectState fightableObjectState = (FightableObjectState)gameObject.State;
            double opponentStrength = (double)fightableObjectState.Strength / 100.0;
            double winningNumber = rng.NextDouble();
            double strengthRatio = opponentStrength / (userModifiedStrength + opponentStrength);

            UserConsole.WriteFightResults(userInfo.Username.ToString(), objectName.ToString(), userModifiedStrength, opponentStrength, winningNumber);

            string status = "";
            if (winningNumber > strengthRatio)
            {
                Console.ForegroundColor = ConsoleColor.White;
                status = gameObject.EffectSuccess.Execute(objectName, worldInfo, userInfo);
                status = gameObject.EffectSuccess.GetFullDescription(status);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                status = gameObject.EffectFailure.Execute(objectName, worldInfo, userInfo);
                status = gameObject.EffectFailure.GetFullDescription(status);
            }

            return status;
        }

        public override void DesignEffect(GameName objectName, WorldInfo worldInfo, UserInfo userInfo)
        {
            Console.WriteLine("You are the first to fight the {0}.", objectName);

            GameObject gameObject = userInfo.CurrentArea.GetGameObject(objectName);

            UserConsoleEffectsDesign.SelectPrerequisite(gameObject, userInfo);

            FightableObjectState state = new FightableObjectState();
            while (state.Strength == null)
            {
                try
                {
                    UserConsole.WriteLine(String.Format("Your strength is {0}. Your health is {1}.", userInfo.Strength, userInfo.Health));
                    string strengthString = UserConsole.AskUser("What is your opponent's strength?");
                    int strengthOfOpponent = Convert.ToInt32(strengthString);
                    strengthOfOpponent = Math.Max(0, Math.Min(100, strengthOfOpponent));
                    state.Strength = strengthOfOpponent;
                }
                catch { }
            }

            string message = String.Format("What happens if the {0} is defeated?", objectName);
            gameObject.EffectSuccess = UserConsoleEffectsDesign.SelectEffect(message, gameObject, worldInfo, userInfo, false);

            message = String.Format("What happens if the {0} defeats you?", objectName);
            gameObject.EffectFailure = UserConsoleEffectsDesign.SelectEffect(message, gameObject, worldInfo, userInfo, false);

            gameObject.State = state;
        }
    }
}
