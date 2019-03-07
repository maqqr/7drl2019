using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{

    public class PSAutoDestroy : MonoBehaviour
    {
        private ParticleSystem ps;

        private void Start()
        {
            ps = GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            if (ps && !ps.IsAlive())
            {
                Destroy(gameObject);
            }
        }
    }
}
