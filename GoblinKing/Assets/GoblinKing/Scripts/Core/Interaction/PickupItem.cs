using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class PickupItem : MonoBehaviour, IInteraction
    {
        public string itemKey = "";
        public event System.Action<GameObject> CollidedFast;

        private float lastCollisionTime;

        private void Awake()
        {
            if (string.IsNullOrEmpty(itemKey))
            {
                Debug.LogError(gameObject.name + ": PickupItem is missing " + itemKey);
            }
        }

        public bool Interact(GameManager gameManager)
        {
            gameManager.PlayerPickupItem(this);
            return true;
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 4f)
            {
                if (Time.time > lastCollisionTime + 2f)
                {
                    lastCollisionTime = Time.time;
                    if (CollidedFast != null)
                    {
                        CollidedFast(collision.collider.gameObject);
                    }
                }
            }
        }
    }
}