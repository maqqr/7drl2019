using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class PickupItem : MonoBehaviour, IInteraction
    {
        public string itemKey = "";
        public event System.Action CollidedFast;

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
            if(gameManager.playerCreature.MaxEnc >= Utils.TotalEncumbrance(gameManager, gameManager.playerCreature) + gameManager.GameData.ItemData[itemKey].Weight) {
                gameManager.playerObject.GetComponent<Creature>().AddItem(itemKey);
                GameObject.Destroy(gameObject);
                gameManager.MessageBuffer.AddMessage(Color.white, "You picked up the " + gameManager.GameData.ItemData[itemKey].Name.ToLower()+".");
            }
            else {
                gameManager.MessageBuffer.AddMessage(Color.white, "You can't carry any more loot.");
            }
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
                        CollidedFast();
                    }
                }
            }
        }
    }
}