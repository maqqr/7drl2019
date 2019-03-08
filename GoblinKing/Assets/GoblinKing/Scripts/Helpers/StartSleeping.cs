using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    [RequireComponent(typeof(Rigidbody))]
    public class StartSleeping : MonoBehaviour
    {
        private void Awake()
        {
            // Sleep does not work well enough
            // GetComponent<Rigidbody>().Sleep();

            GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}