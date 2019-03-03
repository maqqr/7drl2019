using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    /// <summary>
    /// Mostly keeps track of object references so they do not need to be fetched every frame.
    /// <summary>
    internal class DungeonLevel : MonoBehaviour
    {
        public List<Creature> enemyCreatures = new List<Creature>();
        public List<LightSource> lightSources = new List<LightSource>();

        public void UpdateAllReferences()
        {
            Fetch<Creature>(enemyCreatures);
            Fetch<LightSource>(lightSources);
        }

        public void Fetch<T>(List<T> list)
        {
            list.Clear();
            GetComponentsInChildren<T>(false, list);
        }
    }
}