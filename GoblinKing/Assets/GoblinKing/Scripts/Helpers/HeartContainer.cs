using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoblinKing.Helpers
{
    public class HeartContainer : MonoBehaviour
    {
        [SerializeField]
        private MeshRenderer[] heartMeshes;

        public int CurrentLife = 0;
        public int CurrentMaxLife = 0;

        public void SetMaxLife(int maxLife)
        {
            CurrentMaxLife = maxLife;

            for (int i = 0; i < heartMeshes.Length; i++)
            {
                heartMeshes[i].enabled = i < CurrentMaxLife;
            }
        }

        public void SetLife(int life)
        {
            CurrentLife = life;
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
            SetMaxLife(0);
            heartMeshes = GetComponentsInChildren<MeshRenderer>();
        }

        private void Update()
        {

        }
    }
}