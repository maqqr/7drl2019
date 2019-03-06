using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class DownStairs : MonoBehaviour, IInteraction
    {
        public bool Interact(GameManager gameManager)
        {
            // TODO: implement
            Debug.Log("Going down stairs");

            return false;
        }
    }
}