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
                    ShowItemStats(item);
                    highlightedItem = invItem;
                };
                itemHandler.MouseExit += delegate
                {
                    descriptionText.text = "";
                    highlightedItem = null;
                };
                itemHandler.MouseClick += delegate (PointerEventData eventData)
                {
                    EquipSlot slot = eventData.button == PointerEventData.InputButton.Left ? EquipSlot.LeftHand : EquipSlot.RightHand;

                    if (player.HasItemInSlot(invItem, slot))
                    {
                        Unequip(slot);
                    }
                    else
                    {
                        Equip(invItem, slot);
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

        private void Equip(InventoryItem item, EquipSlot slot)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            EquipSlot otherHand = slot == EquipSlot.LeftHand ? EquipSlot.RightHand : EquipSlot.LeftHand;

            // Unequip old item
            if (player.Equipment.ContainsKey(slot))
            {
                Unequip(slot);
            }

            // One item cannot be held in both hands
            if (item.Count == 1 && player.HasItemInSlot(item, otherHand))
            {
                Unequip(otherHand);
            }

            player.Equipment[slot] = item;

            var handObj = GetEquipTransformForSlot(slot);
            gameManager.SpawnItemToHand(handObj.transform, item.ItemKey);
        }

        private void Unequip(EquipSlot slot)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            player.Equipment.Remove(slot);
            var handObj = GetEquipTransformForSlot(slot);

            for (int i = handObj.childCount - 1; i >= 0; i--)
            {
                GameObject.Destroy(handObj.transform.GetChild(i).gameObject);
            }
        }

        private void DropItem(InventoryItem item)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            if (player.HasItemInSlot(item, EquipSlot.LeftHand))
            {
                Unequip(EquipSlot.LeftHand);
            }
            else if (player.HasItemInSlot(item, EquipSlot.RightHand))
            {
                Unequip(EquipSlot.RightHand);
            }
            item.Count--;

            if (item.Count == 0)
            {
                player.Inventory.Remove(item);
            }

            Vector3 spawnPos = Utils.ConvertToWorldCoord(player.Position) + new Vector3(0f, 0.5f, 0f)
                             + player.gameObject.transform.forward * 0.3f;
            gameManager.SpawnItem(item.ItemKey, spawnPos, Random.rotation);
        }

        private Transform GetEquipTransformForSlot(EquipSlot slot)
        {
            // TODO: this should be done without transform.Find
            var handName = slot == EquipSlot.LeftHand ? "LeftHand" : "RightHand";
            return gameManager.Camera.gameObject.transform.Find(handName);
        }
    }
}