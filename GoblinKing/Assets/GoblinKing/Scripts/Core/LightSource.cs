using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    /// <summary>
    /// LightSources are for lights that are not purely decorative and affect creature visibility.
    /// </summary>
    internal class LightSource : MonoBehaviour
    {
        public float lightRadius = 0f;
        public bool drawLightRadius = true;

        private new Light light;

        private void Awake()
        {
            light = GetComponent<Light>();
            if (light == null)
            {
                Debug.LogWarning("LightSource without light");
            }
        }

        // Update is called once per frame
        private void Update()
        {
            // TODO: add light flickering
        }

        private void OnDrawGizmos()
        {
            if (drawLightRadius)
            {
                Gizmos.DrawWireSphere(transform.position, lightRadius);
            }
        }
    }
}