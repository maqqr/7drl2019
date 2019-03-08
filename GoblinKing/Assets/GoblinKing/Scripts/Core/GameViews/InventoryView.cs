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
        private TMPro.TextMeshProUGUI levelText;
        private TMPro.TextMeshProUGUI xpText;

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

            // Find text components
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
                if (child.name == "LevelText")
                {
                    levelText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                }
                if (child.name == "XPText")
                {
                    xpText = child.GetComponentInChildren<TMPro.TextMeshProUGUI>();
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
                highlightedItem = null;
                RefreshView();
            }
            else if (highlightedItem != null && Utils.IsPressed(gameManager.keybindings.ConsumeItem))
            {
                ConsumeItem(highlightedItem);
                highlightedItem = null;
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

            UpdateStatusTexts();

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
                obj.GetComponent<RectTransform>().localPosition = new Vector3(170, 288 - i * 30f, 0);
                guiItems.Add(obj);

                InventoryItem invItem = player.Inventory[i];
                Data.ItemData item = gameManager.GameData.ItemData[invItem.ItemKey];
                obj.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "" + invItem.Count + "x " + System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(item.Name);

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
                        gameManager.MessageBuffer.AddMessage(Color.white, "You unequipped the "+gameManager.GameData.ItemData[invItem.ItemKey].Name.ToLower()+" from your "+ slot.ToString().Replace("Hand","").ToLower() + " hand.");
                    }
                    else
                    {
                        gameManager.PlayerEquip(invItem, slot);
                        gameManager.MessageBuffer.AddMessage(Color.white, "You equipped the "+gameManager.GameData.ItemData[invItem.ItemKey].Name.ToLower()+" to your "+ slot.ToString().Replace("Hand","").ToLower() + " hand.");
                    }
                    RefreshView();
                };
            }
        }

        private void ShowItemStats(Data.ItemData item)
        {
            descriptionText.text = string.Format("Melee damage: {0}\nThrowing damage: {1}\nDefence: {2}\n\nWeight: {3}\n\n{4}", item.MeleeDamage, item.ThrowingDamage, item.Defence, item.Weight, item.Description);
        }

        private void UpdateStatusTexts()
        {
            var playerCre = gameManager.playerObject.GetComponent<Creature>();
            var player = gameManager.playerObject.GetComponent<Player>();

            int total = Utils.TotalEncumbrance(gameManager, playerCre);
            encumbranceText.text = string.Format("Encumbrance: {0} / {1}", total, playerCre.MaxEnc);

            levelText.text = string.Format("Level: {0}", player.Level);
            xpText.text = string.Format("Experience: {0} / 100", player.Experience);
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

            var spawned = gameManager.SpawnItem(item.ItemKey, spawnPos, Random.rotation);
            spawned.GetComponent<Rigidbody>().isKinematic = false;
            gameManager.MessageBuffer.AddMessage(Color.white, "You dropped the "+gameManager.GameData.ItemData[item.ItemKey].Name+".");
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
                gameManager.MessageBuffer.AddMessage(Color.white, "You consumed the "+gameManager.GameData.ItemData[item.ItemKey].Name+".");
                gameManager.AdjustNutrition((int)System.Math.Ceiling(nut*player.PerkSystem.GetMaxFloat("metabolicMultiplier", 1f)));
                if(gameManager.GameData.ItemData[item.ItemKey].Healing > 0)
                {
                    player.RecoverHealth((int)System.Math.Ceiling(gameManager.GameData.ItemData[item.ItemKey].Healing * player.PerkSystem.GetMaxFloat("potionEffect", 1f)));
                    gameManager.MessageBuffer.AddMessage(Color.red, "You feel like you regained some health.");
                }
                if(gameManager.GameData.ItemData[item.ItemKey].Healing < 0)
                {
                    int grd = player.PerkSystem.GetMaxInt("stomachGuard",0);
                    player.TakeDamage(-1*gameManager.GameData.ItemData[item.ItemKey].Healing - grd);
                    gameManager.MessageBuffer.AddMessage((grd == 0 ? Color.green : (grd == 1 ? Color.green+Color.red : Color.red)), "The rotten food had " + (grd == 0 ? "an ill" : (grd == 1 ? "a small" : "no")) + " effect on your health.");
                }
                if(gameManager.GameData.ItemData[item.ItemKey].Experience > 0)
                {
                    gameManager.addExperience((int)System.Math.Ceiling(gameManager.GameData.ItemData[item.ItemKey].Experience * player.PerkSystem.GetMaxFloat("potionEffect", 1f)));
                    gameManager.MessageBuffer.AddMessage(Color.Lerp(Color.red, Color.blue, 0.5f), "You feel more experienced.");
                }

                player.RemoveItem(item, 1);
                gameManager.AdvanceTime(gameManager.playerObject.GetComponent<Creature>().Speed);
                gameManager.UpdateGameWorld();
            }
        }
    }
}