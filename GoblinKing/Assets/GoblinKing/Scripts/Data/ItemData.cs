﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Data
{
    [System.Serializable]
    public class ItemData
    {
        public string Name;
        public int Weight;
        public int MeleeDamage;
        public int ThrowingDamage;
        public int Defence;
        public string Description;
        public string AssetPath;
        public int Nutrition;
        public int Healing;
        public int Experience;
        public bool Breakable;
        public int Poisoning;

        public GameObject ItemPrefab;
    }
}