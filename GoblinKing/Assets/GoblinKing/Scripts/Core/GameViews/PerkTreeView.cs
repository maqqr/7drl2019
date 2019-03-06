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
        private TMPro.TextMeshProUGUI descriptionText;

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

             // Find description object's text component
            Transform[] children = perkTreeCanvas.transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child.name == "Description")
                {
                    descriptionText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    descriptionText.transform.parent.gameObject.SetActive(false);
                }
            }

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
                    descriptionText.transform.parent.gameObject.SetActive(true);
                    Data.Perk perk = gameManager.GameData.PerkData[button.PerkKey];
                    descriptionText.text = perk.Name + "\n\n" + perk.Description;
                };
                button.MouseExit += delegate
                {
                    if (!perkSystem.HasPerk(button.PerkKey))
                    {
                        backgroundImg.color = new Color(1f, 1f, 1f);
                    }
                    descriptionText.transform.parent.gameObject.SetActive(false);
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
    }
}