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

        private float playerTransitionSpeed = 0.3f;
        private float originalTransitionSpeed = 0.3f;

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
            if (gameManager.IsPlayerDead())
            {
                return true;
            }

            if (forcedCooldown >= 0f)
            {
                forcedCooldown -= Time.deltaTime;
            }

            CheckFastMovement();
            CheckHeartContainerUpdate();

            bool playerCanAct = gameManager.playerObject.GetComponent<Creature>().InSync && forcedCooldown < 0f;
            if (playerCanAct)
            {
                gameManager.UpdateGameWorld();

                if (gameManager.IsPlayerDead())
                {
                    return true;
                }
                HandlePlayerInput();

                if (Utils.IsDown(gameManager.keybindings.MoveForward))
                {
                    gameManager.playerAnim.ForwardKeyHeldDuration += Time.deltaTime;
                }
            }

            if (Utils.IsReleased(gameManager.keybindings.MoveForward))
            {
                gameManager.playerAnim.ForwardKeyHeldDuration = 0f;
            }

            Creature displayEnemy = null;
            RaycastHit hitInfo;
            if (Physics.Raycast(gameManager.Camera.transform.position, gameManager.Camera.transform.forward, out hitInfo, 5f))
            {
                gameManager.throwable = hitInfo.collider.gameObject.GetComponent<Rigidbody>();
                const float pickupDistance = 1.5f;

                var pickup = hitInfo.collider.gameObject.GetComponent<Interaction.Interactable>();
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

            gameManager.EnemyNameText.text = displayEnemy ? displayEnemy.Data.Name : "";
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
            if (Input.GetKeyDown(KeyCode.End))
            {
                Vector2Int spawnpos = gameManager.playerObject.GetComponent<Creature>().Position;
                gameManager.SpawnCreature("torchgoblin", spawnpos);
            }
            // -----------------------------------------------------------------------

            return false;
        }

        private void HandlePlayerInput()
        {
            Vector2Int? playerMoveTo = null;
            GameObject playerObj = gameManager.playerObject;
            var player = playerObj.GetComponent<Creature>();

            if (Utils.IsDown(gameManager.keybindings.PeekLeft))
            {
                gameManager.playerAnim.Peek = -1;
            }
            else if (Utils.IsDown(gameManager.keybindings.PeekRight))
            {
                gameManager.playerAnim.Peek = 1;
            }
            else
            {
                gameManager.playerAnim.Peek = 0;
            }

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

            if (Utils.IsPressed(gameManager.keybindings.PickUp))
            {
                if (highlightedObject != null)
                {
                    Unhighlight(highlightedObject);
                    var interactable = highlightedObject.GetComponent<Interaction.Interactable>();
                    bool advanceTime = false;
                    for (int i = 0; i < interactable.Interactions.Length; i++)
                    {
                        if (interactable.Interactions[i].Interact(gameManager))
                        {
                            advanceTime = true;
                        }
                    }

                    if (advanceTime)
                    {
                        gameManager.AdvanceTime(player.Speed);
                    }
                }
            }

            if (Utils.IsPressed(gameManager.keybindings.Help))
            {
                gameManager.HelpWindow.SetActive(!gameManager.HelpWindow.activeSelf);
            }

            if (Utils.IsPressed(gameManager.keybindings.Wait))
            {
                gameManager.AdvanceTime(player.Speed);
            }

            if (Utils.IsPressed(gameManager.keybindings.Push) && gameManager.throwable != null)
            {
                if (Vector3.Distance(gameManager.throwable.transform.position, gameManager.playerObject.transform.position) < 1.5f)
                {
                    gameManager.throwable.isKinematic = false;
                    gameManager.throwable.AddForce(new Vector3(gameManager.Camera.transform.forward.x, 0f, gameManager.Camera.transform.forward.z) * 10f, ForceMode.Impulse);
                    gameManager.AdvanceTime(player.Speed);
                }
            }

            if (Utils.IsPressed(gameManager.keybindings.ThrowLeftHand) || Utils.IsPressed(gameManager.keybindings.ThrowRightHand))
            {
                EquipSlot slot = Utils.IsPressed(gameManager.keybindings.ThrowLeftHand) ? EquipSlot.LeftHand : EquipSlot.RightHand;
                if (player.Equipment.ContainsKey(slot))
                {
                    gameManager.PlayerThrowItem(slot);
                }
                else if (highlightedObject != null)
                {
                    // Unhighlight(highlightedObject);
                    var pick = highlightedObject.GetComponent<Interaction.PickupItem>();
                    if (pick)
                    {
                        pick.Interact(gameManager);
                        foreach (var invitem in player.Inventory)
                        {
                            if (invitem.ItemKey == pick.itemKey)
                            {
                                gameManager.PlayerEquip(invitem, slot);
                                break;
                            }
                        }
                        gameManager.AdvanceTime(player.Speed);
                    }


                }
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
                        gameManager.UpdatePlayerVisibility();
                    }
                    else if (creatureBlocking != player)
                    {
                        gameManager.playerAnim.StartAttackAnimation();
                        gameManager.Fight(player, creatureBlocking);
                        gameManager.AdvanceTime(player.Speed);
                        forcedCooldown = 1.0f; // Add a small delay to prevent too fast attack spam
                    }
                }
            }
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

        private void CheckFastMovement()
        {
            if (Utils.IsDown(gameManager.keybindings.MoveForward) && !gameManager.playerAnim.PeekForward)
            {
                if (playerTransitionSpeed > 0.1f)
                {
                    playerTransitionSpeed -= Time.deltaTime * 0.04f;
                    playerTransitionSpeed = Mathf.Max(0.1f, playerTransitionSpeed);
                }
            }
            if (Utils.IsReleased(gameManager.keybindings.MoveForward) || gameManager.playerAnim.PeekForward)
            {
                playerTransitionSpeed = originalTransitionSpeed;
            }
            gameManager.playerCreature.TransitionSlowness = playerTransitionSpeed;
        }

        private void CheckHeartContainerUpdate()
        {
            if (gameManager.PlayerHearts.CurrentLife != gameManager.playerCreature.Hp || gameManager.PlayerHearts.CurrentMaxLife != gameManager.playerCreature.MaxLife)
            {
                gameManager.UpdateHearts(gameManager.playerCreature, gameManager.PlayerHearts);
            }
        }
    }
}