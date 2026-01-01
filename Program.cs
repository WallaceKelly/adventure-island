using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AdventureIsland
{
    class Program
    {
        static void Main(string[] args)
        {
            WorldInfo worldInfo = GetWorldInfo();
            GameName username = new GameName(UserConsole.AskUser("What is your name?"));
            UserInfo userInfo = GetUserInfo(username, worldInfo);
            userInfo.CurrentArea = worldInfo.FirstArea;

            string message = "Welcome to Adventure Island.";
            while (!worldInfo.ExitNow)
            {
                UserConsole.Clear();

                if (userInfo.Health <= 0)
                {
                    userInfo.CurrentArea = worldInfo.FirstArea;
                    message = "You faint to the ground because your health fell to zero. When you awake you are in the cave again.";
                }

                if (!userInfo.CurrentArea.HasBeenVisited)
                {
                    UserConsole.ReadAreaDescription(worldInfo, userInfo);
                    DataSerializer.SaveWorldInfo<WorldInfo>(WorldInfo.FILENAME, worldInfo);
                    UserConsole.Clear();
                }

                UserConsole.WriteStatusMessage(message);
                UserConsole.WriteWorldInfo(worldInfo);
                UserConsole.WriteUserInfo(worldInfo, userInfo);
                UserConsole.WriteAreaInfo(userInfo.CurrentArea);

                GameName userCmd = new GameName(UserConsole.AskUser("What would you like to do now?"));
                GameName userCmdVerb = new GameName(UserConsole.ParseUserCmdVerb(userCmd.ToString(), userInfo));
                GameName userCmdObject = new GameName(UserConsole.ParseUserCmdObject(userCmd.ToString(), userInfo));
                message = ExecuteVerb(userCmdVerb, userCmdObject, worldInfo, userInfo);

                if (!userInfo.HealthModified && userInfo.Username != new GameName("admin"))
                    userInfo.Health -= WorldInfo.HealthCostPerTurn;
                userInfo.HealthModified = false;

                DataSerializer.SaveUserInfo(userInfo);
                DataSerializer.SaveWorldInfo<WorldInfo>(WorldInfo.FILENAME, worldInfo);
            }
        }

        private static string ExecuteVerb(GameName userCmdVerb, GameName userCmdObject, WorldInfo worldInfo, UserInfo userInfo)
        {
            if (!worldInfo.VerbFactory.Exists(userCmdVerb))
            {
                if(userCmdObject.Length == 0) 
                {
                    return String.Format("You don't know how to {0}.", userCmdVerb);
                }
                else
                {
                    GameObject gameObject = userInfo.CurrentArea.GetGameObject(userCmdObject);
                    if (gameObject != null && gameObject.LegalVerb == null)
                    {
                        worldInfo.AddAutoVerb(userCmdVerb);
                    }
                    else
                    {
                        return String.Format("You cannot {0} the {1}.", userCmdVerb, userCmdObject);
                    }
                }
            }

            Verb userVerb = worldInfo.VerbFactory.GetInstance(userCmdVerb);
            string message = userVerb.Execute(userCmdObject, worldInfo, userInfo);
            return message;
        }

        static WorldInfo GetWorldInfo()
        {
            WorldInfo worldInfo;

            try
            {
                worldInfo = DataSerializer.LoadWorldInfo<WorldInfo>(WorldInfo.FILENAME);
                worldInfo.EffectFactory = new EffectFactory();

            }
            catch
            {
                worldInfo = WorldInfoFactory.CreateSampleWorld();
            }

            return worldInfo;
        }

        static UserInfo GetUserInfo(GameName name, WorldInfo worldInfo)
        {
            UserInfo userInfo;

            try
            {
                userInfo = DataSerializer.LoadUserInfo(name);
                userInfo.Gold = userInfo.CalculateGold(worldInfo);
            }
            catch
            {
                userInfo = new UserInfo(name);
            }
            return userInfo;
        }

    }
}
