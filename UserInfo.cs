using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace AdventureIsland
{
    public delegate void MoveHandler(AreaInfo newAreaInfo);

    [Serializable]
    public class UserInfo
    {
        public GameName Username;
        public List<GameName> GoldAcquiredObjects = new List<GameName>();
        public List<GameName> Inventory = new List<GameName>();

        // careful... only used for coloring map. Can be reset.
        // TODO: rename
        public List<GameName> VisitedAreas = new List<GameName>();

        public UserInfo(GameName name)
        {
            Username = name;
        }

        [field:NonSerialized]
        public event MoveHandler Move;

        public bool HealthModified = false;

        AreaInfo currentArea;
        public AreaInfo CurrentArea
        {
            get { return currentArea; }
            set
            {
                if (value != null && value != currentArea)
                {
                    currentArea = value;
                    if (!VisitedAreas.Contains(currentArea.Name)) VisitedAreas.Add(currentArea.Name);
                    if (Move != null) Move(currentArea);
                }
            }
        }

        int gold = 0;
        public int Gold
        {
            get { return gold; }
            set { gold = value; }
        }

        public int CalculateGold(WorldInfo worldInfo)
        {
            int total = 0;

            List<GameName> TempList = new List<GameName>(GoldAcquiredObjects);

            foreach (GameObject g in worldInfo.GetAllObjects())
            {
                if (TempList.Contains(g.Name))
                {
                    if (g.EffectSuccess is EffectIncreaseGoldBase)
                    {
                        EffectIncreaseGoldBase e = g.EffectSuccess as EffectIncreaseGoldBase;
                        total += e.Delta;
                    }
                    if (g.EffectFailure is EffectIncreaseGoldBase)
                    {
                        EffectIncreaseGoldBase e = g.EffectFailure as EffectIncreaseGoldBase;
                        total += e.Delta;
                    }
                    TempList.Remove(g.Name);
                }
            }

            foreach (GameName s in TempList)
                GoldAcquiredObjects.Remove(s);

            return total;
        }

        int strength = 50;
        public int Strength
        {
            get { return strength; }
            set { strength = Math.Max(0, Math.Min(100, value)); }
        }

        int health = 100;
        public int Health
        {
            get { return health; }
            set { health = Math.Max(0, Math.Min(100, value)); }
        }
    }
}
