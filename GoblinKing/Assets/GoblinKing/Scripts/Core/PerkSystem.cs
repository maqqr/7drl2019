using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoblinKing.Data;

namespace GoblinKing.Core
{
    public class PerkSystem
    {
        private GameData gameData;
        private Dictionary<string, Perk> purchasedPerks = new Dictionary<string, Perk>();

        public PerkSystem(GameData gameData)
        {
            this.gameData = gameData;
        }

        public bool HasPerk(string key)
        {
            return purchasedPerks.ContainsKey(key);
        }

        public bool CanBuyPerk(string key)
        {
            Perk buying = gameData.PerkData[key];

            foreach (var perkKey in buying.Requirement)
            {
                if (!HasPerk(perkKey))
                {
                    return false;
                }
            }
            return true;
        }

        public void BuyPerk(string key)
        {
            Perk buying = gameData.PerkData[key];

            if (!HasPerk(key))
            {
                purchasedPerks.Add(key, gameData.PerkData[key]);
            }
        }

        // Examples:
        // "perks": {
        //   "p1": { "speedReduction": 10 },
        //   "p2": { "speedReduction": 20, "canBackstab": true },
        //   "p3": { "speedReduction": 50 },
        // }
        // If all perks are purchased, then
        // GetMaxFloat("speedReduction") == 50
        // GetMaxFloat("maxLife") == 0
        // HasAnyTrueValue("canBackstab") == true
        // HasAnyTrueValue("walkOnWalls") == false

        public float GetMaxFloat(string name, float defaultValue = 0f)
        {
            float value = defaultValue;

            foreach (var kv in purchasedPerks)
            {
                foreach (var gain in kv.Value.Gains)
                {
                    if (gain.Value.NumberValue > value)
                    {
                        value = gain.Value.NumberValue;
                    }
                }
            }
            return value;
        }

        public int GetMaxInt(string name, int defaultValue = 0)
        {
            int value = defaultValue;

            foreach (var kv in purchasedPerks)
            {
                foreach (var gain in kv.Value.Gains)
                {
                    if ((int)gain.Value.NumberValue > value)
                    {
                        value = (int)gain.Value.NumberValue;
                    }
                }
            }
            return value;
        }

        public bool HasAnyTrueValue(string name)
        {
            foreach (var kv in purchasedPerks)
            {
                foreach (var gain in kv.Value.Gains)
                {
                    if (gain.Value.BoolValue)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}