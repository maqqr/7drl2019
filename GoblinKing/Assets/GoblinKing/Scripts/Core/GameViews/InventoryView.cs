using UnityEngine;
using System.Collections.Generic;
using GoblinKing.UI;
using UnityEngine.EventSystems;

namespace GoblinKing.Core.GameViews
{
    internal class InventoryView : IGameView
    {
        private GameManager gameManager;
        private GameObject inventoryCanvas;
        private List<GameObject> guiItems = new List<GameObject>();
        private TMPro.TextMeshProUGUI descriptionText;
        private TMPro.TextMeshProUGUI encumbranceText;

        private InventoryItem highlightedItem = null;

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

            // Find description object's text component
            Transform[] children = inventoryCanvas.transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                if (child.name == "Description")
                {
                    descriptionText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                }
                if (child.name == "EncumbranceText")
                {
                    encumbranceText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                }
            }

            RefreshView();
        }

        public void CloseView()
        {
            GameObject.Destroy(inventoryCanvas);
        }

        public bool UpdateView()
        {
            if (highlightedItem != null && Utils.IsPressed(gameManager.keybindings.DropItem))
            {
                DropItem(highlightedItem);
                RefreshView();
            }
            if (highlightedItem != null && Utils.IsPressed(gameManager.keybindings.ConsumeItem))
            {
                ConsumeItem(highlightedItem);
                RefreshView();
            }

            return Utils.IsPressed(gameManager.keybindings.OpenInventory);
        }

        private void RefreshView()
        {
            for (int i = 0; i < guiItems.Count; i++)
            {
                GameObject.Destroy(guiItems[i]);
            }
            guiItems.Clear();

            UpdateEncumbranceText();

            var player = gameManager.playerObject.GetComponent<Creature>();

            if (player.Inventory.Count == 0)
            {
                descriptionText.text = "Your inventory is empty.";
            }

            // Instantiate items in inventory
            for (int i = 0; i < player.Inventory.Count; i++)
            {
                var obj = GameObject.Instantiate(gameManager.inventoryGuiItemPrefab);
                var backgroundImg = obj.GetComponent<UnityEngine.UI.Image>();
                obj.transform.SetParent(inventoryCanvas.transform);
                obj.GetComponent<RectTransform>().localPosition = new Vector3(150, 135 - i * 30f, 0);
                guiItems.Add(obj);

                InventoryItem invItem = player.Inventory[i];
                Data.ItemData item = gameManager.GameData.ItemData[invItem.ItemKey];
                obj.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + invItem.Count + "x " + item.Name;

                var itemHandler = obj.GetComponent<UIItemHandler>();

                if (player.HasItemInSlot(invItem, EquipSlot.LeftHand))
                {
                    itemHandler.LeftHandImage.SetActive(true);
                }
                if (player.HasItemInSlot(invItem, EquipSlot.RightHand))
                {
                    itemHandler.RightHandImage.SetActive(true);
                }

                itemHandler.MouseEnter += delegate
                {
                    backgroundImg.color = new Color(0.7f, 0.7f, 0.7f);
                    ShowItemStats(item);
                    highlightedItem = invItem;
                };
                itemHandler.MouseExit += delegate
                {
                    backgroundImg.color = new Color(1f, 1f, 1f);
                    descriptionText.text = "";
                    highlightedItem = null;
                };
                itemHandler.MouseClick += delegate (PointerEventData eventData)
                {
                    EquipSlot slot = eventData.button == PointerEventData.InputButton.Left ? EquipSlot.LeftHand : EquipSlot.RightHand;

                    if (player.HasItemInSlot(invItem, slot))
                    {
                        gameManager.PlayerUnequip(slot);
                    }
                    else
                    {
                        gameManager.PlayerEquip(invItem, slot);
                    }
                    RefreshView();
                };
            }
        }

        private void ShowItemStats(Data.ItemData item)
        {
            descriptionText.text = string.Format("Melee damage: {0}\nThrowing damage: {1}\nDefence: {2}\n\nWeight: {3}\n\n{4}", item.MeleeDamage, item.ThrowingDamage, item.Defence, item.Weight, item.Description);
        }

        private void UpdateEncumbranceText()
        {
            int total = Utils.TotalEncumbrance(gameManager, gameManager.playerObject.GetComponent<Creature>());
            encumbranceText.text = string.Format("Encumbrance: {0} / {1}", total, "?");
        }

        private void DropItem(InventoryItem item)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            if (player.HasItemInSlot(item, EquipSlot.LeftHand))
            {
                gameManager.PlayerUnequip(EquipSlot.LeftHand);
            }
            else if (player.HasItemInSlot(item, EquipSlot.RightHand))
            {
                gameManager.PlayerUnequip(EquipSlot.RightHand);
            }

            player.RemoveItem(item, 1);

            Vector3 spawnPos = Utils.ConvertToWorldCoord(player.Position) + new Vector3(0f, 0.5f, 0f)
                             + player.gameObject.transform.forward * 0.3f;
            gameManager.SpawnItem(item.ItemKey, spawnPos, Random.rotation);
            gameManager.AdvanceTime(gameManager.playerObject.GetComponent<Creature>().Speed);
            gameManager.UpdateGameWorld();
        }
        private void ConsumeItem(InventoryItem item)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            var playerp = gameManager.playerObject.GetComponent<Player>();
            var nut = gameManager.GameData.ItemData[item.ItemKey].Nutrition;
            if(nut != 0)
            {
                if (player.HasItemInSlot(item, EquipSlot.LeftHand))
                {
                    gameManager.PlayerUnequip(EquipSlot.LeftHand);
                }
                else if (player.HasItemInSlot(item, EquipSlot.RightHand))
                {
                    gameManager.PlayerUnequip(EquipSlot.RightHand);
                }
                playerp.Nutrition += nut;
                player.RemoveItem(item, 1);
                gameManager.AdvanceTime(gameManager.playerObject.GetComponent<Creature>().Speed);
                gameManager.UpdateGameWorld();
            }
        }
    }
}