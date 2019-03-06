using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    internal class DoorInteraction : MonoBehaviour, IInteraction
    {
        public Transform OpenTransform;
        public Transform ClosedTransform;

        public bool IsOpen;

        public bool Interact(GameManager gameManager)
        {
            IsOpen = !IsOpen;

            if (IsOpen)
            {
                transform.parent.position = OpenTransform.position;
                transform.parent.rotation = OpenTransform.rotation;
            }
            else
            {
                transform.parent.position = ClosedTransform.position;
                transform.parent.rotation = ClosedTransform.rotation;
            }

            return true;
        }

        void Update()
        {
            // TODO: make door opening smooth
        }
    }
}