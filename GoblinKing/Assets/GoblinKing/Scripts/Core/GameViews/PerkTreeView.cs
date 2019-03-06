using UnityEngine;
using GoblinKing.UI;

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
            var buttons = perkTreeCanvas.GetComponentsInChildren<PerkButton>();

            foreach (var button in buttons)
            {
                var backgroundImg = button.GetComponent<UnityEngine.UI.Image>();
                button.MouseEnter += delegate
                {
                    backgroundImg.color = new Color(0.7f, 0.7f, 0.7f);
                    // ShowItemStats(item);
                    // highlightedItem = invItem;
                };
                button.MouseExit += delegate
                {
                    backgroundImg.color = new Color(1f, 1f, 1f);
                    // descriptionText.text = "";
                    // highlightedItem = null;
                };
                button.MouseClick += delegate
                {
                    Debug.Log(button.PerkKey);
                };
            }
        }

        public void CloseView()
        {
            GameObject.Destroy(perkTreeCanvas);
        }


        public bool UpdateView()
        {
            return Utils.IsPressed(gameManager.keybindings.OpenPerkTree);
        }

        private void RefreshView()
        {
        }
    }
}