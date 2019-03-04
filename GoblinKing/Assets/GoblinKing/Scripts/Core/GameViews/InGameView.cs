using System.Collections.Generic;
using UnityEngine;
using LevelGenerator.Scripts.Helpers;

namespace GoblinKing.Core.GameStates
{
    internal class InGameView : IGameView
    {
        private GameManager gameManager;

        private int advanceTime = 0; // How much time should be advanced due to player actions

        private GameObject highlightedObject = null;

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
            if (gameManager.playerObject.GetComponent<Creature>().InSync)
            {
                if (advanceTime > 0)
                {
                    gameManager.AdvanceGameWorld(advanceTime);
                    advanceTime = 0;
                }

                HandlePlayerInput();
            }

            RaycastHit hitInfo;
            if (Physics.Raycast(gameManager.Camera.transform.position, gameManager.Camera.transform.forward, out hitInfo, 1.5f))
            {
                var pickup = hitInfo.collider.gameObject.GetComponent<PickupItem>();
                if (pickup)
                {
                    HighlightObject(pickup.gameObject);
                }
                else
                {
                    HighlightObject(null);
                }
            }

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
                    string itemKey = highlightedObject.GetComponent<PickupItem>().itemKey;
                    gameManager.playerObject.GetComponent<Creature>().Inventory.Add(new InventoryItem() { ItemKey = itemKey, Count = 1 });
                    Unhighlight(highlightedObject);
                    GameObject.Destroy(highlightedObject);
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
                if (gameManager.IsWalkableFrom(playerObj.GetComponent<Creature>().Position, playerMoveTo.Value))
                {
                    gameManager.playerObject.GetComponent<Creature>().Position = playerMoveTo.Value;
                    advanceTime = gameManager.playerObject.GetComponent<Creature>().Speed;
                    UpdatePlayerVisibility();
                }
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

            MeshRenderer rend = obj.GetComponent<MeshRenderer>();
            rend.material.SetColor("_RimColor", new Color(1f, 1f, 1f));
            rend.material.SetFloat("_RimIntensity", 1f);
            rend.material.SetFloat("_RimSize", 1f);
            rend.material.SetFloat("_RimSmoothness", 1f);
            highlightedObject = obj;
        }

        private void Unhighlight(GameObject obj)
        {
            MeshRenderer rend = obj.GetComponent<MeshRenderer>();
            rend.material.SetColor("_RimColor", new Color(0f, 0f, 0f));
            rend.material.SetFloat("_RimIntensity", 0f);
            rend.material.SetFloat("_RimSize", 0f);
            rend.material.SetFloat("_RimSmoothness", 0f);
        }
    }
}