using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Draws the guard's two vision cones in the game, cut off by the walls.
/// Colors: calm = soft yellow, suspicious = orange, spotted = red.
/// Put it on the Guard, next to GuardVision.
/// </summary>
[RequireComponent(typeof(GuardVision))]
public class VisionConeRenderer : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("More rays = smoother cones.")]
    [SerializeField, Range(4, 64)] private int raysPerCone = 30;
    [SerializeField] private Color calmColor = new Color(1f, 1f, 0.6f, 0.25f);
    [SerializeField] private Color suspiciousColor = new Color(1f, 0.65f, 0f, 0.35f);
    [SerializeField] private Color spottedColor = new Color(1f, 0.1f, 0.1f, 0.4f);

    [Header("Drawing order")]
    [Tooltip("Same as the Floor (-1): the cones are drawn over the floor but under walls, guards and thief.")]
    [SerializeField] private int sortingOrder = -1;

    // Slightly closer to the camera than the floor, so the cones appear on top of it.
    private const float InFrontOfFloor = -0.05f;

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Color> colors = new List<Color>();
    private readonly List<int> triangles = new List<int>();

    private GuardVision vision;
    private Transform coneTransform;
    private Mesh mesh;

    private void Awake()
    {
        vision = GetComponent<GuardVision>();
        CreateConeObject();
    }

    // LateUpdate runs after the guard has moved and turned this frame.
    private void LateUpdate()
    {
        vertices.Clear();
        colors.Clear();
        triangles.Clear();

        Color color = GetColor(vision.CurrentAwareness);
        AddCone(vision.FarRange, vision.FarAngle, color);
        AddCone(vision.NearRange, vision.NearAngle, color);

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    private void OnDestroy()
    {
        if (mesh != null)
        {
            Destroy(mesh);
        }
    }

    /// <summary>Creates a child object "VisionCone" that holds the cone shape.</summary>
    private void CreateConeObject()
    {
        var cone = new GameObject("VisionCone");
        coneTransform = cone.transform;
        coneTransform.SetParent(transform, false);

        mesh = new Mesh { name = "VisionCone" };
        mesh.MarkDynamic();
        cone.AddComponent<MeshFilter>().sharedMesh = mesh;

        var meshRenderer = cone.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.sortingOrder = sortingOrder;
    }

    /// <summary>
    /// A cone is a "fan" of triangles: one point at the guard plus one point at the end of each ray.
    /// Each ray stops when it hits a wall, so the cone never goes through walls.
    /// </summary>
    private void AddCone(float range, float angle, Color color)
    {
        Vector3 origin = transform.position;
        int centerIndex = vertices.Count;
        AddVertex(origin, color);

        for (int i = 0; i <= raysPerCone; i++)
        {
            float rayAngle = Mathf.Lerp(-angle * 0.5f, angle * 0.5f, i / (float)raysPerCone);
            Vector2 direction = Quaternion.Euler(0f, 0f, rayAngle) * vision.Facing;
            float distance = vision.DistanceToWall(direction, range);
            AddVertex(origin + (Vector3)(direction * distance), color);

            if (i > 0)
            {
                triangles.Add(centerIndex);
                triangles.Add(vertices.Count - 2);
                triangles.Add(vertices.Count - 1);
            }
        }
    }

    private void AddVertex(Vector3 worldPoint, Color color)
    {
        worldPoint.z += InFrontOfFloor;
        vertices.Add(coneTransform.InverseTransformPoint(worldPoint));
        colors.Add(color);
    }

    private Color GetColor(Awareness awareness)
    {
        switch (awareness)
        {
            case Awareness.Spotted: return spottedColor;
            case Awareness.Suspicious: return suspiciousColor;
            default: return calmColor;
        }
    }
}
