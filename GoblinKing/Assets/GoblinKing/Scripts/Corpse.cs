using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing
{
    public class Corpse : MonoBehaviour
    {
        public GameObject SmokeCloudPrefab;

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
                if (SmokeCloudPrefab)
                {
                    if (Core.BackgroundMusic.Instance)
                    {
                        Core.BackgroundMusic.Instance.PlaySoundEffectAt("puff", transform.position);
                    }

                    GameObject.Instantiate(SmokeCloudPrefab, transform.position + new Vector3(0f, 0.1f, 0f), Quaternion.identity);
                }

                GameObject.Destroy(gameObject);
            }
        }
    }
}