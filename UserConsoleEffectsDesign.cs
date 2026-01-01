using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureIsland
{
    class UserConsoleEffectsDesign
    {
        internal static Effect SelectEffect(string message, GameObject gameObject, WorldInfo worldInfo, UserInfo userInfo, bool selectPrerequisite)
        {
            if (selectPrerequisite)
                SelectPrerequisite(gameObject, userInfo);

            UserConsole.WriteLine("");
            UserConsole.WriteLine(message);
            int count = 1;
            foreach (string name in worldInfo.EffectFactory.EffectNames)
            {
                Console.WriteLine("\t{0,2}. {1}", count, name);
                count++;
            }

            int? idx = null;
            while (idx == null)
            {
                string selection = UserConsole.AskUser("Select a number >>");
                try
                {
                    double didx = Convert.ToDouble(selection) - 1;
                    idx = Convert.ToInt32(didx);
                }
                catch { }
            }

            Effect effect = worldInfo.EffectFactory.CreateEffect(worldInfo.EffectFactory.EffectNames[idx.Value]);
            effect.DesignEffect(gameObject.Name, worldInfo, userInfo);
            effect.EffectOnObjectDescription = UserConsole.AskUser("[Optional] How would you describe what happens?", false);
            effect.Author = userInfo.Username;

            return effect;
        }

        internal static void SelectPrerequisite(GameObject gameObject, UserInfo userInfo)
        {
            if (userInfo.Inventory.Count == 0) return;

            UserConsole.WriteLine("");
            UserConsole.WriteLine("Are any of these items required to do this?");
            int count = 1;
            Console.WriteLine("\t{0,2}. Nothing is required.", 0);
            foreach (GameName item in userInfo.Inventory)
            {
                Console.WriteLine("\t{0,2}. {1}", count, item);
                count++;
            }

            int? idx = null;
            while (idx == null)
            {
                string selection = UserConsole.AskUser("Select a number >>");
                try
                {
                    double didx = Convert.ToDouble(selection) - 1;
                    idx = Convert.ToInt32(didx);
                }
                catch { }
            }

            if (idx.Value >= 0 && idx.Value < userInfo.Inventory.Count)
                gameObject.InventoryPrerequisite = userInfo.Inventory[idx.Value];
        }

        internal static int DesignEffectDeltaValue(WorldInfo worldInfo, UserInfo userInfo, string valueName, bool increase)
        {
            string dirn = increase? "increase" : "decrease";
            string prompt = String.Format("By how much does the {0} {1}?", valueName, dirn);
            int? delta = null;
            while (delta == null)
            {
                try
                {
                    string deltaStr = UserConsole.AskUser(prompt);
                    double ddelta = Convert.ToDouble(deltaStr);
                    delta = Convert.ToInt32(ddelta);
                }
                catch
                {
                    UserConsole.WriteError("Please enter a number.");
                }
            }
            return delta.Value;
        }
    }
}
