using System.Collections.Generic;
using UnityEngine;
using LevelGenerator.Scripts.Helpers;

namespace GoblinKing.Core.GameViews
{
    internal class InGameView : IGameView
    {
        private GameManager gameManager;
        private GameObject highlightedObject = null;

        private float forcedCooldown = 0f;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
            gameManager.SetMouseLookEnabled(true);
        }

        public void CloseView()
        {
            gameManager.SetMouseLookEnabled(false);
        }

        public bool UpdateView()
        {
            if (forcedCooldown >= 0f)
            {
                forcedCooldown -= Time.deltaTime;
            }

            bool playerCanAct = gameManager.playerObject.GetComponent<Creature>().InSync && forcedCooldown < 0f;

            if (playerCanAct)
            {
                gameManager.UpdateGameWorld();
                HandlePlayerInput();
            }

            Creature displayEnemy = null;
            RaycastHit hitInfo;
            if (Physics.Raycast(gameManager.Camera.transform.position, gameManager.Camera.transform.forward, out hitInfo, 5f))
            {
                const float pickupDistance = 1.5f;

                var pickup = hitInfo.collider.gameObject.GetComponent<PickupItem>();
                if (pickup && hitInfo.distance < pickupDistance)
                {
                    HighlightObject(pickup.gameObject);
                }
                else
                {
                    HighlightObject(null);
                }

                displayEnemy = hitInfo.collider.transform.parent?.gameObject.GetComponent<Creature>();
            }

            gameManager.UpdateHearts(displayEnemy, gameManager.EnemyHearts);

            // ----- These are for debugging purposes ---------------------------------
            if (Input.GetKeyDown(KeyCode.PageUp))
            {
                gameManager.NextDungeonFloor();
            }
            if (Input.GetKeyDown(KeyCode.PageDown))
            {
                gameManager.PreviousDungeonFloor();
            }
            if (Input.GetKey(KeyCode.Insert))
            {
                IEnumerable<string> itemKeys = gameManager.GameData.ItemData.Keys;
                Vector3 spawnpos = gameManager.playerObject.transform.position + new Vector3(0f, 0.5f, 0f) + gameManager.playerObject.transform.forward;
                gameManager.SpawnItem(itemKeys.PickOne(), spawnpos, Random.rotation);
            }
            if (Input.GetKeyDown(KeyCode.Home))
            {
                Vector2Int spawnpos = gameManager.playerObject.GetComponent<Creature>().Position;
                gameManager.SpawnCreature("goblin", spawnpos);
            }
            // -----------------------------------------------------------------------

            return false;
        }

        private void HandlePlayerInput()
        {
            Vector2Int? playerMoveTo = null;
            GameObject playerObj = gameManager.playerObject;
            var player = playerObj.GetComponent<Creature>();

            if (Utils.IsDown(gameManager.keybindings.MoveForward))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition + playerObj.transform.forward);
            }
            else if (Utils.IsDown(gameManager.keybindings.MoveBackward))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition - playerObj.transform.forward);
            }
            else if (Utils.IsDown(gameManager.keybindings.MoveRight))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition + playerObj.transform.right);
            }
            else if (Utils.IsDown(gameManager.keybindings.MoveLeft))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition - playerObj.transform.right);
            }

            if (Utils.IsDown(gameManager.keybindings.PickUp))
            {
                if (highlightedObject != null)
                {
                    string itemKey = highlightedObject.GetComponent<PickupItem>().itemKey;
                    player.AddItem(itemKey);
                    Unhighlight(highlightedObject);
                    GameObject.Destroy(highlightedObject);
                    gameManager.AdvanceTime(player.Speed);
                }
            }

            if (Utils.IsPressed(gameManager.keybindings.ThrowLeftHand) || Utils.IsPressed(gameManager.keybindings.ThrowRightHand))
            {
                EquipSlot slot = Utils.IsPressed(gameManager.keybindings.ThrowLeftHand) ? EquipSlot.LeftHand : EquipSlot.RightHand;
                ThrowItem(slot);
            }

            if (Utils.IsPressed(gameManager.keybindings.OpenPerkTree))
            {
                gameManager.AddNewView(new PerkTreeView());
            }

            if (Utils.IsPressed(gameManager.keybindings.OpenInventory))
            {
                gameManager.AddNewView(new InventoryView());
            }

            if (playerMoveTo != null)
            {
                LayerMask mask = ~LayerMask.GetMask("Player", "Enemy");
                if (gameManager.IsWalkableFrom(player.Position, playerMoveTo.Value, mask))
                {
                    Creature creatureBlocking = gameManager.GetCreatureAt(playerMoveTo.Value);
                    if (creatureBlocking == null)
                    {
                        player.Position = playerMoveTo.Value;
                        gameManager.AdvanceTime(player.Speed);
                        UpdatePlayerVisibility();
                    }
                    else if (creatureBlocking != player)
                    {
                        gameManager.Fight(player, creatureBlocking);
                        gameManager.AdvanceTime(player.Speed);
                        forcedCooldown = 1.0f; // Add a small delay to prevent too fast attack spam
                    }
                }
            }
        }

        private void ThrowItem(EquipSlot slot)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            if (player.Equipment.ContainsKey(slot))
            {
                var removedItem = player.Equipment[slot];
                player.RemoveItem(removedItem, 1);

                if (removedItem.Count == 1 && player.HasItemInSlot(removedItem, EquipSlot.LeftHand) && player.HasItemInSlot(removedItem, EquipSlot.RightHand))
                {
                    gameManager.PlayerUnequip(slot);
                }

                if (removedItem.Count <= 0)
                {
                    gameManager.PlayerUnequip(slot);
                }

                Vector3 spawnPos = Utils.ConvertToWorldCoord(player.Position) + new Vector3(0f, 0.6f, 0f)
                                 + player.gameObject.transform.forward * 0.3f;
                var spawnedItem = gameManager.SpawnItem(removedItem.ItemKey, spawnPos, Random.rotation);

                var rigidbody = spawnedItem.GetComponent<Rigidbody>();
                rigidbody.isKinematic = false;
                rigidbody.AddForce(gameManager.Camera.transform.forward * 10f, ForceMode.Impulse);
                gameManager.AdvanceTime(gameManager.playerObject.GetComponent<Creature>().Speed);
            }
        }

        private void UpdatePlayerVisibility()
        {
            List<LightSource> lights = gameManager.CurrentFloorObject.GetComponent<DungeonLevel>().LightSources.Items;

            Vector3 playerWorldPos = Utils.ConvertToWorldCoord(gameManager.playerObject.GetComponent<Creature>().Position) + new Vector3(0f, 0.5f, 0f);
            VisibilityLevel level = Visibility.Calculate(playerWorldPos, lights);
            gameManager.playerObject.GetComponent<Player>().CurrentVisibility = level;
            gameManager.visibilityDiamondObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Visibility.GetGemColor(level));
        }

        private void HighlightObject(GameObject obj)
        {
            if (obj == highlightedObject)
            {
                return;
            }

            if (obj == null)
            {
                if (highlightedObject != null)
                {
                    Unhighlight(highlightedObject);
                }
                highlightedObject = null;
                return;
            }

            if (highlightedObject != null)
            {
                Unhighlight(highlightedObject);
            }

            MeshRenderer rend = obj.GetComponentInChildren<MeshRenderer>();
            rend.material.SetColor("_RimColor", new Color(1f, 1f, 1f));
            rend.material.SetFloat("_RimIntensity", 1f);
            rend.material.SetFloat("_RimSize", 1f);
            rend.material.SetFloat("_RimSmoothness", 1f);
            highlightedObject = obj;
        }

        private void Unhighlight(GameObject obj)
        {
            MeshRenderer rend = obj.GetComponentInChildren<MeshRenderer>();
            rend.material.SetColor("_RimColor", new Color(0f, 0f, 0f));
            rend.material.SetFloat("_RimIntensity", 0f);
            rend.material.SetFloat("_RimSize", 0f);
            rend.material.SetFloat("_RimSmoothness", 0f);
        }
    }
}