using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Data
{
    public class GameData
    {
        public readonly Dictionary<string, ItemData> ItemData = new Dictionary<string, ItemData>();
        public readonly Dictionary<string, CreatureData> CreatureData = new Dictionary<string, CreatureData>();
        public readonly Dictionary<int, string[]> SpawnList = new Dictionary<int, string[]>();
        public readonly Dictionary<string, Perk> PerkData = new Dictionary<string, Perk>();

        public static GameData LoadData()
        {
            TextAsset textAsset = Resources.Load<TextAsset>("gamedata");
            string rawJsonData = textAsset.text;

            var parsedData = SimpleJSON.JSON.Parse(rawJsonData);
            GameData gameData = new GameData();

            foreach (var item in parsedData["items"])
            {
                gameData.ItemData.Add(item.Key, new ItemData()
                {
                    Name = item.Value["name"],
                    AssetPath = item.Value["assetpath"],
                    Weight = item.Value["weight"].AsInt,
                    MeleeDamage = item.Value["damM"].AsInt,
                    ThrowingDamage = item.Value["damT"].AsInt,
                    Defence = item.Value["def"].AsInt,
                    Description = item.Value["desc"],
                    ItemPrefab = Resources.Load<GameObject>(item.Value["assetpath"]),
                    Nutrition = item.Value["nutrition"] != null ? item.Value["nutrition"].AsInt : 0
                });
            }

            foreach (var cre in parsedData["creatures"])
            {
                gameData.CreatureData.Add(cre.Key, new CreatureData()
                {
                    Name = cre.Value["name"],
                    AssetPath = cre.Value["assetpath"],
                    MaxHp = cre.Value["maxhp"].AsInt,
                    Speed = cre.Value["speed"].AsInt,
                    BaseDamage = cre.Value["basedamage"].AsInt,
                    CreatureLevel = cre.Value["crelevel"] != null ? cre.Value["crelevel"].AsInt : 1,
                    CreaturePrefab = Resources.Load<GameObject>(cre.Value["assetpath"])
                });
            }

            foreach (var parsedPerk in parsedData["perks"])
            {
                string key = parsedPerk.Key;

                Perk perk = new Perk();
                perk.Requirement = "";
                perk.Name = parsedPerk.Value["name"];
                perk.Description = parsedPerk.Value["desc"];

                var gains = new Dictionary<string, PerkValue>();

                foreach (var attrib in parsedPerk.Value)
                {
                    if (attrib.Key == "name" || attrib.Key == "desc")
                    {
                        continue;
                    }

                    if (attrib.Key == "require")
                    {
                        perk.Requirement = attrib.Value;
                        continue;
                    }

                    if (attrib.Value.IsBoolean)
                    {
                        gains.Add(attrib.Key, new PerkValue() { BoolValue = attrib.Value.AsBool });
                    }
                    else if (attrib.Value.IsNumber)
                    {
                        gains.Add(attrib.Key, new PerkValue() { NumberValue = attrib.Value.AsFloat });
                    }
                    else
                    {
                        Debug.LogError("Unsupported perk attrib type for " + attrib.Key);
                    }
                }
            }

            return gameData;
        }
    }
}