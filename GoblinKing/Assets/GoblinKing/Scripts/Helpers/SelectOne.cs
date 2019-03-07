using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class SelectOne : MonoBehaviour
    {
        void Awake()
        {
            if (transform.childCount > 0)
            {
                int randomIndex = Random.Range(0, transform.childCount);
                transform.GetChild(randomIndex).SetParent(transform.parent); // Save one from destruction

                Transform[] remainingChildren = GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < remainingChildren.Length; i++)
                {
                    Destroy(remainingChildren[i].gameObject);
                }
            }
        }
    }
}
