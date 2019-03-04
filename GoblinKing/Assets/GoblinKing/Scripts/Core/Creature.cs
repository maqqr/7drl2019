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
        public List<InventoryItem> Inventory = new List<InventoryItem>();

        // Variables for keeping 3D model in sync
        public float TransitionSlowness = 0.3f; // TODO: should this be affected by creature speed?
        private Vector3 velocity = Vector3.zero; // Velocity calculated by Vector3.SmoothDamp

        [SerializeField]
        private Animator animator;
        private int movingParam = Animator.StringToHash("moving");

        public bool InSync
        {
            get
            {
                return Vector3.Distance(Utils.ConvertToWorldCoord(Position), transform.position) < 0.1f;
            }
        }

        public int Speed
        {
            get
            {
                // TODO: should inventory weight affect speed?
                return Data.Speed;
            }
        }

        public void AddItem(string newItemKey)
        {
            for (int i=0; i<Inventory.Count; i++)
            {
                if (Inventory[i].ItemKey == newItemKey)
                {
                    Inventory[i].Count++;
                    return;
                }
            }
            Inventory.Add(new InventoryItem() { ItemKey = newItemKey, Count = 1 });
        }

        private void Awake()
        {
            // Get Animator component from the first child object
            if (transform.childCount > 0)
            {
                animator = transform.GetChild(0).GetComponent<Animator>();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            // Keep the 3D model's world coordinates in sync with game coordinates
            Vector3 targetPosition = Utils.ConvertToWorldCoord(Position);
            transform.localPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, TransitionSlowness);

            // Turn the model to face movement direction
            if (!InSync)
            {
                // TODO: implement
            }

            // Update animator parameters
            if (animator)
            {
                animator.SetBool(movingParam, !InSync);
            }
        }
    }

}