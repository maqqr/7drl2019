﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class ExtinguishTorch : MonoBehaviour, IInteraction
    {
        public GameObject FlameObject = null;

        private void Awake()
        {
            if (!FlameObject)
            {
                Debug.LogError(gameObject.name + " is missing FlameObject");
            }
        }

        public bool Interact(GameManager gameManager)
        {
            GameObject.DestroyImmediate(FlameObject);
            gameManager.CurrentFloorObject.GetComponent<DungeonLevel>().UpdateLights();
            gameManager.UpdatePlayerVisibility();

            Destroy(GetComponent<Interactable>());

            // TODO: add small smoke cloud

            return false;
        }
    }
}