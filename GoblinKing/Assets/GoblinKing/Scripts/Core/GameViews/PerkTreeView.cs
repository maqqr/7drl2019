using UnityEngine;

namespace GoblinKing.Core.GameViews
{
    internal class PerkTreeView : IGameView
    {
        private GameManager gameManager;

        private GameObject perkTreeCanvas;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
            perkTreeCanvas = GameObject.Instantiate(gameManager.perkTreePrefab);
        }

        public void CloseView()
        {
            GameObject.Destroy(perkTreeCanvas);
        }


        public bool UpdateView()
        {
            return Utils.IsPressed(gameManager.keybindings.OpenPerkTree);
        }
    }
}