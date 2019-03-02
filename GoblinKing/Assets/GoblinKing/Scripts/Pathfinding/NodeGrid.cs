﻿/**
 * Original code by Ronen Ness.
 * https://github.com/RonenNess/UnityUtils/tree/master/Controls/PathFinding/2dTileBasedPathFinding
 */
using System.Collections.Generic;

namespace GoblinKing.Pathfinding
{
    /// <summary>
    /// A 2D grid of nodes we use to find path.
    /// The grid mark which tiles are walkable and which are not.
    /// </summary>
    public class NodeGrid
    {
        // nodes in grid
        public Node[,] nodes;

        // grid size
        int gridSizeX, gridSizeY;

        /// <summary>
        /// Create a new grid with tile prices.
        /// </summary>
        /// <param name="tiles_costs">A 2d array of tile prices.
        ///     0.0f = Unwalkable tile.
        ///     1.0f = Normal tile.
        ///     > 1.0f = costy tile.
        ///     < 1.0f = cheap tile.
        /// </param>
        public NodeGrid(float[,] tiles_costs)
        {
            // create nodes
            CreateNodes(tiles_costs.GetLength(0), tiles_costs.GetLength(1));

            // init nodes
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    nodes[x, y] = new Node(tiles_costs[x, y], x, y);
                }
            }
        }

        /// <summary>
        /// Create a new grid without tile prices, eg with just walkable / unwalkable tiles.
        /// </summary>
        /// <param name="walkable_tiles">A 2d array representing which tiles are walkable and which are not.</param>
        public NodeGrid(bool[,] walkable_tiles)
        {
            // create nodes
            CreateNodes(walkable_tiles.GetLength(0), walkable_tiles.GetLength(1));

            // init nodes
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    nodes[x, y] = new Node(walkable_tiles[x, y] ? 1.0f : 0.0f, x, y);
                }
            }
        }

        /// <summary>
        /// Create the nodes grid and set size.
        /// </summary>
        /// <param name="width">Nodes grid width.</param>
        /// <param name="height">Nodes grid height.</param>
        private void CreateNodes(int width, int height)
        {
            gridSizeX = width;
            gridSizeY = height;
            nodes = new Node[gridSizeX, gridSizeY];
        }

        /// <summary>
        /// Updates the already created grid with new tile prices.
        /// </summary>
        /// <returns><c>true</c>, if grid was updated, <c>false</c> otherwise.</returns>
        /// <param name="tiles_costs">Tiles costs.</param>
        public void UpdateGrid (float[,] tiles_costs)
        {
            // check if need to re-create grid
            if (nodes == null ||
                gridSizeX != tiles_costs.GetLength(0) ||
                gridSizeY != tiles_costs.GetLength(1))
            {
                CreateNodes(tiles_costs.GetLength(0), tiles_costs.GetLength(1));
            }

            // update nodes
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    nodes[x, y].Update(tiles_costs[x, y], x, y);
                }
            }
        }

        /// <summary>
        /// Updates the already created grid without new tile prices, eg with just walkable / unwalkable tiles.
        /// </summary>
        /// <returns><c>true</c>, if grid was updated, <c>false</c> otherwise.</returns>
        /// <param name="walkable_tiles">Walkable tiles.</param>
        public void UpdateGrid (bool[,] walkable_tiles)
        {
            // check if need to re-create grid
            if (nodes == null ||
                gridSizeX != walkable_tiles.GetLength(0) ||
                gridSizeY != walkable_tiles.GetLength(1))
            {
                CreateNodes(walkable_tiles.GetLength(0), walkable_tiles.GetLength(1));
            }

            // update grid
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    nodes[x, y].Update(walkable_tiles[x, y] ? 1.0f : 0.0f, x, y);
                }
            }
        }

        /// <summary>
        /// Get all the neighbors of a given tile in the grid.
        /// </summary>
        /// <param name="node">Node to get neighbors for.</param>
        /// <returns>List of node neighbors.</returns>
        public System.Collections.IEnumerable GetNeighbours(Node node, Pathfinding.DistanceType distanceType)
        {
            int x = 0, y = 0;
            switch (distanceType)
            {
                case Pathfinding.DistanceType.Manhattan:
                    y = 0;
                    for (x = -1; x <= 1; ++x)
                    {
                        var neighbor = AddNodeNeighbour(x, y, node);
                        if (neighbor != null)
                            yield return neighbor;
                    }

                    x = 0;
                    for (y = -1; y <= 1; ++y)
                    {
                        var neighbor = AddNodeNeighbour(x, y, node);
                        if (neighbor != null)
                            yield return neighbor;
                    }
                    break;

                case Pathfinding.DistanceType.Euclidean:
                    for (x = -1; x <= 1; x++)
                    {
                        for (y = -1; y <= 1; y++)
                        {
                            var neighbor = AddNodeNeighbour(x, y, node);
                            if (neighbor != null)
                                yield return neighbor;
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Adds the node neighbour.
        /// </summary>
        /// <returns><c>true</c>, if node neighbour was added, <c>false</c> otherwise.</returns>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="node">Node.</param>
        /// <param name="neighbours">Neighbours.</param>
        Node AddNodeNeighbour(int x, int y, Node node)
        {
            if (x == 0 && y == 0)
            {
                return null;
            }

            int checkX = node.gridX + x;
            int checkY = node.gridY + y;

            if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
            {
                return nodes[checkX, checkY];
            }

            return null;
        }
    }

}