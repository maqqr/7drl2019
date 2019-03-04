using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Data
{
    public class GameData
    {
        public readonly Dictionary<string, ItemData> ItemData = new Dictionary<string, ItemData>();
        public readonly Dictionary<string, CreatureData> CreatureData = new Dictionary<string, CreatureData>();

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
                    ItemPrefab = Resources.Load<GameObject>(item.Value["assetpath"])
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
                    CreaturePrefab = Resources.Load<GameObject>(cre.Value["assetpath"])
                });
            }

            return gameData;
        }
    }
}