using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class DownStairs : MonoBehaviour, IInteraction
    {
        public bool Interact(GameManager gameManager)
        {
            Debug.Log("Going down stairs");
            gameManager.PreviousDungeonFloor();

            return false;
        }
    }
}