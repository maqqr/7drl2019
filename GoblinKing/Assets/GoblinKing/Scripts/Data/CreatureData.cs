using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Data
{
    [System.Serializable]
    public class CreatureData
    {
        public string Name;
        public int MaxHp;
        public int Speed;
        public int BaseDamage;
        public int CreatureLevel;

        public string AssetPath;
        public GameObject CreaturePrefab;
    }
}