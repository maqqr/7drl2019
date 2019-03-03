using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal class ReferenceList<T>
    {
        private List<T> items = new List<T>();

        public List<T> Items
        {
            get
            {
                for (var i = items.Count - 1; i > -1; i--)
                {
                    // TODO: this is a stupid workaround, destroyed Unity components are not actually null, but appear null
                    if (items[i].ToString() == "null")
                    {
                        items.RemoveAt(i);
                    }
                }
                return items;
            }
        }
    }

    /// <summary>
    /// Mostly keeps track of object references so they do not need to be fetched every frame.
    /// <summary>
    internal class DungeonLevel : MonoBehaviour
    {
        public ReferenceList<Creature> EnemyCreatures = new ReferenceList<Creature>();
        public ReferenceList<LightSource> LightSources = new ReferenceList<LightSource>();

        public void UpdateAllReferences()
        {
            Fetch<Creature>(EnemyCreatures.Items);
            Fetch<LightSource>(LightSources.Items);
        }

        public void Fetch<T>(List<T> list)
        {
            list.Clear();
            GetComponentsInChildren<T>(false, list);
        }
    }
}