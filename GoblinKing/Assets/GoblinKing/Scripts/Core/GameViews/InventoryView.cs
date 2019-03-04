using UnityEngine;
using System.Collections.Generic;
using GoblinKing.UI;

namespace GoblinKing.Core.GameViews
{
    internal class InventoryView : IGameView
    {
        private GameManager gameManager;
        private GameObject inventoryCanvas;
        private List<GameObject> guiItems = new List<GameObject>();
        private TMPro.TextMeshProUGUI descriptionText;

        public void Initialize(GameManager gameManager)
        {
            this.gameManager = gameManager;
        }

        public void Destroy()
        {
        }

        public void OpenView()
        {
            inventoryCanvas = GameObject.Instantiate(gameManager.inventoryPrefab);
            var inventory = gameManager.playerObject.GetComponent<Creature>().Inventory;

            // Find description object's text component
            Transform[] children = inventoryCanvas.transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child.name == "Description")
                {
                    descriptionText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    break;
                }
            }

            if (inventory.Count == 0)
            {
                descriptionText.text = "Your inventory is empty.";
            }

            // Instantiate items in inventory
            for (int i = 0; i < inventory.Count; i++)
            {
                var obj = GameObject.Instantiate(gameManager.inventoryGuiItemPrefab);
                obj.transform.SetParent(inventoryCanvas.transform);
                obj.GetComponent<RectTransform>().localPosition = new Vector3(150, 135 - i * 30f, 0);

                InventoryItem invItem = inventory[i];
                Data.ItemData item = gameManager.GameData.ItemData[invItem.ItemKey];
                obj.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + invItem.Count + "x " + item.Name;

                var itemEvent = obj.GetComponent<InventoryItemEventHandler>();

                itemEvent.MouseEnter += delegate { ShowItemStats(item); };
                itemEvent.MouseExit += delegate { descriptionText.text = ""; };
            }
        }

        public void CloseView()
        {
            GameObject.Destroy(inventoryCanvas);
        }


        public bool UpdateView()
        {
            return Utils.IsPressed(gameManager.keybindings.OpenInventory);
        }

        private void ShowItemStats(Data.ItemData item)
        {
            descriptionText.text = string.Format("Melee damage: {0}\nThrowing damage: {1}\nDefence: {2}\n\nWeight: {3}\n\n{4}", item.MeleeDamage, item.ThrowingDamage, item.Defence, item.Weight, item.Description);
        }
    }
}