using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal class Player : MonoBehaviour
    {
        public VisibilityLevel CurrentVisibility;
        public int Nutrition = 300;
        public int MaxNutrition = 500;

        public int Experience = 0;
        public int Level = 1;
        public int Perkpoints = 2;

        
    }
}