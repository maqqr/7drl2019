using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing
{
    public class SpinningSymbol : MonoBehaviour
    {
        private float creationTime;

        void Start()
        {
            creationTime = Time.time;
        }

        void Update()
        {
            // TODO: fade to transparent
            // TODO: parametrize magic numbers

            if (Time.time > creationTime + 2f)
            {
                GameObject.Destroy(gameObject);
            }

            transform.Translate(new Vector3(0f, Time.deltaTime * 0.05f, 0f), Space.World);
            transform.Rotate(0f, 0f, Time.deltaTime * 180f);
        }
    }
}