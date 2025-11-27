using UnityEngine;

/// <summary>
/// Generates a high-poly sphere mesh for jelly deformation.
/// Attach this to a GameObject to create a subdivided sphere at runtime.
/// </summary>
public class JellySphereGenerator : MonoBehaviour
{
    [Header("Mesh Generation")]
    [SerializeField] private int subdivisions = 3; // 0=20 tris, 1=80, 2=320, 3=1280
    [SerializeField] private float radius = 1f;
    [SerializeField] private bool generateOnStart = true;
    
    [Header("Components")]
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    
    void Start()
    {
        if (generateOnStart)
        {
            GenerateSphere();
        }
    }
    
    [ContextMenu("Generate Sphere")]
    public void GenerateSphere()
    {
        // Get or add required components
        if (meshFilter == null)
            meshFilter = GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
        
        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();
        
        // Generate the mesh
        Mesh mesh = CreateSubdividedIcosphere(subdivisions, radius);
        mesh.name = $"JellySphere_Sub{subdivisions}";
        
        // Apply to mesh filter
        meshFilter.mesh = mesh;
        
        // Update collider if present (optional)
        if (meshCollider == null)
            meshCollider = GetComponent<MeshCollider>();
        
        if (meshCollider != null)
        {
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true; // Required for trigger collisions
        }
        
        Debug.Log($"Generated jelly sphere with {mesh.vertexCount} vertices, {mesh.triangles.Length / 3} triangles");
    }
    
    private Mesh CreateSubdividedIcosphere(int subdivisions, float radius)
    {
        Mesh mesh = new Mesh();
        
        // Start with icosahedron (20 faces, 12 vertices)
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-1,  t,  0).normalized,
            new Vector3( 1,  t,  0).normalized,
            new Vector3(-1, -t,  0).normalized,
            new Vector3( 1, -t,  0).normalized,
            new Vector3( 0, -1,  t).normalized,
            new Vector3( 0,  1,  t).normalized,
            new Vector3( 0, -1, -t).normalized,
            new Vector3( 0,  1, -t).normalized,
            new Vector3( t,  0, -1).normalized,
            new Vector3( t,  0,  1).normalized,
            new Vector3(-t,  0, -1).normalized,
            new Vector3(-t,  0,  1).normalized
        };
        
        int[] triangles = new int[]
        {
            0, 11, 5,   0, 5, 1,    0, 1, 7,    0, 7, 10,   0, 10, 11,
            1, 5, 9,    5, 11, 4,   11, 10, 2,  10, 7, 6,   7, 1, 8,
            3, 9, 4,    3, 4, 2,    3, 2, 6,    3, 6, 8,    3, 8, 9,
            4, 9, 5,    2, 4, 11,   6, 2, 10,   8, 6, 7,    9, 8, 1
        };
        
        // Subdivide
        for (int i = 0; i < subdivisions; i++)
        {
            SubdivideMesh(ref vertices, ref triangles);
        }
        
        // Scale to radius
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] *= radius;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    private void SubdivideMesh(ref Vector3[] vertices, ref int[] triangles)
    {
        var midPointCache = new System.Collections.Generic.Dictionary<long, int>();
        var newVertices = new System.Collections.Generic.List<Vector3>(vertices);
        var newTriangles = new System.Collections.Generic.List<int>();
        
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int v0 = triangles[i];
            int v1 = triangles[i + 1];
            int v2 = triangles[i + 2];
            
            // Get midpoints (or create them)
            int m0 = GetMidPoint(v0, v1, ref newVertices, ref midPointCache);
            int m1 = GetMidPoint(v1, v2, ref newVertices, ref midPointCache);
            int m2 = GetMidPoint(v2, v0, ref newVertices, ref midPointCache);
            
            // Create 4 new triangles
            newTriangles.Add(v0); newTriangles.Add(m0); newTriangles.Add(m2);
            newTriangles.Add(v1); newTriangles.Add(m1); newTriangles.Add(m0);
            newTriangles.Add(v2); newTriangles.Add(m2); newTriangles.Add(m1);
            newTriangles.Add(m0); newTriangles.Add(m1); newTriangles.Add(m2);
        }
        
        vertices = newVertices.ToArray();
        triangles = newTriangles.ToArray();
    }
    
    private int GetMidPoint(int v0, int v1, ref System.Collections.Generic.List<Vector3> vertices, 
        ref System.Collections.Generic.Dictionary<long, int> cache)
    {
        // Create unique key for edge
        long key = ((long)Mathf.Min(v0, v1) << 32) | (long)Mathf.Max(v0, v1);
        
        if (cache.TryGetValue(key, out int midIndex))
        {
            return midIndex;
        }
        
        // Create new midpoint
        Vector3 mid = ((vertices[v0] + vertices[v1]) * 0.5f).normalized;
        int newIndex = vertices.Count;
        vertices.Add(mid);
        cache[key] = newIndex;
        
        return newIndex;
    }
    
    /// <summary>
    /// Get vertex count estimate before generating
    /// </summary>
    public static int GetEstimatedVertexCount(int subdivisions)
    {
        int triangleCount = 20; // Base icosahedron has 20 faces
        
        for (int i = 0; i < subdivisions; i++)
        {
            triangleCount *= 4;
        }
        
        // Approximate vertex count (not exact due to shared vertices)
        return 10 + (triangleCount * 3) / 2;
    }
}
