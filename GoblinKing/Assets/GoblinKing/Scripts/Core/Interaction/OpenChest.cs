using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Core.Interaction
{
    public class OpenChest : MonoBehaviour, IInteraction
    {
        public GameObject Lid;
        public Transform LidOpenTransform;

        public bool Interact(GameManager gameManager)
        {
            var lockpick = gameManager.playerCreature.GetItemFromInventory("lockpick");
            if (lockpick != null)
            {
                if (gameManager.playerCreature.HasItemInSlot(lockpick, EquipSlot.LeftHand)) gameManager.PlayerUnequip(EquipSlot.LeftHand);
                if (gameManager.playerCreature.HasItemInSlot(lockpick, EquipSlot.RightHand)) gameManager.PlayerUnequip(EquipSlot.RightHand);
                gameManager.playerCreature.RemoveItem(lockpick, 1);

                gameManager.MessageBuffer.AddMessage(Color.red, "You managed to pick open the chest, but the lock pick is now spent.");

                Lid.transform.position = LidOpenTransform.position;
                Lid.transform.rotation = LidOpenTransform.rotation;

                int spawnCount = Random.Range(1, 4);

                for (int i = 0; i < spawnCount; i++)
                {
                    var spawnableItems = gameManager.GameData.ItemSpawnList[gameManager.currentFloor];
                    Vector3 spawnPos = transform.position + new Vector3(Random.Range(-0.05f, 0.05f), 0.2f, Random.Range(-0.05f, 0.05f));

                    int randomItemIndex = Random.Range(0, spawnableItems.Length);
                    gameManager.SpawnItem(spawnableItems[randomItemIndex], spawnPos, Random.rotation);
                }

                Destroy(GetComponent<Interactable>());
                Destroy(GetComponent<Collider>());
                Destroy(this);
                return true;
            }
            else
            {
                gameManager.MessageBuffer.AddMessage(Color.red, "You need a lock pick to open the chest.");
                return false;
            }
        }
    }
}