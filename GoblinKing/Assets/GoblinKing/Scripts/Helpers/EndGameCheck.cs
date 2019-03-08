using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoblinKing.Core;

namespace GoblinKing.Helpers
{
    // This whole thing is a dirty hack because time is running out
    internal class EndGameCheck : MonoBehaviour
    {
        GameManager gameManager = null;

        private Vector2Int prevPosition = Vector2Int.zero;

        void Start()
        {
            gameManager = GameObject.FindObjectOfType<GameManager>();
        }

        void Update()
        {
            bool playerMoved = prevPosition != gameManager.playerCreature.Position;
            if (playerMoved && gameManager.playerCreature.Position == Utils.ConvertToGameCoord(transform.position))
            {
                gameManager.WinGame();
            }

            prevPosition = gameManager.playerCreature.Position;
        }
    }
}