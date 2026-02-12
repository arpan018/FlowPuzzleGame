using Game.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Utilities
{
    /// <summary>
    /// Hex grid utilities for neighbor calculations and connection validation.
    /// Uses odd-row offset coordinates (odd Y rows shifted +0.5 in X).
    /// </summary>
    public static class ConnectionChecker
    {
        #region Neighbors

        // Get neighbor grid position in specified hex direction (handles odd/even row offset)
        public static Vector2Int GetNeighborPosition(Vector2Int gridPos, HexDirection direction)
        {
            int x = gridPos.x;
            int y = gridPos.y;
            bool isOddRow = (y % 2 != 0); // Use this if your layout shifts every other row

            switch (direction)
            {
                case HexDirection.Top:
                    return new Vector2Int(x, y + 2); // Exact Top
                case HexDirection.Bottom:
                    return new Vector2Int(x, y - 2); // Exact Bottom

                // For staggered brick layouts, diagonal X depends on if the row is Odd/Even
                case HexDirection.TopRight:
                    return isOddRow ? new Vector2Int(x + 1, y + 1) : new Vector2Int(x, y + 1);
                case HexDirection.TopLeft:
                    return isOddRow ? new Vector2Int(x, y + 1) : new Vector2Int(x - 1, y + 1);
                case HexDirection.BottomRight:
                    return isOddRow ? new Vector2Int(x + 1, y - 1) : new Vector2Int(x, y - 1);
                case HexDirection.BottomLeft:
                    return isOddRow ? new Vector2Int(x, y - 1) : new Vector2Int(x - 1, y - 1);

                default: return gridPos;
            }
        }

        // Get all six neighbor positions for a grid position
        public static Vector2Int[] GetAllNeighborPositions(Vector2Int gridPos)
        {
            Vector2Int[] neighbors = new Vector2Int[6];
            
            neighbors[0] = GetNeighborPosition(gridPos, HexDirection.Top);
            neighbors[1] = GetNeighborPosition(gridPos, HexDirection.TopRight);
            neighbors[2] = GetNeighborPosition(gridPos, HexDirection.BottomRight);
            neighbors[3] = GetNeighborPosition(gridPos, HexDirection.Bottom);
            neighbors[4] = GetNeighborPosition(gridPos, HexDirection.BottomLeft);
            neighbors[5] = GetNeighborPosition(gridPos, HexDirection.TopLeft);

            return neighbors;
        }

        #endregion

        #region Directions

        // Determine hex direction from one position to another if adjacent (returns null if not neighbors)
        public static HexDirection? GetDirectionBetween(Vector2Int from, Vector2Int to)
        {
            for (int i = 0; i < 6; i++)
            {
                HexDirection direction = (HexDirection)i;
                Vector2Int neighborPos = GetNeighborPosition(from, direction);
                
                if (neighborPos == to)
                    return direction;
            }

            return null;
        }

        // Get opposite direction (e.g., Top -> Bottom, TopRight -> BottomLeft)
        public static HexDirection GetOppositeDirection(HexDirection direction)
        {
            return (HexDirection)(((int)direction + 3) % 6);
        }

        // Check if two positions are adjacent neighbors in hex grid
        public static bool ArePositionsAdjacent(Vector2Int posA, Vector2Int posB)
        {
            return GetDirectionBetween(posA, posB) != null;
        }

        #endregion

        #region Angles

        // Get angle in degrees for hex direction (0�, 60�, 120�, 180�, 240�, 300�)
        public static float GetDirectionAngle(HexDirection direction)
        {
            return HexDirectionHelper.GetAngle(direction);
        }

        #endregion

        #region Validation

        // Check if grid position is within bounds
        public static bool IsPositionValid(Vector2Int gridPos, int width, int height)
        {
            return gridPos.x >= 0 && gridPos.x < width && 
                   gridPos.y >= 0 && gridPos.y < height;
        }

        // Get all valid neighbor positions within grid bounds
        public static Vector2Int[] GetValidNeighborPositions(Vector2Int gridPos, int width, int height)
        {
            Vector2Int[] allNeighbors = GetAllNeighborPositions(gridPos);
            List<Vector2Int> validNeighbors = new List<Vector2Int>();

            foreach (Vector2Int neighbor in allNeighbors)
            {
                if (IsPositionValid(neighbor, width, height))
                    validNeighbors.Add(neighbor);
            }

            return validNeighbors.ToArray();
        }

        #endregion
    }
}
