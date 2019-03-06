using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class PickupItem : MonoBehaviour, IInteraction
    {
        public string itemKey = "";

        private void Awake()
        {
            if (string.IsNullOrEmpty(itemKey))
            {
                Debug.LogError(gameObject.name + ": PickupItem is missing " + itemKey);
            }
        }

        public bool Interact(GameManager gameManager)
        {
            gameManager.playerObject.GetComponent<Creature>().AddItem(itemKey);
            GameObject.Destroy(gameObject);

            return true;
        }
    }
}