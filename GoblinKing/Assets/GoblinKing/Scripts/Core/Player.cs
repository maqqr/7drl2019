using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal class Player : MonoBehaviour
    {
        public VisibilityLevel CurrentVisibility;
        public int Nutrition = 300;

        public int Experience = 0;
        public int Level = 1;
        public int Perkpoints = 2;

        public void addExperience(int xp)
        {
            Experience += xp;
            checkLevelUp();
        }

        public void checkLevelUp() {
            if(Experience >= 100) {
                Experience = 0;
                Level += 1;
                Perkpoints += Level % 3 == 0 ? 1 : 0;
                Debug.Log("Level UP!");
            }
        }
    }
}