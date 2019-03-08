using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class PickupItem : MonoBehaviour, IInteraction
    {
        public string itemKey = "";
        public event System.Action<GameObject, float> CollidedFast;

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
                if (CollidedFast != null)
                {
                    CollidedFast(collision.collider.gameObject, Time.time - lastCollisionTime);
                }
                lastCollisionTime = Time.time;
            }
        }
    }
}