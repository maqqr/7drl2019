﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoblinKing.Helpers;
using LevelGen = LevelGenerator.Scripts.LevelGenerator;

namespace GoblinKing.Core
{
    public class GameManager : MonoBehaviour
    {
        public Keybindings keybindings;
        public GameObject playerObject;
        internal Creature playerCreature;
        public PlayerAnimation playerAnim;
        public GameObject visibilityDiamondObject;
        public Data.GameData GameData;
        public Camera Camera;
        public HeartContainer PlayerHearts;
        public HeartContainer EnemyHearts;
        public HungerContainer HungerContainer;
        public TMPro.TextMeshProUGUI EnemyNameText;
        public MessageBuffer MessageBuffer;

        public GameObject[] levelGeneratorPrefabs;
        public GameObject perkTreePrefab;
        public GameObject inventoryPrefab;
        public GameObject inventoryGuiItemPrefab;
        public GameObject questionMarkPrefab;
        public GameObject exclamationMarkPrefab;
        public GameObject playerPrefab;
        public GameObject smokeCloudPrefab;

        private int currentFloor = -1;
        private List<GameObject> dungeonFloors = new List<GameObject>();
        private Stack<GameViews.IGameView> gameViews = new Stack<GameViews.IGameView>();
        private Collider[] raycastResult = new Collider[1];
        private int advanceTime = 0; // How much time should be advanced due to player actions

        private bool pathfindDirty = false;
        private int lightingDirty = 0;
        private Pathfinding.DungeonGrid pathfindingGrid;


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
            pathfindingGrid.CreateGrid(minX, maxX, minY, maxY, delegate (Vector2Int pos)
            {
                LayerMask mask = ~LayerMask.GetMask("Player", "Enemy");
                return playerCreature.Position == pos || IsWalkable(pos, mask);
            });
        }

        public List<Vector2Int> FindPath(Vector2Int from, Vector2Int to)
        {
            System.Func<Vector2Int, Vector2Int, bool> isWalkableFrom = delegate (Vector2Int start, Vector2Int end)
            {
                // TODO: this could use some caching
                return IsWalkableFrom(start, end, LayerMask.NameToLayer("Player"));
            };

            return Pathfinding.Pathfinding.FindPath(pathfindingGrid, from, to, isWalkableFrom);
        }

        public void UpdatePlayerVisibility()
        {
            List<LightSource> lights = CurrentFloorObject.GetComponent<DungeonLevel>().LightSources.Items;

            Vector3 playerWorldPos = Utils.ConvertToWorldCoord(playerCreature.Position) + new Vector3(0f, 0.5f, 0f);
            VisibilityLevel level = Visibility.Calculate(playerWorldPos, lights);
            playerObject.GetComponent<Player>().CurrentVisibility = level;
            visibilityDiamondObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Visibility.GetGemColor(level));
        }

        public void MakeALoudNoise(Vector3 noisePosition)
        {
            List<Creature> creatures = CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items;
            for (int i = 0; i < creatures.Count; i++)
            {
                Creature cre = creatures[i];
                if (Vector3.Distance(noisePosition, Utils.ConvertToWorldCoord(cre.Position)) < 6f)
                {
                    if (cre.AlertLevel != AI.AlertLevel.Alerted)
                    {
                        AI.AIBehaviour.ChangeAlertness(this, cre, AI.AlertLevel.Suspicious);
                        cre.SuspiciousPosition = Utils.ConvertToGameCoord(noisePosition);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (playerObject == null)
            {
                return;
            }

            // if (CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items.Count == 0)
            // {
            //     return;
            // }

            // var creature = CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items[0];

            // var result = FindPath(creature.Position, playerObject.GetComponent<Creature>().Position);

            // foreach (var point in result)
            // {
            //     Vector3 pos = Utils.ConvertToWorldCoord(point);
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawSphere(pos, 0.3f);
            // }

            // foreach (var kv in pathfindingGrid.nodes)
            // {
            //     Vector3 pos = Utils.ConvertToWorldCoord(kv.Key);
            //     Gizmos.color = Color.white;
            //     Gizmos.DrawWireSphere(pos, 0.4f);
            // }
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

            CurrentFloorObject.GetComponent<DungeonLevel>().UpdateLights();

            itemObject.GetComponent<Interaction.PickupItem>().CollidedFast += delegate
            {
                MessageBuffer.AddMessage(Color.magenta, "The falling " + item.Name + " caused a loud noise!");
                MakeALoudNoise(itemObject.transform.position);
            };

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
            creature.PerkSystem = new PerkSystem(GameData);
            creature.Data = data;
            creature.Hp = creature.MaxLife;
            creature.Position = position;

            if (!string.IsNullOrEmpty(creature.InitialLeftHandItem))
            {
                SpawnItemToHand(creature.LeftHandTransform, creature.InitialLeftHandItem);
            }
            if (!string.IsNullOrEmpty(creature.InitialRightHandItem))
            {
                SpawnItemToHand(creature.RightHandTransform, creature.InitialRightHandItem);
            }

            var throwHandler = creature.GetComponentInChildren<RegisterThrownItemCollision>();
            if (throwHandler)
            {
                throwHandler.HitByItem += delegate (string itemKey)
                {
                    Data.ItemData item = GameData.ItemData[itemKey];
                    creature.TakeDamage(item.ThrowingDamage);
                };
            }
            else
            {
                Debug.LogWarning("Creature " + key + " is missing RegisterThrownItemCollision");
            }
        }

        public void SpawnCreatureAtSpawnPoint(CreatureSpawnPoint spawnPoint)
        {
            // TODO: spawn if 1d100 < spawnPoint.SpawnChance

            if (string.IsNullOrEmpty(spawnPoint.SpawnCreature))
            {
                var creatureKeyList = GameData.SpawnList[currentFloor];
                int index = Random.Range(0, creatureKeyList.Length);
                SpawnCreature(creatureKeyList[index], Utils.ConvertToGameCoord(spawnPoint.transform.position));
            }
            else
            {
                SpawnCreature(spawnPoint.SpawnCreature, Utils.ConvertToGameCoord(spawnPoint.transform.position));
            }
        }

        public void SpawnPlayer(Vector2Int position)
        {
            Data.CreatureData data = GameData.CreatureData["player"];

            playerObject = Instantiate(playerPrefab, Utils.ConvertToWorldCoord(position), Quaternion.identity);
            playerObject.transform.position = Utils.ConvertToWorldCoord(position);

            playerCreature = playerObject.GetComponent<Creature>();
            playerCreature.PerkSystem = new PerkSystem(GameData);
            playerCreature.Data = data;
            playerCreature.Hp = playerCreature.MaxLife;
            playerCreature.Position = position;

            Camera = playerObject.GetComponentInChildren<Camera>();
            playerAnim = Camera.GetComponent<PlayerAnimation>();
        }

        public void SpawnItemToHand(Transform hand, string itemKey)
        {
            var itemObject = SpawnItem(itemKey, Vector3.zero, Quaternion.identity);

            var grabChild = itemObject.transform.Find("Grab");
            GameObject.Destroy(itemObject.GetComponentInChildren<Collider>());
            GameObject.Destroy(itemObject.GetComponentInChildren<Rigidbody>());
            GameObject.Destroy(itemObject.GetComponentInChildren<Interaction.PickupItem>());
            itemObject.GetComponentInChildren<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            if (grabChild == null)
            {
                Debug.LogError("Item \"" + itemKey + "\" is missing Grab object!");
            }

            Utils.Alignment(itemObject.transform, grabChild.transform, hand.transform);
            itemObject.transform.parent = hand.transform;
        }

        internal void PlayerEquip(InventoryItem item, EquipSlot slot)
        {
            var player = playerCreature;
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

            var handObj = GetEquipTransformForSlot(playerCreature, slot);
            SpawnItemToHand(handObj.transform, item.ItemKey);
        }

        internal void PlayerUnequip(EquipSlot slot)
        {
            var player = playerCreature;
            player.Equipment.Remove(slot);
            var handObj = GetEquipTransformForSlot(playerCreature, slot);

            for (int i = handObj.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(handObj.transform.GetChild(i).gameObject);
            }
        }

        private Transform GetEquipTransformForSlot(Creature creature, EquipSlot slot)
        {
            // TODO: move to Creature.cs
            return slot == EquipSlot.LeftHand ? creature.LeftHandTransform : creature.RightHandTransform;
        }

        public bool IsWalkable(Vector2Int position)
        {
            LayerMask mask = ~0;
            return IsWalkable(position, mask);
        }

        internal Creature GetCreatureAt(Vector2Int position)
        {
            if (position == playerCreature.Position)
            {
                return playerCreature;
            }

            List<Creature> creatures = CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items;
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].Position == position)
                {
                    return creatures[i];
                }
            }
            return null;
        }

        public bool IsWalkable(Vector2Int position, LayerMask ignoreMask)
        {
            // Use sphere cast to check that there is enough free space
            Vector3 worldPosition = Utils.ConvertToWorldCoord(position) + new Vector3(0f, 0.5f, 0f);
            int hits = Physics.OverlapSphereNonAlloc(worldPosition, 0.2f, raycastResult, ignoreMask, QueryTriggerInteraction.Ignore);
            bool noObstacles = hits == 0;

            // Check ground to prevent moving to tiles outside of map
            LayerMask groundMask = Physics.DefaultRaycastLayers;
            bool hasGroundUnderneath = Physics.Raycast(worldPosition, Vector3.down, 2.0f, groundMask, QueryTriggerInteraction.Ignore);

            return hasGroundUnderneath && noObstacles;
        }

        public bool IsWalkableFrom(Vector2Int from, Vector2Int to)
        {
            return IsWalkableFrom(from, to, ~0);
        }

        public bool IsWalkableFrom(Vector2Int from, Vector2Int to, LayerMask ignoreMask)
        {
            // TODO: check that manhattan distance between from and to is not greater than 1?

            Vector3 fromWorld = Utils.ConvertToWorldCoord(from) + new Vector3(0f, 0.5f, 0f);
            Vector3 toWorld = Utils.ConvertToWorldCoord(to) + new Vector3(0f, 0.5f, 0f);
            Vector3 raycastDir = toWorld - fromWorld;
            bool wayBlocked = Physics.Raycast(fromWorld, raycastDir, 1f, ignoreMask);

            bool targetSpaceFree = IsWalkable(to, ignoreMask);
            return targetSpaceFree && !wayBlocked;
        }

        public bool IsDungeonValid(GameObject dungeonLevel)
        {
            var spawnPoint = dungeonLevel.transform.GetComponentInChildren<PlayerSpawnPoint>();
            if (spawnPoint == null || spawnPoint.ToString() == "null")
            {
                return false;
            }

            var upstairs = dungeonLevel.transform.GetComponentInChildren<Interaction.UpStairs>();
            if (upstairs == null || upstairs.ToString() == "null")
            {
                return false;
            }

            return true;
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

                int attempt = 0;
                while (attempt < 10)
                {
                    attempt++;
                    Debug.Log("Generation attempt " + attempt);

                    GameObject generatorInstance = Instantiate(generatorPrefab);
                    LevelGen generator = generatorInstance.GetComponent<LevelGen>();

                    // Add empty game object that will be filled with objects generated by the level generator
                    GameObject dungeonFloor = new GameObject("Floor" + currentFloor);
                    dungeonFloor.AddComponent<DungeonLevel>();
                    generator.SectionContainer = dungeonFloor.transform;
                    generator.GenerateLevel();
                    Destroy(generatorInstance);

                    if (IsDungeonValid(dungeonFloor))
                    {
                        dungeonFloors.Add(dungeonFloor);
                        break;
                    }
                    else
                    {
                        // Something went wrong, clean up failed floor
                        Destroy(dungeonFloor);
                    }
                }

                // Dungeon was generated, spawn some monsters next
                var enemySpawnPoints = CurrentFloorObject.transform.GetComponentsInChildren<CreatureSpawnPoint>();
                for (int i = 0; i < enemySpawnPoints.Length; i++)
                {
                    SpawnCreatureAtSpawnPoint(enemySpawnPoints[i]);
                }

                pathfindDirty = true;
            }
            else
            {
                CurrentFloorObject.SetActive(true);
            }

            CurrentFloorObject.GetComponent<DungeonLevel>().UpdateAllReferences();

            var spawnPoint = CurrentFloorObject.transform.GetComponentInChildren<PlayerSpawnPoint>();
            if (spawnPoint)
            {
                Vector2Int point = Utils.ConvertToGameCoord(spawnPoint.transform.position);
                playerObject.transform.position = Utils.ConvertToWorldCoord(point);
                playerCreature.Position = point;
            }
            else
            {
                Debug.LogError("Spawn point not found!");
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

        public void AdjustNutrition(int deltahunger)
        {
            var player = playerObject.GetComponent<Player>();
            player.Nutrition = Mathf.Min(player.Nutrition + deltahunger, player.MaxNutrition);
            HungerContainer.CurrentNutrition = player.Nutrition;
            HungerContainer.CurrentMaxNutrition = player.MaxNutrition;
            HungerContainer.UpdateModels();
            int percent = (int)(100 * (player.Nutrition / (float)player.MaxNutrition));
            if(deltahunger > 0)
            {
                if (percent < 90 && percent >= 70)
                {
                    MessageBuffer.AddMessage(Color.white,"You feel full.");
                }
                if (percent < 70 && percent >= 50)
                {
                    MessageBuffer.AddMessage(Color.white,"You feel content.");
                }
                if (percent < 50 && percent >= 30)
                {
                    MessageBuffer.AddMessage(Color.white,"You feel like you could grab another bite.");
                }
                if (percent < 30 && percent >=10)
                {
                    MessageBuffer.AddMessage(Color.white,"Your stomach still growls.");
                }
                if (percent < 10 && percent >= 0)
                {
                    MessageBuffer.AddMessage(Color.white,"Your stomach is howling in hunger.");
                }
                if (percent < 0)
                {
                    MessageBuffer.AddMessage(Color.white,"You are starving.");
                }
            }
        }

        public void UpdateHunger()
        {
            int deltahp = playerObject.GetComponent<Player>().Nutrition < 1 ? (playerObject.GetComponent<Player>().Nutrition > -10 ? -1 : -2) : 0;
            playerCreature.Hp -= deltahp;
            // TODO: add message when starvation status changes downwards between turns

        }

        public void addExperience(int xp)
        {
            playerObject.GetComponent<Player>().Experience += xp;
            checkLevelUp();
        }

        public void checkLevelUp()
        {
            var player = playerObject.GetComponent<Player>();
            if (player.Experience >= 100)
            {
                player.Experience = 0;
                player.Level += 1;
                player.Perkpoints += player.Level % 3 == 0 ? 1 : 0;
                playerCreature.Data.MaxHp += player.Level % 2 == 0 ? 1 : 0;
                playerCreature.Hp += 1;
                MessageBuffer.AddMessage(Color.green,"Level UP!");
            }
        }

        public void SetMouseLookEnabled(bool enabled)
        {
            Camera.gameObject.GetComponent<SmoothMouseLook>().enabled = enabled;
        }

        private void AdvanceGameWorld(int deltaTime)
        {
            List<Creature> creatures = CurrentFloorObject.GetComponent<DungeonLevel>().EnemyCreatures.Items;

            for (int i = 0; i < creatures.Count; i++)
            {
                Creature cre = creatures[i];
                cre.TimeElapsed += deltaTime;

                while (cre.TimeElapsed >= cre.Speed)
                {
                    cre.TimeElapsed -= cre.Speed;
                    AI.AIBehaviour.UpdateAI(this, cre);
                }
            }
            AdjustNutrition(-1);
            UpdateHunger();
            UpdateHearts(playerCreature, PlayerHearts);
            //Debug.Log("Player nutrition: " + playerObject.GetComponent<Player>().Nutrition);
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

        internal void Fight(Creature attacker, Creature defender)
        {
            attacker.TriggerAttackAnimation();
            AI.AIBehaviour.ChangeAlertness(this, defender, AI.AlertLevel.Alerted);
            defender.SuspiciousPosition = attacker.Position;

            // Damage is sum of meleedmg - sum of defence
            // Some creatures can't hold equips, so they attack with their base dmg. E.g. rats
            // Unarmed dmg is also basedmg
            double atk_left = attacker.Equipment.ContainsKey(EquipSlot.LeftHand) ? GameData.ItemData[attacker.Equipment[EquipSlot.LeftHand].ItemKey].MeleeDamage : attacker.Data.BaseDamage;
            double atk_right = attacker.Equipment.ContainsKey(EquipSlot.RightHand) ? GameData.ItemData[attacker.Equipment[EquipSlot.RightHand].ItemKey].MeleeDamage : attacker.Data.BaseDamage;
            double def_left = defender.Equipment.ContainsKey(EquipSlot.LeftHand) ? GameData.ItemData[defender.Equipment[EquipSlot.LeftHand].ItemKey].Defence : 0;
            double def_right = defender.Equipment.ContainsKey(EquipSlot.RightHand) ? GameData.ItemData[defender.Equipment[EquipSlot.RightHand].ItemKey].Defence : 0;

            // Backstab
            var atkdir = attacker.gameObject.transform.forward;
            var defdir = defender.gameObject.transform.forward;
            var angle = Vector2.Angle(new Vector2(atkdir.x, atkdir.z), new Vector2(defdir.x, defdir.z));
            float multiplier = 1f;

            if (angle < (attacker.PerkSystem.HasPerk("widebackstab") ? 55 : 20))
            {
                multiplier = attacker.PerkSystem.GetMaxFloat("backstabMultiplier", 1.3f);
                MessageBuffer.AddMessage(Color.red, "Backstab!");
            }

            int dmg = (int)System.Math.Max(System.Math.Ceiling((atk_left + atk_right) * multiplier) - (def_left + def_right), 1) + attacker.PerkSystem.GetMaxInt("addDmg", 0);
            defender.TakeDamage(dmg);

            // If player is attacking, award some experience
            bool pcattack = playerObject.GetComponent<Player>().Equals(attacker.GetComponent<Player>());
            int xp = pcattack ? (int)System.Math.Floor(1.5f * (float)System.Math.Max(1, defender.Data.CreatureLevel - playerObject.GetComponent<Player>().Level)) : 0;
            if (defender.Hp < 1)
            {
                foreach (InventoryItem dropped_item in defender.Inventory)
                {
                    System.Random rnd = new System.Random();
                    SpawnItem(dropped_item.ItemKey, Utils.ConvertToWorldCoord(defender.Position) + new Vector3(0, (float)rnd.NextDouble() * 0.6f + 0.2f, 0f), Random.rotation);
                }
                xp += xp > 0 ? System.Math.Max(1, defender.Data.CreatureLevel - playerObject.GetComponent<Player>().Level) * 20 : 0;
                var corpseComponent = defender.gameObject.AddComponent<Corpse>();
                corpseComponent.SmokeCloudPrefab = smokeCloudPrefab;
                GameObject.Destroy(defender.GetComponentInChildren<Collider>());
                if (defender.LeftHandTransform)
                {
                    GameObject.Destroy(defender.LeftHandTransform.gameObject);
                    lightingDirty = 5;
                }
                if (defender.RightHandTransform)
                {
                    GameObject.Destroy(defender.RightHandTransform.gameObject);
                    lightingDirty = 5;
                }
                GameObject.Destroy(defender);
            }
            MessageBuffer.AddMessage(Color.white, attacker.Data.Name + " attacks " + defender.Data.Name + " for " + dmg + " damage.");
            MessageBuffer.AddMessage(Color.white, defender.Data.Name + " has " + defender.Hp + " hp. ");
            if (xp > 0) 
            {
                MessageBuffer.AddMessage(Color.white, attacker.Data.Name + " is awarded " + xp + " xp!");
                 addExperience(xp);
            }
            
        }

        internal void UpdateHearts(Creature creature, HeartContainer container)
        {
            if (creature)
            {
                container.SetLife(creature.Hp);
                container.SetMaxLife(creature.MaxLife);
            }
            else
            {
                container.SetMaxLife(0);
            }
        }

        internal bool IsPlayerDead()
        {
            if (playerCreature)
            {
                return playerCreature.Hp <= 0;
            }
            return true;
        }

        private void Awake()
        {
            keybindings = new Keybindings();
            GameData = Data.GameData.LoadData();
        }

        private void Start()
        {
            SpawnPlayer(Vector2Int.zero);
            NextDungeonFloor();
            AddNewView(new GameViews.InGameView());
            UpdateHearts(playerCreature, PlayerHearts);
        }

        // Update is called once per frame
        private void Update()
        {
            if (gameViews.Count == 0)
            {
                return;
            }

            if (pathfindDirty)
            {
                pathfindDirty = false;
                UpdatePathfindingGrid();
            }

            if (lightingDirty > 0)
            {
                lightingDirty--;
                if (lightingDirty == 0)
                {
                    CurrentFloorObject.GetComponent<DungeonLevel>().UpdateLights();
                    UpdatePlayerVisibility();
                }
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

                if (gameViews.Count == 0)
                {
                    MessageBuffer.AddMessage(Color.red,"GAME OVER");
                }
            }
        }
    }
}