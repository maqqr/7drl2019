using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core
{
    internal class Creature : MonoBehaviour
    {
        // Game related data
        public int Hp = 0;
        public int TimeElapsed = 0; // Creature takes turn when TimeElapsed > Speed
        public Vector2Int Position; // Position in game coordinates
        public Data.CreatureData Data;

        // Variables for keeping 3D model in sync
        public float TransitionSlowness = 0.3f; // TODO: should this be affected by creature speed?
        private Vector3 velocity = Vector3.zero; // Velocity calculated by Vector3.SmoothDamp

        public bool InSync
        {
            get
            {
                return Vector3.Distance(Utils.ConvertToWorldCoord(Position), transform.position) < 0.1f;
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
            Vector3 targetPosition = Utils.ConvertToWorldCoord(Position);
            transform.localPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, TransitionSlowness);
        }
    }

}