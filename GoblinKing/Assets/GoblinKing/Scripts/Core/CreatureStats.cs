using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    [Serializable]
    public class CreatureStats
    {
        public string Name;

        public int Hp;
        public int MaxHp;

        public int Speed;

        [HideInInspector]
        public int TimeElapsed;
    }
}
