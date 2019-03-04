using UnityEngine;
using System.Collections.Generic;

namespace GoblinKing.Core.GameStates
{
    internal class InventoryView : IGameView
    {
        private GameManager gameManager;

        private GameObject inventoryCanvas;

        private List<GameObject> guiItems = new List<GameObject>();

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

            for (int i = 0; i < inventory.Count; i++)
            {
                var obj = GameObject.Instantiate(gameManager.inventoryGuiItemPrefab);
                obj.transform.SetParent(inventoryCanvas.transform);
                obj.GetComponent<RectTransform>().localPosition = new Vector3(150, 135- i * 30f, 0);

                InventoryItem invItem = inventory[i];
                Data.ItemData item = gameManager.GameData.ItemData[invItem.ItemKey];
                obj.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + invItem.Count + "x " + item.Name;
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
    }
}