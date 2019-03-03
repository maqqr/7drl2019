using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.GameStates
{
    internal class InGame : IGameView
    {
        private GameManager gameManager;

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
                HandlePlayerInput();
            }

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
                // TODO: check collisions
                gameManager.playerObject.GetComponent<Creature>().position = playerMoveTo.Value;
            }
        }

        private void UpdatePlayerVisibility()
        {
            VisibilityLevel level = Visibility.Calculate(gameManager.playerObject.transform.position, gameManager.CurrentFloorObject.GetComponent<DungeonLevel>().lightSources);

            gameManager.visibilityDiamondObject.GetComponent<MeshRenderer>().material.SetColor("_Color", Visibility.GetGemColor(level));
        }
    }
}