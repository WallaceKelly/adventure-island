using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;


namespace AdventureIsland
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class VerbAttribute : Attribute
    {
        // should this verb be listed to the user
        public bool Visible = true;
    }

    [Serializable]
    public class VerbFactory : SingletonNameFactory<Verb>
    {
        public VerbFactory()
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in asm.GetTypes())
                {
                    foreach (VerbAttribute attr in t.GetCustomAttributes(typeof(VerbAttribute), false))
                    {
                        Verb verb = Activator.CreateInstance(t) as Verb;
                        this.existingObjects.Add(verb.Name, verb);
                    }
                }
            }
        }

        public void AddAutoVerb(GameName name, Verb verb)
        {
            this.existingObjects.Add(name, verb);
        }
    }

    [Serializable]
    public class Verb : INamed
    {
        GameName name;
        public virtual GameName Name
        {
            get { return name; }
            set { name = value; }
        }
        public virtual string Execute(GameName userCmdObject, WorldInfo worldInfo, UserInfo userInfo)
        {
            if (VerbEffect != null)
            {
                string prerequisite = TestPrerequsite(userCmdObject, userInfo);
                if (prerequisite.Length > 0)
                    return prerequisite;
                return VerbEffect.Execute(userCmdObject, worldInfo, userInfo);
            }
            else
                return "That does not seem to have an effect.";
        }

        protected virtual string TestPrerequsite(GameName userCmdObject, UserInfo userInfo)
        {
            GameObject gameObject = userInfo.CurrentArea.GetGameObject(userCmdObject);
            if (gameObject != null && gameObject.InventoryPrerequisite.Length > 0)
            {
                if (!userInfo.Inventory.Contains(gameObject.InventoryPrerequisite))
                {
                    return String.Format("You must have the {0} to {1} the {2}.", gameObject.InventoryPrerequisite, Name, userCmdObject);
                }
            }
            return "";
        }

        public Effect VerbEffect;
    }

    [Serializable]
    [Verb]
    public class VerbGo : Verb
    {
        GameName name = new GameName("go");
        public override GameName Name
        {
            get { return name; }
//            set { return; }
        }
        public VerbGo()
        {
            VerbEffect = new EffectGo();
        }
    }
    [Serializable]
    [Verb]
    public class VerbExit : Verb
    {
        public VerbExit()
        {
            base.VerbEffect = new EffectExit();
        }
        GameName name = new GameName("exit");
        public override GameName Name { get { return name; } }
    }

    [Serializable]
    public abstract class VerbWithObject : Verb
    {
        public override string Execute(GameName userCmdObject, WorldInfo worldInfo, UserInfo userInfo)
        {
            GameObject obj = userInfo.CurrentArea.GetGameObject(userCmdObject);

            // check to see if the object is in the room
            if (obj == null) return String.Format("There is no {0} to {1} in this area.", userCmdObject, Name);

            // check to see if the object is gettable
            if (obj.LegalVerb != null && !(obj.LegalVerb == Name)) return String.Format("You cannot {0} the {1}.", Name, userCmdObject);

            // check to see if the prerequisite is met
            string prerequisite = TestPrerequsite(userCmdObject, userInfo);
            if (prerequisite.Length > 0)
                return prerequisite;

            // check to see if getting still needs to have an effect defined
            if (obj.EffectSuccess == null)
            {
                string message = String.Format("You are the first to {0} the {1}. What happens?", this.Name, userCmdObject);
                obj.EffectSuccess = UserConsoleEffectsDesign.SelectEffect(message, obj, worldInfo, userInfo, true);
                obj.LegalVerb = Name;
            }

            string genericDescription = obj.EffectSuccess.Execute(userCmdObject, worldInfo, userInfo);
            return obj.EffectSuccess.GetFullDescription(genericDescription);
        }
    }

    [Serializable]
    public class VerbAuto : VerbWithObject
    {

    }

    [Serializable]
    [Verb]
    public class VerbGet : VerbWithObject
    {
        GameName name = new GameName("get");
        public override GameName Name { get { return name; } }
    }

    [Serializable]
    [Verb]
    public class VerbEat : VerbWithObject
    {
        GameName name = new GameName("eat");
        public override GameName Name { get { return name; } }
    }

    [Serializable]
    [Verb]
    public class VerbList : Verb
    {
        GameName name = new GameName("list");
        public override GameName Name
        {
            get { return name; }
            // set { return; }
        }

        public override string Execute(GameName userCmdObject, WorldInfo worldInfo, UserInfo userInfo)
        {
            if (userCmdObject.Equals("inventory"))
            {
                UserConsole.ListInventory(userInfo);
                return "Your inventory was listed.";
            }

            if (userCmdObject.Equals("verbs"))
            {
                UserConsole.ListVerbs(worldInfo);
                return "The available verbs were listed.";
            }

            if (userCmdObject.Equals("scores"))
            {
                UserConsole.ListScores(worldInfo);
                return "The scores were listed.";
            }

            else
            {
                return String.Format("Listing the {0} is not supported.", userCmdObject);
            }
        }
    }

    [Serializable]
    [Verb(Visible=false)]
    public class VerbDelete : Verb
    {
        GameName name = new GameName("delete");
        public override GameName Name
        {
            get { return name; }
            // set { return; }
        }

        public override string Execute(GameName userCmdObject, WorldInfo worldInfo, UserInfo userInfo)
        {
            if (!userInfo.Username.Equals("admin"))
                return "Only the admin can delete things.";
            if(userCmdObject.Equals("room"))
            {
                if (userInfo.CurrentArea == worldInfo.FirstArea)
                    return "The starting area cannot be deleted.";

                if (UserConsole.AskUserYesOrNo("Deleting this room cannot be undone. Coninue? (y/n)"))
                {
                    worldInfo.AreaManager.Remove(userInfo.CurrentArea);
                    userInfo.CurrentArea = worldInfo.FirstArea;
                    return "The room was deleted.";
                }
                else
                {
                    return "The room was not deleted.";
                }
            }
            else 
                return String.Format("You cannot delete the {0}.", userCmdObject);
        }
    }

    [Serializable]
    [Verb]
    public class VerbView : Verb
    {
        GameName name = new GameName("view");
        public override GameName Name
        {
            get { return name; }
            // set { return; }
        }

        [NonSerialized]
        MapForm mapForm = null;
        public override string Execute(GameName userCmdObject, WorldInfo worldInfo, UserInfo userInfo)
        {
            if (userCmdObject.Equals("map"))
            {
                if (mapForm == null)
                {
                    Thread t = new Thread(RunMap);
                    // TODO: Check for cross thread calls
                    mapForm = new MapForm();
                    mapForm.WorldInfo = worldInfo;
                    mapForm.UserInfo = userInfo;
                    t.IsBackground = true;
                    t.Start();
                    return "A map was created.";
                }
                else
                {
                    return "A map was already available.";
                }
            }

            else
            {
                return String.Format("Viewing the {0} is not supported.", userCmdObject);
            }
        }

        void RunMap()
        {
            Application.Run(mapForm);
            mapForm.Dispose();
            mapForm = null;
        }
    }

    [Serializable]
    [Verb]
    public class VerbFight : Verb
    {
        GameName name = new GameName("fight");
        public override GameName Name
        {
            get { return name; }
            set { return; }
        }

        public VerbFight()
        {
            base.VerbEffect = new EffectFight();
        }
    }
}
