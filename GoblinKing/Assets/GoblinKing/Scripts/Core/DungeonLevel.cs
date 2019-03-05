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

    internal class DungeonLevel : MonoBehaviour
    {
        public ReferenceList<Creature> EnemyCreatures = new ReferenceList<Creature>();
        public ReferenceList<LightSource> LightSources = new ReferenceList<LightSource>();

        public Bounds Bounds;

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

        public void CalculateBounds()
        {
            Bounds = new Bounds(Vector3.zero, Vector3.zero);
            Renderer[] mfs = GetComponentsInChildren<Renderer>();

            for (int i = 0; i < mfs.Length; i++)
            {
                var mf = mfs[i];
                Vector3 pos = mf.transform.position;
                Bounds childBounds = mf.bounds;
                childBounds.center = pos;
                Bounds.Encapsulate(childBounds);
            }
        }
    }
}