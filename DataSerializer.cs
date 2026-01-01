using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Text;

namespace AdventureIsland
{
    public class DataSerializer
    {
        public static bool WorldInfoFileExists(string name)
        {
            return File.Exists(name);
        }

        public static bool UserInfoFileExists(string username)
        {
            string filename = String.Format("{0}.ser", username);
            return File.Exists(filename);
        }

        public static T LoadWorldInfo<T>(string filename) where T : class
        {
            T worldInfo = null;
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                worldInfo = formatter.Deserialize(stream) as T;
            }
            return worldInfo;
        }

        public static void SaveWorldInfo<T>(string filename, T worldInfo)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, worldInfo);
            }
        }

        public static void SaveUserInfo(UserInfo userInfo)
        {
            using (FileStream stream = new FileStream(String.Format("{0}.ser", userInfo.Username), FileMode.Create))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, userInfo);
            }
        }

        public static UserInfo LoadUserInfo(GameName username)
        {
            UserInfo userInfo = null;
            using (FileStream stream = new FileStream(String.Format("{0}.ser", username), FileMode.Open))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                userInfo = formatter.Deserialize(stream) as UserInfo;
            }
            return userInfo;
        }
    }
}
