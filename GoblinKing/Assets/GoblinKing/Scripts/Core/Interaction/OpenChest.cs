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

            Destroy(Lid.GetComponent<Collider>());
            Destroy(GetComponent<Interactable>());
            Destroy(GetComponent<Collider>());
            Destroy(this);

            return true;
        }
    }
}