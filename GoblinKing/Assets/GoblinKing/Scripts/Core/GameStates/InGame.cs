using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.GameStates
{
    internal class InGame : IGameView
    {
        private GameManager gameManager;

        private int advanceTime = 0; // Gow much time should be advanced due to player actions

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void CloseView()
        {
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

            // TODO: visibility does not need to be updated every frame?
            UpdatePlayerVisibility();

            // These are for debugging purposes
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
                Vector3 spawnpos = gameManager.playerObject.transform.position + new Vector3(0f, 0.5f, 0f) + gameManager.playerObject.transform.forward;
                gameManager.SpawnItem("skull", spawnpos, Random.rotation);
            }

            if (Input.GetKeyDown(KeyCode.Home))
            {
                Vector2Int spawnpos = gameManager.playerObject.GetComponent<Creature>().Position;
                gameManager.SpawnCreature("goblin", spawnpos);
            }

            return false;
        }

        private void HandlePlayerInput()
        {
            Vector2Int? playerMoveTo = null;
            GameObject playerObj = gameManager.playerObject;

            if (Utils.IsDown(gameManager.keybindings.moveForward))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition + playerObj.transform.forward);
            }
            else if (Utils.IsDown(gameManager.keybindings.moveBackward))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition - playerObj.transform.forward);
            }
            else if (Utils.IsDown(gameManager.keybindings.moveRight))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition + playerObj.transform.right);
            }
            else if (Utils.IsDown(gameManager.keybindings.moveLeft))
            {
                playerMoveTo = Utils.ConvertToGameCoord(playerObj.transform.localPosition - playerObj.transform.right);
            }

            if (Utils.IsPressed(gameManager.keybindings.openPerkTree))
            {
                gameManager.AddView(new PerkTree());
            }

            if (playerMoveTo != null)
            {
                if (gameManager.IsWalkableFrom(playerObj.GetComponent<Creature>().Position, playerMoveTo.Value))
                {
                    gameManager.playerObject.GetComponent<Creature>().Position = playerMoveTo.Value;
                    advanceTime = gameManager.playerObject.GetComponent<Creature>().Speed;
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
    }
}