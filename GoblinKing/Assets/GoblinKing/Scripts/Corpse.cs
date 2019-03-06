using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing
{
    public class Corpse : MonoBehaviour
    {
        private float timeCreated;
        private float disappearDuration = 5f;

        void Start()
        {
            timeCreated = Time.time;
        }

        void Update()
        {
            if (Time.time > timeCreated + disappearDuration)
            {
                // TODO: spawn smoke cloud?

                GameObject.Destroy(gameObject);
            }
        }
    }
}