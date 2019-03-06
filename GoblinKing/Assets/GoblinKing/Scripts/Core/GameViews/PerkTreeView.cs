using UnityEngine;
using GoblinKing.UI;

namespace GoblinKing.Core.GameViews
{
    internal class PerkTreeView : IGameView
    {
        private GameManager gameManager;
        private PerkSystem perkSystem;

        private GameObject perkTreeCanvas;
        private Color perkBoughtColor = Color.green;
        private PerkButton[] buttons;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
            perkSystem = gameManager.playerObject.GetComponent<Creature>().PerkSystem;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
            perkTreeCanvas = GameObject.Instantiate(gameManager.perkTreePrefab);
            buttons = perkTreeCanvas.GetComponentsInChildren<PerkButton>();

            foreach (var button in buttons)
            {
                var backgroundImg = button.GetComponent<UnityEngine.UI.Image>();

                if (perkSystem.HasPerk(button.PerkKey))
                {
                    backgroundImg.color = perkBoughtColor;
                }

                button.MouseEnter += delegate
                {
                    if (!perkSystem.HasPerk(button.PerkKey))
                    {
                        backgroundImg.color = new Color(0.7f, 0.7f, 0.7f);
                    }
                };
                button.MouseExit += delegate
                {
                    if (!perkSystem.HasPerk(button.PerkKey))
                    {
                        backgroundImg.color = new Color(1f, 1f, 1f);
                    }
                };
                button.MouseClick += delegate
                {
                    if (!perkSystem.HasPerk(button.PerkKey))
                    {
                        // TODO: check that player has enough perk points and can buy perk
                        perkSystem.BuyPerk(button.PerkKey);
                        backgroundImg.color = perkBoughtColor;
                    }
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