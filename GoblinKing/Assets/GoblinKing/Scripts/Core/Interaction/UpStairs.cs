using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class UpStairs : MonoBehaviour, IInteraction
    {
        public bool Interact(GameManager gameManager)
        {
            // TODO: implement
            Debug.Log("Going up stairs");

            gameManager.NextDungeonFloor();

            return false;
        }
    }
}