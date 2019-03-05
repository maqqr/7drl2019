using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing
{
    public class Flame : MonoBehaviour
    {
        public float FlickerSpeed = 2f;
        public float FlickerAmount = 0.4f;

        private new Light light;
        private float originalIntensity;

        void Awake()
        {
            light = GetComponentInChildren<Light>();
            if (light)
            {
                originalIntensity = light.intensity;
            }
        }

        // Update is called once per frame
        void Update()
        {
            float newScale = 1f + FlickerAmount * Mathf.PerlinNoise(Time.time * FlickerSpeed, 0f);
            newScale = Mathf.Max(0.01f, newScale);
            transform.localScale = new Vector3(newScale, newScale, newScale);

            if (light)
            {
                light.intensity = newScale * originalIntensity;
            }

            // Make flame always point up regardless of parent rotation
            transform.LookAt(transform.position + Vector3.up, Vector3.forward);
        }
    }
}