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

        public static void RaiseAlertness(GameManager gameManager, Creature creature, AlertLevel newAlertLevel)
        {
            if (creature.AlertLevel == AlertLevel.Unaware)
            {
                // All new alertness levels are allowed when unaware
                ChangeAlertness(gameManager, creature, newAlertLevel);
                return;
            }
            if (creature.AlertLevel == AlertLevel.Suspicious && newAlertLevel == AlertLevel.Alerted)
            {
                // Can only increase to Alerted when Suspicious
                ChangeAlertness(gameManager, creature, newAlertLevel);
                return;
            }
            // Alertness cannot increase above Alerted
        }

        public static void ChangeAlertness(GameManager gameManager, Creature creature, AlertLevel newAlertLevel)
        {
            bool levelChanged = newAlertLevel != creature.AlertLevel;
            creature.AlertLevel = newAlertLevel;
            Debug.Log("New alertness: " + newAlertLevel);

            if (levelChanged && newAlertLevel != AlertLevel.Unaware)
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

            VisibilityLevel playerModifiedVisibility = player.CurrentVisibility;

            // Check angle
            var dirToPlayer = Utils.ConvertToWorldCoord(playerCre.Position) - Utils.ConvertToWorldCoord(creature.Position);
            var forward = creature.transform.forward;
            var angle = Vector2.Angle(new Vector2(dirToPlayer.x, dirToPlayer.z), new Vector2(forward.x, forward.z));
            bool playerInFromOfCreature = angle <= 90;

            // Check if player is right in from of creature
            Vector2Int inFront = Utils.ConvertToGameCoord(Utils.ConvertToWorldCoord(creature.Position) + creature.transform.forward * 0.7f);
            Vector2Int inFrontRight = Utils.ConvertToGameCoord(Utils.ConvertToWorldCoord(creature.Position) + creature.transform.forward * 0.7f + creature.transform.right);
            Vector2Int inFrontLeft = Utils.ConvertToGameCoord(Utils.ConvertToWorldCoord(creature.Position) + creature.transform.forward * 0.7f - creature.transform.right);
            if (playerCre.Position == inFront || playerCre.Position == inFrontLeft || playerCre.Position == inFrontRight)
            {
                playerModifiedVisibility = VisibilityLevel.Visible;
            }

            // Check ray cast
            bool noLineOfSight = true;
            if (playerInFromOfCreature)
            {
                LayerMask mask = ~LayerMask.GetMask("Player", "Enemy");
                Vector3 raycastDir = Utils.ConvertToWorldCoord(gameManager.playerCreature.Position) - Utils.ConvertToWorldCoord(creature.Position);
                noLineOfSight = Physics.Raycast(Utils.ConvertToWorldCoord(creature.Position) + new Vector3(0f, 0.5f, 0f), raycastDir, raycastDir.magnitude, mask, QueryTriggerInteraction.Ignore);
            }

            if (noLineOfSight)
            {
                playerModifiedVisibility = VisibilityLevel.Hidden;
            }

            if (creature.AlertLevel == AlertLevel.Alerted)
            {
                if (playerModifiedVisibility == VisibilityLevel.Hidden)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Suspicious);
                    creature.SuspiciousPosition = playerCre.Position;
                }
            }
            else if (creature.AlertLevel == AlertLevel.Suspicious)
            {
                if (playerModifiedVisibility == VisibilityLevel.Partial || playerModifiedVisibility == VisibilityLevel.Visible)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Alerted);
                }
            }
            else
            {
                if (playerModifiedVisibility == VisibilityLevel.Partial)
                {
                    ChangeAlertness(gameManager, creature, AlertLevel.Suspicious);
                    creature.SuspiciousPosition = playerCre.Position;
                }
                else if (playerModifiedVisibility == VisibilityLevel.Visible)
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
                // Debug.LogError("No path!");
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
            Vector2Int newPos = creature.Position;
            if (Random.Range(0, 2) == 0)
            {
                int randomX = Random.Range(-1, 2);
                int randomY = Random.Range(-1, 2);
                newPos = new Vector2Int(creature.Position.x + randomX, creature.Position.y + randomY);
            }
            return newPos;
        }

        private static Vector2Int Patrol(GameManager gameManager, Creature creature)
        {
            if (creature.PatrolAttemptsLeft <= 0)
            {
                creature.PatrolAttemptsLeft = Random.Range(8, 14);
                creature.PatrolTarget = gameManager.RandomFreeSpace();
                Debug.Log("Got new patrol position (" + creature.PatrolTarget.x + ", " + creature.PatrolTarget.y + ")");
            }

            Vector2Int target = PathfindTo(gameManager, creature.Position, creature.PatrolTarget);

            var blockingCreature = gameManager.GetCreatureAt(target);
            if (target == creature.Position || blockingCreature != null)
            {
                creature.PatrolAttemptsLeft--;
            }

            return target;
        }
    }
}