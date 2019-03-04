using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Data
{
    [System.Serializable]
    public class ItemData
    {
        public string Name;
        public int Weight;

        public string AssetPath;
        public GameObject ItemPrefab;
    }
}