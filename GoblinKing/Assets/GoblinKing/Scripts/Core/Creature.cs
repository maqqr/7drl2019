using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoblinKing.AI;

namespace GoblinKing.Core
{
    internal class Creature : MonoBehaviour
    {
        // Game related data
        public int Hp = 0;
        public int TimeElapsed = 0; // Creature takes turn when TimeElapsed > Speed
        public int MaxEncumbrance = 0;
        public int Poison = 0;
        public Vector2Int Position; // Position in game coordinates
        [HideInInspector] public Data.CreatureData Data;
        public List<InventoryItem> Inventory = new List<InventoryItem>();
        public Dictionary<EquipSlot, InventoryItem> Equipment = new Dictionary<EquipSlot, InventoryItem>();
        public PerkSystem PerkSystem;

        public Transform LeftHandTransform = null;
        public Transform RightHandTransform = null;

        public string InitialLeftHandItem = "";
        public string InitialRightHandItem = "";
        public List<string> InitialInventory = new List<string>();

        // AI
        public AIType AIType = AIType.Still;
        public AlertLevel AlertLevel = AlertLevel.Unaware;
        public Vector2Int SuspiciousPosition;
        public Vector2Int PatrolTarget;
        public int PatrolAttemptsLeft = 0;

        // Variables for keeping 3D model in sync
        public float TransitionSlowness = 0.3f; // TODO: should this be affected by creature speed?
        private Vector3 velocity = Vector3.zero; // Velocity calculated by Vector3.SmoothDamp

        [SerializeField]
        private Animator animator;
        private int animMovingParam = Animator.StringToHash("moving");
        private int animHitTrigger = Animator.StringToHash("hitTrigger");
        private int animDieTrigger = Animator.StringToHash("dieTrigger");
        private int animAttackTrigger = Animator.StringToHash("attackTrigger");

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

        public int MaxLife
        {
            get
            {
                return Data.MaxHp + PerkSystem.GetMaxInt("addMaxLife");
            }
        }

        public int MaxEnc
        {
            get
            {
                return Data.MaxEncumbrance + PerkSystem.GetMaxInt("addMaxEnc");
            }
        }

        public void TriggerAttackAnimation()
        {
            if (animator)
            {
                animator.SetTrigger(animAttackTrigger);
            }
        }

        public void TakeDamage(int damage)
        {
            if(damage < 0) damage *=-1;
            Hp -= damage;
            if (animator)
            {
                if (Hp <= 0)
                {
                    animator.SetTrigger(animDieTrigger);
                }
                else
                {
                    animator.SetTrigger(animHitTrigger);
                }
            }
        }

        public void RecoverHealth(int health)
        {
            Hp = System.Math.Min(Hp+health, MaxLife);
        }

        public InventoryItem GetItemFromInventory(string itemKey)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                if (Inventory[i].ItemKey == itemKey)
                {
                    return Inventory[i];
                }
            }
            return null;
        }

        public bool HasItemInSlot(InventoryItem item, EquipSlot slot)
        {
            return Equipment.ContainsKey(slot) && Equipment[slot] == item;
        }

        public void AddItem(string newItemKey)
        {
            for (int i = 0; i < Inventory.Count; i++)
            {
                if (Inventory[i].ItemKey == newItemKey)
                {
                    Inventory[i].Count++;
                    return;
                }
            }
            Inventory.Add(new InventoryItem() { ItemKey = newItemKey, Count = 1 });
        }

        public void RemoveItem(InventoryItem item, int removeCount)
        {
            item.Count--;

            if (item.Count == 0)
            {
                Inventory.Remove(item);
            }
        }

        public void TurnTowards(Vector2Int position)
        {
            if (position != Position)
            {
                transform.rotation = Quaternion.LookRotation(Utils.ConvertToWorldCoord(position) - transform.position, new Vector3(0,1f,0));
            }
        }

        private void Awake()
        {
            // Get Animator component from the first child object
            if (transform.childCount > 0)
            {
                animator = transform.GetChild(0).GetComponent<Animator>();
            }
        }

        private void Start()
        {
            if (InitialLeftHandItem.Length > 0)
            {
                var item = new InventoryItem() { ItemKey = InitialLeftHandItem, Count = 1 };
                Equipment.Add(EquipSlot.LeftHand, item);
                Inventory.Add(item);
            }
            if (InitialRightHandItem.Length > 0)
            {
                var item = new InventoryItem() { ItemKey = InitialRightHandItem, Count = 1 };
                Equipment.Add(EquipSlot.RightHand, item);
                Inventory.Add(item);
            }
            foreach (string item in InitialInventory)
            {
                AddItem(item);
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
                animator.SetBool(animMovingParam, !InSync);
            }
        }
    }

}