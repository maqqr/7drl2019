using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    public class Interactable : MonoBehaviour
    {
        public IInteraction[] Interactions;

        private void Awake()
        {
            Interactions = GetComponents<IInteraction>();
        }
    }
}