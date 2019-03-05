using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class HeartContainer : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer[] heartMeshes;

        private int currentMaxLife = 0;

        public void SetMaxLife(int maxLife)
        {
            currentMaxLife = maxLife;

            for (int i = 0; i < heartMeshes.Length; i++)
            {
                heartMeshes[i].enabled = i < currentMaxLife;
            }
        }

        public void SetLife(int life)
        {
            for (int i = 0; i < heartMeshes.Length; i++)
            {
                if (heartMeshes[i].enabled)
                {
                    heartMeshes[i].material.SetColor("_Color", i < life ? Color.white : Color.black);
                }
            }
        }

        private void Awake()
        {
            heartMeshes = GetComponentsInChildren<MeshRenderer>();
        }

        private void Start()
        {
            SetMaxLife(0);
        }

        private void Update()
        {

        }
    }
}