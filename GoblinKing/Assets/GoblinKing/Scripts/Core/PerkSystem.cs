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
    }
}