using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A* (A-star) pathfinding: finds the shortest path between two points of the NavGrid,
/// going around walls. It always explores first the square that looks most promising:
///   f = g (steps already walked) + h (estimated steps left to the goal).
/// </summary>
public static class AStar
{
    private const float StraightCost = 1f;
    private const float DiagonalCost = 1.414f;

    // The 8 neighbours of a square: 4 straight + 4 diagonal.
    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
    };

    /// <summary>Returns the path as a list of world positions (empty if there is no way).</summary>
    public static List<Vector2> FindPath(NavGrid grid, Vector2 from, Vector2 to)
    {
        Vector2Int start = grid.GetClosestWalkable(grid.WorldToCell(from));
        Vector2Int goal = grid.GetClosestWalkable(grid.WorldToCell(to));

        var open = new List<Vector2Int> { start };                          // squares to explore
        var closed = new HashSet<Vector2Int>();                             // squares already explored
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();            // to rebuild the path at the end
        var costSoFar = new Dictionary<Vector2Int, float> { [start] = 0f }; // g

        while (open.Count > 0)
        {
            Vector2Int current = GetMostPromising(open, costSoFar, goal);
            if (current == goal)
            {
                return BuildPath(grid, cameFrom, current);
            }

            open.Remove(current);
            closed.Add(current);

            foreach (Vector2Int direction in Directions)
            {
                Vector2Int next = current + direction;
                if (closed.Contains(next) || !grid.IsWalkable(next) || CutsCorner(grid, current, direction))
                {
                    continue;
                }

                bool isDiagonal = direction.x != 0 && direction.y != 0;
                float newCost = costSoFar[current] + (isDiagonal ? DiagonalCost : StraightCost);
                if (costSoFar.TryGetValue(next, out float oldCost) && newCost >= oldCost)
                {
                    continue;
                }

                costSoFar[next] = newCost;
                cameFrom[next] = current;
                if (!open.Contains(next))
                {
                    open.Add(next);
                }
            }
        }

        return new List<Vector2>();
    }

    /// <summary>Picks the open square with the lowest f = g + h.</summary>
    private static Vector2Int GetMostPromising(List<Vector2Int> open, Dictionary<Vector2Int, float> costSoFar, Vector2Int goal)
    {
        Vector2Int best = open[0];
        float bestScore = float.MaxValue;
        foreach (Vector2Int cell in open)
        {
            float score = costSoFar[cell] + Vector2Int.Distance(cell, goal);
            if (score < bestScore)
            {
                bestScore = score;
                best = cell;
            }
        }

        return best;
    }

    /// <summary>A diagonal step is not allowed if it would squeeze past the corner of a wall.</summary>
    private static bool CutsCorner(NavGrid grid, Vector2Int from, Vector2Int direction)
    {
        bool isDiagonal = direction.x != 0 && direction.y != 0;
        return isDiagonal &&
               (!grid.IsWalkable(new Vector2Int(from.x + direction.x, from.y)) ||
                !grid.IsWalkable(new Vector2Int(from.x, from.y + direction.y)));
    }

    /// <summary>Walks backwards from the goal to the start using cameFrom, then flips the list.</summary>
    private static List<Vector2> BuildPath(NavGrid grid, Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int end)
    {
        var path = new List<Vector2> { grid.CellToWorld(end) };
        Vector2Int current = end;
        while (cameFrom.TryGetValue(current, out Vector2Int previous))
        {
            current = previous;
            path.Add(grid.CellToWorld(current));
        }

        path.Reverse();
        return path;
    }
}