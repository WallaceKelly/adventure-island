using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace AdventureIsland
{
    [Serializable]
    public class WorldInfo
    {
        public const int HealthCostPerTurn = 10;

        [NonSerialized]
        public const string FILENAME = "AdventureIsland3.ser";

        // [Obsolete]
        // public Dictionary<string, GameObject> Objects = new Dictionary<string, GameObject>();

        public Dictionary<string, int> Scores = new Dictionary<string, int>();

        public bool ContainsObject(GameName name)
        {
            foreach (AreaInfo area in AreaManager.All)
                foreach (GameObject g in area.GameObjects)
                    if(g.Name.Equals(name))
                        return true;
            return false;
        }

        public List<GameObject> GetAllObjects()
        {
            List<GameObject> objects = new List<GameObject>();
            foreach (AreaInfo area in AreaManager.All)
                foreach (GameObject g in area.GameObjects)
                    objects.Add(g);
            return objects;
        }

        public List<GameName> AutoVerbs = new List<GameName>();
        public AreaInfoManager AreaManager = new AreaInfoManager();

        [NonSerialized]
        public VerbFactory VerbFactory;

        [NonSerialized]
        public EffectFactory EffectFactory;

        int totalAvailableGold = 0;
        [Obsolete]
        public int TotalAvailableGold
        {
            set { totalAvailableGold = value; }
            get { return totalAvailableGold; }
        }
        public int CalculateTotalAvailableGold()
        {
            int total = 0;
            foreach (GameObject g in GetAllObjects())
            {
                if(g.EffectSuccess is EffectIncreaseGoldBase)
                {
                    EffectIncreaseGoldBase e = g.EffectSuccess as EffectIncreaseGoldBase;
                    total += Math.Max(0, e.Delta);
                }
                if(g.EffectFailure is EffectIncreaseGoldBase)
                {
                    EffectIncreaseGoldBase e = g.EffectFailure as EffectIncreaseGoldBase;
                    total += Math.Max(0, e.Delta);
                }
            }
            return total;
        }

        public AreaInfo FirstArea
        {
            get { return AreaManager[new AreaInfoCoordinate(0, 0)]; }
        }

        [NonSerialized]
        public bool ExitNow = false;

        public WorldInfo()
        {
            Initialize();
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            Initialize();
        }

        protected void Initialize()
        {
            VerbFactory = new VerbFactory();
            foreach (GameName s in AutoVerbs)
                AddAutoVerb(s);
            ExitNow = false;
        }

        public void AddAutoVerb(GameName name)
        {
            VerbAuto newVerb = new VerbAuto();
            newVerb.Name = name;
            VerbFactory.AddAutoVerb(name, newVerb);
            if (!AutoVerbs.Contains(name))
                AutoVerbs.Add(name);
        }
    }
}
