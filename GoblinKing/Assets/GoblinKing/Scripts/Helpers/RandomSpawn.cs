using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class RandomSpawn : MonoBehaviour
    {
        public int SpawnChance = 50;

        void Awake()
        {
            if (Random.Range(0, 100) >= SpawnChance)
            {
                GameObject.Destroy(gameObject);
            }
        }
    }
}