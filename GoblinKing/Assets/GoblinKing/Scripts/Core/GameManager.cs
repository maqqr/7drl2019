﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelGen = LevelGenerator.Scripts.LevelGenerator;

namespace GoblinKing.Core
{
    public class GameManager : MonoBehaviour
    {
        public Keybindings keybindings;
        public GameObject playerObject;
        public GameObject visibilityDiamondObject;
        public Data.GameData GameData;
        public Camera Camera;

        public GameObject[] levelGeneratorPrefabs;
        public GameObject perkTreePrefab;
        public GameObject inventoryPrefab;
        public GameObject inventoryGuiItemPrefab;

        private int currentFloor = -1;
        private List<GameObject> dungeonFloors = new List<GameObject>();
        private Stack<GameViews.IGameView> gameViews = new Stack<GameViews.IGameView>();
        private Collider[] raycastResult = new Collider[1];
        private int advanceTime = 0; // How much time should be advanced due to player actions

        [SerializeField]
        private bool pathfindDirty = false;
        private Pathfinding.DungeonGrid pathfindingGrid;

        private List<Vector2Int> reservedPlaces = new List<Vector2Int>(); // Prevent creatures from moving inside eachother


        public GameObject CurrentFloorObject
        {
            get
            {
                if (currentFloor >= 0 && currentFloor < dungeonFloors.Count)
                {
                    return dungeonFloors[currentFloor];
                }
                return null;
            }
        }

        public void UpdatePathfindingGrid()
        {
            var dungeonLevel = CurrentFloorObject.GetComponent<DungeonLevel>();
            dungeonLevel.CalculateBounds();
            int minX = (int)Mathf.Round(dungeonLevel.Bounds.min.x);
            int maxX = (int)Mathf.Round(dungeonLevel.Bounds.max.x);
            int minY = (int)Mathf.Round(dungeonLevel.Bounds.min.z);
            int maxY = (int)Mathf.Round(dungeonLevel.Bounds.max.z);
            pathfindingGrid = new Pathfinding.DungeonGrid();
            pathfindingGrid.CreateGrid(minX, maxX, minY, maxY, IsWalkable);
        }

        private void OnDrawGizmos()
        {
            if (playerObject == null)
            {
                return;
            }

            var result = Pathfinding.Pathfinding.FindPath(pathfindingGrid, new Vector2Int(0, 0), playerObject.GetComponent<Creature>().Position);

            foreach (var point in result)
            {
                Vector3 pos = Utils.ConvertToWorldCoord(point);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(pos, 0.3f);
            }
        }

        public GameObject SpawnItem(string key, Vector3 position, Quaternion rotation)
        {
            if (!GameData.ItemData.ContainsKey(key))
            {
                Debug.LogError("SpawnItem: invalid key \"" + key + "\"");
                return null;
            }

            Data.ItemData item = GameData.ItemData[key];
            GameObject itemObject = Instantiate(item.ItemPrefab, position, rotation);
            itemObject.transform.parent = CurrentFloorObject.transform;
            return itemObject;
        }

        public void SpawnCreature(string key, Vector2Int position)
        {
            if (!GameData.CreatureData.ContainsKey(key))
            {
                Debug.LogError("SpawnCreature: invalid key \"" + key + "\"");
                return;
            }

            Data.CreatureData data = GameData.CreatureData[key];

            GameObject creatureObject = Instantiate(data.CreaturePrefab, Utils.ConvertToWorldCoord(position), Quaternion.identity);
            creatureObject.transform.position = Utils.ConvertToWorldCoord(position);
            creatureObject.transform.parent = CurrentFloorObject.transform;
            CurrentFloorObject.GetComponent<DungeonLevel>().UpdateAllReferences();

            Creature creature = creatureObject.GetComponent<Creature>();
            creature.Data = data;
            creature.Hp = creature.Data.MaxHp;
            creature.Position = position;
        }

        public void SpawnItemToHand(Transform hand, string itemKey)
        {
            var itemObject = SpawnItem(itemKey, Vector3.zero, Quaternion.identity);

            var grabChild = itemObject.transform.Find("Grab");
            GameObject.Destroy(itemObject.GetComponentInChildren<Collider>());
            GameObject.Destroy(itemObject.GetComponentInChildren<Rigidbody>());
            GameObject.Destroy(itemObject.GetComponentInChildren<PickupItem>());
            itemObject.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            Utils.Alignment(itemObject.transform, grabChild.transform, hand.transform);
            itemObject.transform.parent = hand.transform;
        }

        internal void PlayerEquip(InventoryItem item, EquipSlot slot)
        {
            var player = playerObject.GetComponent<Creature>();
            EquipSlot otherHand = slot == EquipSlot.LeftHand ? EquipSlot.RightHand : EquipSlot.LeftHand;

            // Unequip old item
            if (player.Equipment.ContainsKey(slot))
            {
                PlayerUnequip(slot);
            }

            // One item cannot be held in both hands
            if (item.Count == 1 && player.HasItemInSlot(item, otherHand))
            {
                PlayerUnequip(otherHand);
            }

            player.Equipment[slot] = item;

            var handObj = GetEquipTransformForSlot(slot);
            SpawnItemToHand(handObj.transform, item.ItemKey);
        }

        internal void PlayerUnequip(EquipSlot slot)
        {
            var player = playerObject.GetComponent<Creature>();
            player.Equipment.Remove(slot);
            var handObj = GetEquipTransformForSlot(slot);

            for (int i = handObj.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(handObj.transform.GetChild(i).gameObject);
            }
        }

        private Transform GetEquipTransformForSlot(EquipSlot slot)
        {
            // TODO: this should be done without transform.Find
            var handName = slot == EquipSlot.LeftHand ? "LeftHand" : "RightHand";
            return Camera.gameObject.transform.Find(handName);
        }

        public bool IsWalkable(Vector2Int position)
        {
            // Check that no creature is currently occupying the position
            List<Creature> creatures = CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items;
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].Position == position)
                {
                    return false;
                }
            }

            // Use sphere cast to check that there is enough free space
            Vector3 worldPosition = Utils.ConvertToWorldCoord(position) + new Vector3(0f, 0.5f, 0f);
            int hits = Physics.OverlapSphereNonAlloc(worldPosition, 0.3f, raycastResult, ~0, QueryTriggerInteraction.Ignore);
            bool noObstacles = hits == 0;

            // Check ground to prevent moving to tiles outside of map
            bool hasGroundUnderneath = Physics.Raycast(worldPosition, Vector3.down, 2.0f, ~0, QueryTriggerInteraction.Ignore);

            return hasGroundUnderneath && noObstacles;
        }

        public bool IsWalkableFrom(Vector2Int from, Vector2Int to)
        {
            // TODO: check that manhattan distance between from and to is not greater than 1?

            bool isDiagonal = from.x != to.x && from.y != to.y;

            if (isDiagonal)
            {
                Vector3 fromWorld = Utils.ConvertToWorldCoord(from) + new Vector3(0f, 0.5f, 0f);
                Vector3 toWorld = Utils.ConvertToWorldCoord(to) + new Vector3(0f, 0.5f, 0f);
                Vector3 raycastDir = toWorld - fromWorld;
                bool wayBlocked = Physics.Raycast(fromWorld, raycastDir, 1f);

                bool targetSpaceFree = IsWalkable(to);
                return targetSpaceFree && !wayBlocked;
            }
            else
            {
                return IsWalkable(to);
            }
        }

        public void NextDungeonFloor()
        {
            // Disable current level
            if (CurrentFloorObject != null)
            {
                CurrentFloorObject.SetActive(false);
            }

            // Go up one level
            currentFloor++;

            // Generate dungeon if it does not exist
            if (currentFloor >= dungeonFloors.Count)
            {
                if (levelGeneratorPrefabs.Length == 0)
                {
                    Debug.LogError("levelGeneratorPrefabs are missing!");
                    return;
                }

                // TODO: add some logic to generator prefab selection
                // generator should depend on which floor the player is going
                GameObject generatorPrefab = levelGeneratorPrefabs[0];
                GameObject generatorInstance = Instantiate(generatorPrefab);
                LevelGen generator = generatorInstance.GetComponent<LevelGen>();

                // Add empty game object that will be filled with objects generated by the level generator
                GameObject dungeonFloor = new GameObject("Floor" + currentFloor);
                dungeonFloor.AddComponent<DungeonLevel>();
                dungeonFloors.Add(dungeonFloor);
                generator.SectionContainer = CurrentFloorObject.transform;

                generator.GenerateLevel();

                dungeonFloor.GetComponent<DungeonLevel>().UpdateAllReferences();

                // Level generator has done its job and is no longer needed
                Destroy(generatorInstance);
                pathfindDirty = true;
            }
            else
            {
                CurrentFloorObject.SetActive(true);
            }
        }

        public void PreviousDungeonFloor()
        {
            if (currentFloor == 0)
            {
                Debug.LogWarning("Attempted to go below level 0");
                return;
            }

            // Disable current level
            CurrentFloorObject.SetActive(false);

            // Activate previous level
            currentFloor--;
            CurrentFloorObject.SetActive(true);
        }

        public void AdvanceTime(int deltaTime)
        {
            advanceTime += deltaTime;
        }

        public void UpdateGameWorld()
        {
            if (advanceTime > 0)
            {
                AdvanceGameWorld(advanceTime);
                advanceTime = 0;
            }
        }

        public void SetMouseLookEnabled(bool enabled)
        {
            Camera.gameObject.GetComponent<SmoothMouseLook>().enabled = enabled;
        }

        private void AdvanceGameWorld(int deltaTime)
        {
            List<Creature> creatures = CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items;
            reservedPlaces.Clear();

            for (int i = 0; i < creatures.Count; i++)
            {
                Creature cre = creatures[i];
                cre.TimeElapsed += deltaTime;

                while (cre.TimeElapsed >= cre.Speed)
                {
                    cre.TimeElapsed -= cre.Speed;
                    UpdateCreatureAi(cre);
                }
            }
        }

        private void UpdateCreatureAi(Creature cre)
        {
            // TODO: implement real creature AI instead of random movement
            int randomX = Random.Range(-1, 2);
            int randomY = Random.Range(-1, 2);
            Vector2Int newPos = new Vector2Int(cre.Position.x + randomX, cre.Position.y + randomY);

            if (!reservedPlaces.Contains(newPos) && IsWalkableFrom(cre.Position, newPos))
            {
                cre.Position = newPos;
                reservedPlaces.Add(newPos);
            }
        }

        internal void AddNewView(GameViews.IGameView view)
        {
            if (gameViews.Count > 0)
            {
                gameViews.Peek().CloseView();
            }

            gameViews.Push(view);
            view.Initialize(this);
            view.OpenView();
        }

        private void Awake()
        {
            keybindings = new Keybindings();
            GameData = Data.GameData.LoadData();
            playerObject = FindObjectOfType<Player>().gameObject;
            Camera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        }

        private void Start()
        {
            NextDungeonFloor();
            AddNewView(new GameViews.InGameView());
        }

        // Update is called once per frame
        private void Update()
        {
            if (pathfindDirty)
            {
                pathfindDirty = false;
                UpdatePathfindingGrid();
            }

            bool closeView = gameViews.Peek().UpdateView();

            if (closeView)
            {
                var view = gameViews.Pop();
                view.CloseView();
                view.Destroy();

                if (gameViews.Count > 0)
                {
                    // Re-enable previous view
                    gameViews.Peek().OpenView();
                }

                // TODO: if last view was closed, quit the game
            }
        }
    }
}