using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AdventureIsland
{
    class UserConsole
    {
        internal static void Clear()
        {
            Console.Clear();
        }

        internal static string AskUser(string question)
        {
            return AskUser(question, true);
        }

        internal static string AskUser(string question, bool stringRequired)
        {
            string response = "";
            do
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(question + " ");
                Console.ForegroundColor = ConsoleColor.White;
                response = Console.ReadLine();
            } while (response.Length == 0 && stringRequired);

            return response;
        }

        internal static bool AskUserYesOrNo(string question)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(question + " ");
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleKeyInfo info = Console.ReadKey();
                if (info.KeyChar == 'y' || info.KeyChar == 'Y')
                    return true;
                if (info.KeyChar == 'n' || info.KeyChar == 'N')
                    return false;
                Console.WriteLine();
            }
        }

        internal static void WriteStatusMessage(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
        }

        internal static void WriteWorldInfo(WorldInfo worldInfo)
        {
            Console.WriteLine("");
        }

        internal static void WriteUserInfo(WorldInfo worldInfo, UserInfo userInfo)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Gold: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(userInfo.Gold);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write(" (of {0})", worldInfo.CalculateTotalAvailableGold());

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Health: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(userInfo.Health);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("  Strength: ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(userInfo.Strength);

            Console.WriteLine();
        }

        internal static void WriteError(string p)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(p);
            Console.ResetColor();
        }

        internal static string ParseUserCmdVerb(string userCmd, UserInfo userInfo)
        {
            //foreach (GameObject1 obj in userInfo.CurrentArea.GameObjects)
            //{
            //    if (userCmd.EndsWith(obj.Name))
            //    {
            //        int idx = userCmd.LastIndexOf(obj.Name);
            //        string verb = userCmd.Substring(0, idx-1);
            //        return verb;
            //    }
            //}

            userCmd = userCmd + ' ';
            string[] userCmdWords = userCmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return userCmdWords[0];
        }

        internal static string ParseUserCmdObject(string userCmd, UserInfo userInfo)
        {
            userCmd = userCmd + ' ';
            //string[] userCmdWords = userCmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] userCmdWords = userCmd.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (userCmdWords.Length == 1)
                return "";
            return userCmdWords[1].Trim();
        }

        internal static void ReadAreaDescription(WorldInfo worldInfo, UserInfo userInfo)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("You are the first to come here.");

            // get an unique name for the area
            bool validName = false;
            while(!validName)
            {
                string areaName = AskUser("What would you like to name this area?", true).Trim();
                if (worldInfo.AreaManager.AreaExists(areaName))
                {
                    UserConsole.WriteError(String.Format("There is already an area named {0}.", areaName));
                }
                else
                {
                    validName = true;
                    userInfo.CurrentArea.Name = new GameName(areaName);
                }
            }

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("What do you see in this area?");

            // get a list of the objects in this area
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("\t");
                GameName objectName = new GameName(Console.ReadLine().Trim());
                if (objectName.Length == 0)
                    break;

                if (!worldInfo.ContainsObject(objectName))
                {
                    GameObject go = new GameObject();
                    go.Name = objectName;
                    // worldInfo.Objects.Add(objectStr, go);
                    userInfo.CurrentArea.GameObjects.Add(go);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("\tThere is already an object named {0}", objectName);
                }
            }

            userInfo.CurrentArea.Description = AskUser("How would you describe this area?");
            userInfo.CurrentArea.Discoverer = userInfo.Username;
            userInfo.CurrentArea.HasBeenVisited = true;
        }

        internal static void WriteAreaInfo(AreaInfo areaInfo)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("{0} named this area:", areaInfo.Discoverer);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t{0}", areaInfo.Name);

            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine(areaInfo.Coordinate);

            //Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine("{0} described it: ", areaInfo1.Discoverer);
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t{0}", areaInfo.Description);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("You see the following: ");
            foreach (GameObject go in areaInfo.GameObjects)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\t{0}", go.Name);
            }
            Console.WriteLine();
        }

        internal static void WriteLine(string p)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(p);
        }

        internal static void ListInventory(UserInfo userInfo)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nYour inventory contains the following:");

            Console.ForegroundColor = ConsoleColor.Green;
            bool firstItem = true;
            foreach (GameName s in userInfo.Inventory)
            {
                if (!firstItem)
                    Console.Write(", ");
                Console.Write(s);
                firstItem = false;
            }
            Console.WriteLine();
            PressAnyKey();
        }

        internal static void ListVerbs(WorldInfo worldInfo)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nYou know how to do the following:");

            Console.ForegroundColor = ConsoleColor.Green;
            bool firstItem = true;
            foreach (GameName s in worldInfo.VerbFactory.GetKeys())
            {
                Type verbType = worldInfo.VerbFactory.GetInstance(s).GetType();
                VerbAttribute[] attrs = verbType.GetCustomAttributes(typeof(VerbAttribute), false) as VerbAttribute[];
                if (attrs.Length > 0)
                {
                    VerbAttribute attr = attrs[0];
                    if (attr.Visible == true)
                    {
                        if (!firstItem)
                            Console.Write(", ");
                        Console.Write(s);
                        firstItem = false;
                    }
                }
            }
            Console.WriteLine();
            PressAnyKey();

        }

        internal static void PressAnyKey()
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey(true);
        }

        internal static void WriteFightResults(string userName, string opponentName, double userStrength, double opponentStrength, double winningNumber)
        {
            Random rng = new Random();
            double strengthRatio = opponentStrength / (userStrength + opponentStrength);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("You fight the {0}, whose strength is {1}.", opponentName, (opponentStrength * 100));
            //Console.WriteLine("strengthRatio: {0}", strengthRatio);
            //Console.WriteLine("  winningSide: {0}", winningNumber);

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\t{0}", userName.ToUpper());
            int row0 = Console.CursorTop;
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\t{0}", opponentName.ToUpper());

            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition(0, row0);
                string str1 = new String('#', (int)(6.0 * userStrength * winningNumber * i + 6.0 * rng.Next(10-i)));
                string str2 = new String('#', (int)(6.0 * opponentStrength * (1 - winningNumber) * i + 6.0 * rng.Next(10-i)));
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\t|#{0,-60}", str1);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\t|#{0,-60}", str2);
                Console.WriteLine();
                System.Threading.Thread.Sleep(200 * i);
            }

            Console.WriteLine();

            if (winningNumber > strengthRatio)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("You win!");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("You lose!");
            }
            UserConsole.PressAnyKey();
        }

        internal static void ListScores(WorldInfo worldInfo)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nGame Scores:");

            Console.ForegroundColor = ConsoleColor.Green;

            List<string> highScoreList = new List<string>(worldInfo.Scores.Keys);
            highScoreList.Sort(delegate(string n1, string n2)
            {
                return n1.CompareTo(n2);
            });


            //foreach (KeyValuePair<int, string> scores in highScores)
            //    Console.WriteLine("{0}: {1}", scores.Value, scores.Key);

            //foreach (GameName s in worldInfo.VerbFactory.GetKeys())
            //{
            //    Type verbType = worldInfo.VerbFactory.GetInstance(s).GetType();
            //    VerbAttribute attr = verbType.GetCustomAttributes(typeof(VerbAttribute), false)[0] as VerbAttribute;
            //    if (attr.Visible == true)
            //    {
            //        if (!firstItem)
            //            Console.Write(", ");
            //        Console.Write(s);
            //        firstItem = false;
            //    }
            //}
            Console.WriteLine();
            PressAnyKey();
        }
    }
}
