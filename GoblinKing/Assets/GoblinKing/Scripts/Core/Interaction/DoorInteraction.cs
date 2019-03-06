using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class DoorInteraction : MonoBehaviour, IInteraction
    {
        public bool Interact(GameManager gameManager)
        {
            Debug.Log("Door open");

            return true;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}