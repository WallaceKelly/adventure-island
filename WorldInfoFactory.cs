using System;
using System.Collections.Generic;
using System.Text;

namespace AdventureIsland
{
    public class WorldInfoFactory
    {
        public static WorldInfo CreateSampleWorld()
        {
            WorldInfo worldInfo = new WorldInfo();
            worldInfo.EffectFactory = new EffectFactory();

            AreaInfo areaInfo = new AreaInfo();

            areaInfo.Name = new GameName("The Cave");
            areaInfo.Description = "The cave is damp and cool. You hear gentle, but forceful breathing. As your eyes adjust to the darkness, you realize there is a sleeping dragon before you.";

            EffectIncreaseHealthBase elfBreadEffect = worldInfo.EffectFactory.CreateEffect("Health increases.") as EffectIncreaseHealthBase;
            elfBreadEffect.Delta = 25;

            EffectIncreaseGoldBase goldCoinsEffect = worldInfo.EffectFactory.CreateEffect("Gold is acquired.") as EffectIncreaseGoldBase;
            goldCoinsEffect.Delta = 10;
            // worldInfo.TotalAvailableGold += 10;

            EffectAddToInventoryBase swordEffect = worldInfo.EffectFactory.CreateEffect("An item is added to your inventory.") as EffectAddToInventoryBase;
            swordEffect.ItemToAdd = new GameName("sword");

            GameObject elfBread = new GameObject("elf bread", "eat", elfBreadEffect);
            GameObject goldCoins = new GameObject("gold coins", "get", goldCoinsEffect);
            GameObject dragon = new GameObject("dragon", "fight");
            GameObject sword = new GameObject("sword", "get", swordEffect);

            EffectFight.FightableObjectState dragonState = new EffectFight.FightableObjectState();
            dragonState.Strength = 50;
            dragon.State = dragonState;
            dragon.InventoryPrerequisite = new GameName("sword");

            EffectIncreaseGoldBase defeatDragonEffect = worldInfo.EffectFactory.CreateEffect("Gold is acquired.") as EffectIncreaseGoldBase;
            defeatDragonEffect.Delta = 10;
            // worldInfo.TotalAvailableGold += 10;
            dragon.EffectSuccess = defeatDragonEffect;

            EffectDecreaseHealthBase loseToDragonEffect = worldInfo.EffectFactory.CreateEffect("Health decreases.") as EffectDecreaseHealthBase;
            loseToDragonEffect.Delta = 25;
            dragon.EffectFailure = loseToDragonEffect;

            areaInfo.GameObjects.Add(elfBread);
            areaInfo.GameObjects.Add(goldCoins);
            areaInfo.GameObjects.Add(dragon);
            areaInfo.GameObjects.Add(sword);

            //worldInfo.Objects[elfBread.Name] = elfBread;
            //worldInfo.Objects[goldCoins.Name] = goldCoins;
            //worldInfo.Objects[dragon.Name] = dragon;
            //worldInfo.Objects[sword.Name] = sword;

            areaInfo.HasBeenVisited = true;
            areaInfo.Coordinate = new AreaInfoCoordinate(0, 0);

            worldInfo.AreaManager.Add(areaInfo);

            return worldInfo;
        }
    }
}
