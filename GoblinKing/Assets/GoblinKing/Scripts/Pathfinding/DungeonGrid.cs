/**
 * Original code by Ronen Ness.
 * https://github.com/RonenNess/UnityUtils/tree/master/Controls/PathFinding/2dTileBasedPathFinding
 */
using UnityEngine;
using System.Collections.Generic;

namespace GoblinKing.Pathfinding
{
    /// <summary>
    /// A 2D grid of nodes we use to find path.
    /// The grid mark which tiles are walkable and which are not.
    /// </summary>
    public class DungeonGrid
    {
        public Dictionary<Vector2Int, Node> nodes;

        private int minX = 0;
        private int maxX = 0;
        private int minY = 0;
        private int maxY = 0;

        public DungeonGrid()
        {
            nodes = new Dictionary<Vector2Int, Node>();
        }

        public Node At(int x, int y)
        {
            var pos = new Vector2Int(x, y);
            if (nodes.ContainsKey(pos))
            {
                return nodes[pos];
            }
            return null;
        }

        public void CreateGrid(int minX, int maxX, int minY, int maxY, System.Func<Vector2Int, bool> isWalkable)
        {
            this.minX = minX;
            this.maxX = maxX;
            this.minY = minY;
            this.maxY = maxY;
            nodes.Clear();

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    if (isWalkable(new Vector2Int(x, y)))
                    {
                        nodes.Add(new Vector2Int(x, y), new Node(1f, x, y));
                    }
                }
            }
        }

        public void UpdateGrid(System.Func<Vector2Int, bool> isWalkable)
        {
            foreach (var keyValue in nodes)
            {
                var pos = keyValue.Key;
                nodes[keyValue.Key].Update(isWalkable(pos), pos.x, pos.y);
            }
        }

        public System.Collections.IEnumerable GetNeighbours(Node node, Pathfinding.DistanceType distanceType, System.Func<Vector2Int, Vector2Int, bool> isWalkableFrom)
        {
            int x = 0, y = 0;
            switch (distanceType)
            {
                case Pathfinding.DistanceType.Manhattan:
                    y = 0;
                    for (x = -1; x <= 1; ++x)
                    {
                        var neighbor = AddNodeNeighbour(x, y, node, isWalkableFrom);
                        if (neighbor != null)
                            yield return neighbor;
                    }

                    x = 0;
                    for (y = -1; y <= 1; ++y)
                    {
                        var neighbor = AddNodeNeighbour(x, y, node, isWalkableFrom);
                        if (neighbor != null)
                            yield return neighbor;
                    }
                    break;

                case Pathfinding.DistanceType.Euclidean:
                    for (x = -1; x <= 1; x++)
                    {
                        for (y = -1; y <= 1; y++)
                        {
                            var neighbor = AddNodeNeighbour(x, y, node, isWalkableFrom);
                            if (neighbor != null)
                                yield return neighbor;
                        }
                    }
                    break;
            }
        }

        Node AddNodeNeighbour(int x, int y, Node node, System.Func<Vector2Int, Vector2Int, bool> isWalkableFrom)
        {
            if (x == 0 && y == 0)
            {
                return null;
            }

            int checkX = node.gridX + x;
            int checkY = node.gridY + y;
            var checkPos = new Vector2Int(checkX, checkY);

            // if (checkX >= minX && checkX <= maxX && checkY >= minY && checkY <= maxY)
            // {
            if (nodes.ContainsKey(checkPos) && isWalkableFrom(new Vector2Int(node.gridX, node.gridY), checkPos))
            {
                return nodes[checkPos];
            }
            // }

            return null;
        }
    }

}
