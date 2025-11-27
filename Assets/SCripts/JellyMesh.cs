using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class JellyMesh : MonoBehaviour
{
    [Header("Jelly Physics")]
    [SerializeField] private float stiffness = 50f;
    [SerializeField] private float damping = 5f;
    [SerializeField] private float mass = 0.3f;
    [SerializeField] private float gravity = -2f;
    
    [Header("Deformation")]
    [SerializeField] private float maxDeformation = 0.5f;
    [SerializeField] private float touchForceMultiplier = 5f;
    [SerializeField] private float touchForceRadius = 0.3f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    
    private struct JellyVertex
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 restPosition;
    }
    
    private Mesh originalMesh;
    private Mesh workingMesh;
    private JellyVertex[] jellyVertices;
    private Vector3[] currentVertexPositions;
    private MeshFilter meshFilter;
    private Transform meshTransform;
    private Vector3 lastPosition;
    private Vector3 meshVelocity;
    
    void Start()
    {
        InitializeJellyMesh();
    }
    
    void InitializeJellyMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshTransform = transform;
        
        // Create a copy of the original mesh
        originalMesh = meshFilter.sharedMesh;
        workingMesh = Instantiate(originalMesh);
        workingMesh.MarkDynamic(); // Optimize for frequent updates
        meshFilter.mesh = workingMesh;
        
        // Initialize jelly vertices
        Vector3[] vertices = workingMesh.vertices;
        jellyVertices = new JellyVertex[vertices.Length];
        currentVertexPositions = new Vector3[vertices.Length];
        
        for (int i = 0; i < vertices.Length; i++)
        {
            jellyVertices[i] = new JellyVertex
            {
                position = vertices[i],
                velocity = Vector3.zero,
                restPosition = vertices[i]
            };
            currentVertexPositions[i] = vertices[i];
        }
        
        lastPosition = meshTransform.position;
        
        Debug.Log($"JellyMesh initialized with {vertices.Length} vertices");
    }
    
    void FixedUpdate()
    {
        // Calculate mesh velocity for momentum
        meshVelocity = (meshTransform.position - lastPosition) / Time.fixedDeltaTime;
        lastPosition = meshTransform.position;
        
        // Update jelly physics
        UpdateJellyPhysics();
        
        // Apply to mesh
        UpdateMeshVertices();
    }
    
    void UpdateJellyPhysics()
    {
        float deltaTime = Time.fixedDeltaTime;
        
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            // Calculate spring force to rest position
            Vector3 displacement = jellyVertices[i].position - jellyVertices[i].restPosition;
            Vector3 springForce = -stiffness * displacement;
            
            // Damping force
            Vector3 dampingForce = -damping * jellyVertices[i].velocity;
            
            // Gravity force (downward pull for droopy effect)
            Vector3 gravityForce = Vector3.up * gravity * mass;
            
            // Momentum from mesh movement (creates inertia)
            Vector3 inertiaForce = -meshVelocity * mass * 0.5f;
            
            // Total force
            Vector3 totalForce = springForce + dampingForce + gravityForce + inertiaForce;
            
            // Apply physics integration
            Vector3 acceleration = totalForce / mass;
            jellyVertices[i].velocity += acceleration * deltaTime;
            jellyVertices[i].position += jellyVertices[i].velocity * deltaTime;
            
            // Constrain deformation to prevent mesh explosion
            Vector3 constrainedDisplacement = jellyVertices[i].position - jellyVertices[i].restPosition;
            if (constrainedDisplacement.magnitude > maxDeformation)
            {
                jellyVertices[i].position = jellyVertices[i].restPosition + 
                    constrainedDisplacement.normalized * maxDeformation;
            }
            
            // Store current position
            currentVertexPositions[i] = jellyVertices[i].position;
        }
    }
    
    void UpdateMeshVertices()
    {
        workingMesh.vertices = currentVertexPositions;
        workingMesh.RecalculateNormals(); // Required for proper lighting
        workingMesh.RecalculateBounds();
    }
    
    /// <summary>
    /// Apply force at a specific world point (for touch interaction)
    /// </summary>
    public void ApplyForceAtPoint(Vector3 worldPoint, Vector3 force)
    {
        // Convert world point to local space
        Vector3 localPoint = meshTransform.InverseTransformPoint(worldPoint);
        
        // Apply force to nearby vertices
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            float distance = Vector3.Distance(jellyVertices[i].position, localPoint);
            
            if (distance < touchForceRadius)
            {
                // Falloff based on distance
                float falloff = 1f - (distance / touchForceRadius);
                falloff = falloff * falloff; // Square for smoother falloff
                
                // Apply impulse force
                Vector3 localForce = meshTransform.InverseTransformDirection(force);
                jellyVertices[i].velocity += localForce * touchForceMultiplier * falloff;
            }
        }
    }
    
    /// <summary>
    /// Apply radial push force (pushes vertices away from point)
    /// </summary>
    public void ApplyPushForceAtPoint(Vector3 worldPoint, float forceMagnitude)
    {
        Vector3 localPoint = meshTransform.InverseTransformPoint(worldPoint);
        
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            float distance = Vector3.Distance(jellyVertices[i].position, localPoint);
            
            if (distance < touchForceRadius)
            {
                float falloff = 1f - (distance / touchForceRadius);
                falloff = falloff * falloff;
                
                // Direction away from touch point
                Vector3 pushDirection = (jellyVertices[i].position - localPoint).normalized;
                jellyVertices[i].velocity += pushDirection * forceMagnitude * falloff;
            }
        }
    }
    
    /// <summary>
    /// Apply inward pull force (pulls vertices toward point)
    /// </summary>
    public void ApplyPullForceAtPoint(Vector3 worldPoint, float forceMagnitude)
    {
        Vector3 localPoint = meshTransform.InverseTransformPoint(worldPoint);
        
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            float distance = Vector3.Distance(jellyVertices[i].position, localPoint);
            
            if (distance < touchForceRadius)
            {
                float falloff = 1f - (distance / touchForceRadius);
                falloff = falloff * falloff;
                
                // Direction toward touch point
                Vector3 pullDirection = (localPoint - jellyVertices[i].position).normalized;
                jellyVertices[i].velocity += pullDirection * forceMagnitude * falloff;
            }
        }
    }
    
    /// <summary>
    /// Add global shake/wobble to entire mesh
    /// </summary>
    public void AddGlobalWobble(Vector3 impulse)
    {
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            jellyVertices[i].velocity += impulse;
        }
    }
    
    /// <summary>
    /// Reset all vertices to rest position (useful for debugging)
    /// </summary>
    public void ResetToRestPosition()
    {
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            jellyVertices[i].position = jellyVertices[i].restPosition;
            jellyVertices[i].velocity = Vector3.zero;
        }
        UpdateMeshVertices();
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || jellyVertices == null || !Application.isPlaying)
            return;
        
        // Draw vertices in red, rest positions in green
        Gizmos.color = Color.red;
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            Vector3 worldPos = meshTransform.TransformPoint(jellyVertices[i].position);
            Gizmos.DrawSphere(worldPos, 0.02f);
        }
        
        Gizmos.color = Color.green;
        for (int i = 0; i < jellyVertices.Length; i++)
        {
            Vector3 worldRestPos = meshTransform.TransformPoint(jellyVertices[i].restPosition);
            Gizmos.DrawWireSphere(worldRestPos, 0.02f);
        }
    }
}
