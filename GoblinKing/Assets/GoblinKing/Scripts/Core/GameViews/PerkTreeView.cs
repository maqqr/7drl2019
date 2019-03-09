using UnityEngine;
using GoblinKing.UI;

namespace GoblinKing.Core.GameViews
{
    internal class PerkTreeView : IGameView
    {
        private GameManager gameManager;
        private PerkSystem perkSystem;

        private GameObject perkTreeCanvas;
        private Color availableColor = Color.white;
        private Color cannotBuyYetColor = new Color(0.3f, 0.3f, 0.3f);
        private Color perkBoughtColor = Color.green;
        private PerkButton[] buttons;
        private TMPro.TextMeshProUGUI descriptionText;
        private TMPro.TextMeshProUGUI perkPointText;


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
                if (child.name == "PerkPointText")
                {
                    perkPointText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                }
            }

            UpdateTexts();

            buttons = perkTreeCanvas.GetComponentsInChildren<PerkButton>();
            UpdateButtonColors();

            foreach (var button in buttons)
            {
                var backgroundImg = button.GetComponent<UnityEngine.UI.Image>();

                if (perkSystem.HasPerk(button.PerkKey))
                {
                    backgroundImg.color = perkBoughtColor;
                }

                button.MouseEnter += delegate
                {
                    if (!perkSystem.HasPerk(button.PerkKey) && perkSystem.CanBuyPerk(button.PerkKey) && gameManager.playerObject.GetComponent<Player>().Perkpoints > 0)
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
                        UpdateButtonColor(button);
                    }
                    descriptionText.transform.parent.gameObject.SetActive(false);
                };
                button.MouseClick += delegate
                {
                    if (!perkSystem.HasPerk(button.PerkKey) && perkSystem.CanBuyPerk(button.PerkKey) && gameManager.playerObject.GetComponent<Player>().Perkpoints > 0)
                    {
                        perkSystem.BuyPerk(button.PerkKey);
                        gameManager.playerObject.GetComponent<Player>().Perkpoints -= 1;
                        gameManager.MessageBuffer.AddMessage(Color.yellow, "You gained the perk: " + gameManager.GameData.PerkData[button.PerkKey].Name + ".");
                        UpdateTexts();
                        UpdateButtonColors();
                        gameManager.UpdateHearts(gameManager.playerCreature, gameManager.PlayerHearts);
                    }
                    else
                    {
                        string reason = null;
                        if (perkSystem.HasPerk(button.PerkKey)) reason = "You already have that perk.";
                        else if (!perkSystem.CanBuyPerk(button.PerkKey)) reason = "You cannot buy that perk yet.";
                        else if (gameManager.playerObject.GetComponent<Player>().Perkpoints == 0) reason = "You do not have enough perk point to buy that.";
                        if (!string.IsNullOrEmpty(reason))
                        {
                            gameManager.MessageBuffer.AddMessage(Color.yellow, reason);
                        }
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

        private void UpdateTexts()
        {
            perkPointText.text = string.Format("Perk points: {0}", gameManager.playerObject.GetComponent<Player>().Perkpoints);
        }

        private void UpdateButtonColors()
        {
            foreach (var button in buttons)
            {
                UpdateButtonColor(button);
            }
        }

        private void UpdateButtonColor(PerkButton button)
        {
            var backgroundImg = button.GetComponent<UnityEngine.UI.Image>();

            if (perkSystem.HasPerk(button.PerkKey))
            {
                backgroundImg.color = perkBoughtColor;
            }
            else if (perkSystem.CanBuyPerk(button.PerkKey))
            {
                backgroundImg.color = availableColor;
            }
            else
            {
                backgroundImg.color = cannotBuyYetColor;
            }
        }
    }
}