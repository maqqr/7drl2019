using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{

    public class HungerContainer : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer[] hungerMeshes;

        public int CurrentNutrition = 0;
        public int CurrentMaxNutrition = 0;

        public void UpdateModels()
        {
            if (CurrentMaxNutrition == 0)
            {
                Debug.LogError("Max nutrition == 0");
                return;
            }
            int percent = (int)(100 * (CurrentNutrition / (float)CurrentMaxNutrition));

            // 90
            // 70
            // 50
            // 30
            // 10

            int modelIndex = 0;

            if (percent < 90)
            {
                modelIndex = 1;
            }
            if (percent < 70)
            {
                modelIndex = 2;
            }
            if (percent < 50)
            {
                modelIndex = 3;
            }
            if (percent < 30)
            {
                modelIndex = 4;
            }
            if (percent < 10)
            {
                modelIndex = 5;
            }
            if (percent < 0)
            {
                modelIndex = 6;
            }

             for (int i = 0; i < hungerMeshes.Length; i++)
            {
                hungerMeshes[i].enabled = i == modelIndex;
            }
        }

        private void Awake()
        {
            hungerMeshes = GetComponentsInChildren<MeshRenderer>();
            UpdateModels();
        }

        private void Update()
        {
        }
    }
}
