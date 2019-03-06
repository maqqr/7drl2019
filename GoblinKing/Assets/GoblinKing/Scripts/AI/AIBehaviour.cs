using System.Collections.Generic;
using UnityEngine;
using GoblinKing.Core;

namespace GoblinKing.AI
{
    internal static class AIBehaviour
    {
        internal delegate Vector2Int UpdateFunction(GameManager gameManager, Creature creature);

        private static readonly Dictionary<AIType, UpdateFunction> behaviours = new Dictionary<AIType, UpdateFunction>()
        {
            { AIType.Still, Still },
            { AIType.MoveRandomly, MoveRandomly },
            { AIType.Patrol, Patrol }
        };

        internal static void UpdateAI(GameManager gameManager, Creature creature)
        {
            var playerCre = gameManager.playerObject.GetComponent<Creature>();
            Vector2Int newPosition = creature.Position;

            UpdateCreatureAlertLevels(gameManager, creature);

            // Act based on alert level
            if (creature.AlertLevel == AlertLevel.Alerted)
            {
                newPosition = ChasePlayer(gameManager, creature);
            }
            else if (creature.AlertLevel == AlertLevel.Suspicious)
            {
                newPosition = PathfindTo(gameManager, creature.Position, creature.SuspiciousPosition);

                if (newPosition == creature.Position)
                {
                    // TODO: search areas nearby suspicious position instead of changing directly to unaware?
                    ChangeAlertness(gameManager, creature, AlertLevel.Unaware);
                }
            }
            else if (creature.AlertLevel == AlertLevel.Unaware)
            {
                newPosition = behaviours[creature.AIType](gameManager, creature);
            }

            LayerMask mask = ~LayerMask.GetMask("Player", "Enemy");
            if (gameManager.IsWalkableFrom(creature.Position, newPosition, mask))
            {
                creature.TurnTowards(newPosition);

                Creature creatureBlocking = gameManager.GetCreatureAt(newPosition);
                if (creatureBlocking == null)
                {
                    creature.Position = newPosition;
                }
                else if (creatureBlocking == playerCre)
                {
                    gameManager.Fight(creature, creatureBlocking);
                }
                else
                {
                    // TODO: move randomly to avoid the other enemy that blocks the way?
                }
            }
        }

        private static void ChangeAlertness(GameManager gameManager, Creature creature, AlertLevel newAlertLevel)
        {
            // TODO: spawn question mark or exc. mark if needed
            creature.AlertLevel = newAlertLevel;
            Debug.Log("New alertness: " + newAlertLevel);

            if (newAlertLevel != AlertLevel.Unaware)
            {
                GameObject prefab = newAlertLevel == AlertLevel.Suspicious ? gameManager.questionMarkPrefab : gameManager.exclamationMarkPrefab;
                var obj = GameObject.Instantiate(prefab, creature.gameObject.transform.position + new Vector3(0f, 0.7f, 0f), Quaternion.Euler(-90f, 0f, 0f));
                obj.transform.parent = creature.transform;
            }
        }

        private static void UpdateCreatureAlertLevels(GameManager gameManager, Creature creature)
        {
            var playerCre = gameManager.playerObject.GetComponent<Creature>();
            var player = gameManager.playerObject.GetComponent<Player>();

            // TODO: check line of sight

            if (creature.AlertLevel == AlertLevel.Alerted)
            {
                if (player.CurrentVisibility == VisibilityLevel.Hidden)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Suspicious);
                    creature.SuspiciousPosition = playerCre.Position;
                }
            }
            else if (creature.AlertLevel == AlertLevel.Suspicious)
            {
                if (player.CurrentVisibility == VisibilityLevel.Partial || player.CurrentVisibility == VisibilityLevel.Visible)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Alerted);
                }
            }
            else
            {
                if (player.CurrentVisibility == VisibilityLevel.Partial)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Suspicious);
                    creature.SuspiciousPosition = playerCre.Position;
                }
                else if (player.CurrentVisibility == VisibilityLevel.Visible)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Alerted);
                }
            }
        }

        private static Vector2Int PathfindTo(GameManager gameManager, Vector2Int from, Vector2Int to)
        {
            var path = gameManager.FindPath(from, to);
            Vector2Int newPos = from;

            if (path.Count > 0)
            {
                newPos = path[0];
            }
            else
            {
                Debug.LogError("No path!");
            }
            return newPos;
        }

        private static Vector2Int ChasePlayer(GameManager gameManager, Creature creature)
        {
            var player = gameManager.playerObject.GetComponent<Creature>();
            return PathfindTo(gameManager, creature.Position, player.Position);
        }

        private static Vector2Int Still(GameManager gameManager, Creature creature)
        {
            return creature.Position;
        }

        private static Vector2Int MoveRandomly(GameManager gameManager, Creature creature)
        {
            int randomX = Random.Range(-1, 2);
            int randomY = Random.Range(-1, 2);
            Vector2Int newPos = new Vector2Int(creature.Position.x + randomX, creature.Position.y + randomY);
            return newPos;
        }

        private static Vector2Int Patrol(GameManager gameManager, Creature creature)
        {
            // TODO: implement
            return creature.Position;
        }
    }
}