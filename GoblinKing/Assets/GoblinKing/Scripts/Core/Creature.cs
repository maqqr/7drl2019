using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal class Creature : MonoBehaviour
    {
        public CreatureStats stats = new CreatureStats();
        public Vector2Int position; // Position in game coordinates

        public float transitionSlowness = 0.3f;
        private Vector3 velocity = Vector3.zero; // Velocity calculated by Vector3.SmoothDamp

        public bool InSync
        {
            get
            {
                return Vector3.Distance(Utils.ConvertToWorldCoord(position), transform.position) < 0.1f;
            }
        }

        // Start is called before the first frame update
        private void Start()
        {

        }

        // Update is called once per frame
        private void Update()
        {
            // Keep the 3D model's world coordinates in sync with game coordinates
            Vector3 targetPosition = Utils.ConvertToWorldCoord(position);
            transform.localPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, transitionSlowness);
        }
    }

}