using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureIsland
{
    [Serializable]
    public class AreaInfoManager
    {
        Dictionary<AreaInfoCoordinate, AreaInfo> Areas = new Dictionary<AreaInfoCoordinate, AreaInfo>();
        public void Add(AreaInfo newArea)
        {
            if (Areas.ContainsKey(newArea.Coordinate))
                throw new ApplicationException(String.Format("An area was added to {0}, where an area already exists.", newArea.Coordinate));

            Areas.Add(newArea.Coordinate, newArea);
        }
        public AreaInfo this[AreaInfoCoordinate coord]
        {
            get { return Areas[coord]; }
        }
        public bool AreaExists(AreaInfoCoordinate coord)
        {
            return Areas.ContainsKey(coord);
        }
        public bool AreaExists(string name)
        {
            foreach (AreaInfo area in Areas.Values)
                if(area.Name.Equals(name))
                    return true;
            return false;
        }
        public AreaInfo GetArea(AreaInfo areaInfo, AreaDirection dirn)
        {
            AreaInfoCoordinate coor = areaInfo.Coordinate[dirn];
            if (!AreaExists(coor)) return null;
            return Areas[coor];
        }
        public IEnumerable<AreaInfo> All
        {
            get { return Areas.Values; }
        }
        public void Remove(AreaInfo areaInfo2)
        {
            Areas.Remove(areaInfo2.Coordinate);
        }
    }


    [Serializable]
    public enum AreaDirection
    {
        N, E, S, W
    }

    [Serializable]
    public struct AreaInfoCoordinate
    {
        public int X;
        public int Y;
        public AreaInfoCoordinate(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        public AreaInfoCoordinate this[AreaDirection direction]
        {
            get
            {
                AreaInfoCoordinate nextAreaCoord = this;
                switch (direction)
                {
                    case AreaDirection.N: nextAreaCoord.Y++; break;
                    case AreaDirection.E: nextAreaCoord.X++; break;
                    case AreaDirection.S: nextAreaCoord.Y--; break;
                    case AreaDirection.W: nextAreaCoord.X--; break;
                    default:
                        throw new ApplicationException("Tried to GetAdjacentArea for something other than N, E, S, or W.");
                }
                return nextAreaCoord;
            }
        }
        public override string ToString()
        {
            return String.Format("({0},{1})", X, Y);
        }
    }

    [Serializable]
    public class AreaInfo
    {
        public bool HasBeenVisited = false;
        public string Description = "";
        public GameName Discoverer = new GameName("");
        public GameName Name = new GameName("");
        public AreaInfoCoordinate Coordinate = new AreaInfoCoordinate();
        public List<GameObject> GameObjects = new List<GameObject>();
        public GameObject GetGameObject(GameName name)
        {
            foreach (GameObject obj in GameObjects)
                if (obj.Name == name)
                    return obj;
            return null;
        }
        public bool ContainsObject(GameName name)
        {
            foreach (GameObject obj in GameObjects)
                if (obj.Name == name)
                    return true;
            return false;
        }
        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
