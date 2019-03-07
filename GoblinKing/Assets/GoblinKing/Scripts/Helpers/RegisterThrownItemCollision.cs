using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class RegisterThrownItemCollision : MonoBehaviour
    {
        public delegate void HitHandler(string itemKey);
        public event HitHandler HitByItem;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.relativeVelocity.magnitude > 5f)
            {
                var item = collision.collider.GetComponent<Core.Interaction.PickupItem>();
                if (HitByItem != null)
                {
                    HitByItem(item.itemKey);
                }
            }
        }
    }
}
