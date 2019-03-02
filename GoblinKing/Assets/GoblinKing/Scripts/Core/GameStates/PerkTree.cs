using UnityEngine;

namespace GoblinKing.Core.GameStates
{
    internal class PerkTree : IGameView
    {
        private GameManager gameManager;

        private GameObject perkTreeCanvas;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;

            perkTreeCanvas = GameObject.Instantiate(gameManager.perkTreePrefab);
        }

        public void CloseView()
        {
            GameObject.Destroy(perkTreeCanvas);
        }

        public bool UpdateView()
        {
            return Utils.IsPressed(gameManager.keybindings.openPerkTree);
        }
    }
}