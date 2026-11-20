using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The museum seen as a grid of small squares. Each square is either walkable or blocked by a wall.
/// The A* algorithm uses this grid to find paths that go through the doors.
/// Put it on an empty object called "NavGrid" in the Museum scene.
/// </summary>
public class NavGrid : MonoBehaviour
{
    [Header("Area covered by the grid")]
    [Tooltip("Bottom-left corner of the museum.")]
    [SerializeField] private Vector2 bottomLeft = new Vector2(-15f, -10f);

    [Tooltip("Width and height of the museum.")]
    [SerializeField] private Vector2 size = new Vector2(30f, 20f);

    [Tooltip("Size of each square. Smaller = more precise but slower.")]
    [SerializeField] private float cellSize = 0.5f;

    [Tooltip("Size of a guard. A square is blocked if a guard standing there would touch a wall.")]
    [SerializeField] private float agentSize = 0.9f;

    [Header("Debug")]
    [Tooltip("Shows the blocked squares in red in the Scene view while playing.")]
    [SerializeField] private bool showBlockedCells = true;

    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private bool[,] walkable;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize => cellSize;

    private void Awake()
    {
        Build();
    }

    /// <summary>Checks every square once: is there a wall there?</summary>
    public void Build()
    {
        Width = Mathf.CeilToInt(size.x / cellSize);
        Height = Mathf.CeilToInt(size.y / cellSize);
        walkable = new bool[Width, Height];

        var solidOnly = new ContactFilter2D { useTriggers = false };
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                Vector2 center = CellToWorld(new Vector2Int(x, y));
                walkable[x, y] = !TouchesWall(center, solidOnly);
            }
        }
    }

    public bool IsWalkable(Vector2Int cell)
    {
        bool insideGrid = cell.x >= 0 && cell.y >= 0 && cell.x < Width && cell.y < Height;
        return insideGrid && walkable[cell.x, cell.y];
    }

    public Vector2Int WorldToCell(Vector2 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt((position.x - bottomLeft.x) / cellSize),
            Mathf.FloorToInt((position.y - bottomLeft.y) / cellSize));
    }

    public Vector2 CellToWorld(Vector2Int cell)
    {
        return bottomLeft + new Vector2((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize);
    }

    /// <summary>If a square is blocked, finds the nearest walkable one around it.</summary>
    public Vector2Int GetClosestWalkable(Vector2Int cell)
    {
        if (IsWalkable(cell))
        {
            return cell;
        }

        for (int radius = 1; radius < Mathf.Max(Width, Height); radius++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    bool onRing = Mathf.Abs(dx) == radius || Mathf.Abs(dy) == radius;
                    var candidate = new Vector2Int(cell.x + dx, cell.y + dy);
                    if (onRing && IsWalkable(candidate))
                    {
                        return candidate;
                    }
                }
            }
        }

        return cell;
    }

    private bool TouchesWall(Vector2 center, ContactFilter2D solidOnly)
    {
        int count = Physics2D.OverlapBox(center, Vector2.one * agentSize, 0f, solidOnly, overlaps);
        for (int i = 0; i < count; i++)
        {
            if (GuardVision.IsWall(overlaps[i]))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        // Outline of the grid area.
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(bottomLeft + size * 0.5f, size);

        if (!showBlockedCells || walkable == null)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (!walkable[x, y])
                {
                    Gizmos.DrawCube(CellToWorld(new Vector2Int(x, y)), Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}