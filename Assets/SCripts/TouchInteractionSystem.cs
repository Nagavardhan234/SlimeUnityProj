using System.Collections;
using UnityEngine;

public class TouchInteractionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private JellyMesh jellyMesh;
    [SerializeField] private SlimeFaceController faceController;
    [SerializeField] private ParticleController particleController;
    [SerializeField] private Camera mainCamera;
    
    [Header("Touch Settings")]
    [SerializeField] private float tapThreshold = 0.2f; // Time threshold for tap vs hold
    [SerializeField] private float dragThreshold = 0.1f; // Distance threshold for drag
    
    [Header("Force Settings")]
    [SerializeField] private float tapForce = 3f;
    [SerializeField] private float holdForce = 1.5f;
    [SerializeField] private float dragForce = 2f;
    [SerializeField] private float pokeForce = 5f;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] squishSounds;
    [SerializeField] private AudioClip[] happySounds;
    [SerializeField] private float minPitchVariation = 0.9f;
    [SerializeField] private float maxPitchVariation = 1.1f;
    
    private bool isTouching = false;
    private float touchStartTime;
    private Vector3 touchStartPosition;
    private Vector3 lastTouchPosition;
    private InteractionType currentInteraction;
    
    public enum InteractionType
    {
        None,
        Tap,
        Hold,
        Drag,
        Poke
    }
    
    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (jellyMesh == null)
            jellyMesh = GetComponent<JellyMesh>();
        
        if (faceController == null)
            faceController = GetComponent<SlimeFaceController>();
        
        if (particleController == null)
            particleController = GetComponent<ParticleController>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0f; // 2D sound
            audioSource.playOnAwake = false;
        }
    }
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // Desktop: Mouse input
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchDown(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            OnTouchHold(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnTouchUp(Input.mousePosition);
        }
        
        // Mobile: Touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchDown(touch.position);
                    break;
                    
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    OnTouchHold(touch.position);
                    break;
                    
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    OnTouchUp(touch.position);
                    break;
            }
        }
    }
    
    void OnTouchDown(Vector3 screenPosition)
    {
        isTouching = true;
        touchStartTime = Time.time;
        touchStartPosition = screenPosition;
        lastTouchPosition = screenPosition;
        currentInteraction = InteractionType.None;
        
        // Try to raycast to slime
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                // Initial poke force
                if (jellyMesh != null)
                {
                    jellyMesh.ApplyPushForceAtPoint(hit.point, pokeForce * 0.5f);
                }
                
                // Spawn touch particle
                if (particleController != null)
                {
                    particleController.EmitTouchEffect(hit.point);
                }
            }
        }
    }
    
    void OnTouchHold(Vector3 screenPosition)
    {
        if (!isTouching) return;
        
        float touchDuration = Time.time - touchStartTime;
        float touchDistance = Vector3.Distance(screenPosition, touchStartPosition);
        
        // Determine interaction type
        if (touchDistance > dragThreshold * Screen.height)
        {
            currentInteraction = InteractionType.Drag;
            ProcessDrag(screenPosition);
        }
        else if (touchDuration > tapThreshold)
        {
            currentInteraction = InteractionType.Hold;
            ProcessHold(screenPosition);
        }
        
        lastTouchPosition = screenPosition;
    }
    
    void OnTouchUp(Vector3 screenPosition)
    {
        if (!isTouching) return;
        
        float touchDuration = Time.time - touchStartTime;
        float touchDistance = Vector3.Distance(screenPosition, touchStartPosition);
        
        // Determine final interaction type
        if (touchDistance < dragThreshold * Screen.height && touchDuration < tapThreshold)
        {
            currentInteraction = InteractionType.Tap;
            ProcessTap(screenPosition);
        }
        
        isTouching = false;
        currentInteraction = InteractionType.None;
    }
    
    void ProcessTap(Vector3 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                // Apply bounce force
                if (jellyMesh != null)
                {
                    jellyMesh.ApplyPushForceAtPoint(hit.point, tapForce);
                }
                
                // Face reaction
                if (faceController != null)
                {
                    faceController.ReactToInteraction(hit.point);
                }
                
                // Particles
                if (particleController != null)
                {
                    particleController.EmitTapEffect(hit.point);
                    particleController.EmitSparkles(hit.point, 5);
                }
                
                // Sound
                PlaySquishSound();
            }
        }
    }
    
    void ProcessHold(Vector3 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                // Gentle push force
                if (jellyMesh != null)
                {
                    Vector3 pushDirection = (hit.point - transform.position).normalized;
                    jellyMesh.ApplyForceAtPoint(hit.point, pushDirection * holdForce * Time.deltaTime);
                }
            }
        }
    }
    
    void ProcessDrag(Vector3 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                // Calculate drag direction
                Vector3 dragDirection = screenPosition - lastTouchPosition;
                Vector3 worldDrag = mainCamera.transform.TransformDirection(dragDirection.normalized);
                
                // Apply drag force (petting motion)
                if (jellyMesh != null)
                {
                    jellyMesh.ApplyForceAtPoint(hit.point, worldDrag * dragForce * Time.deltaTime);
                }
                
                // Emit sparkles along drag path
                if (particleController != null && Random.value > 0.7f)
                {
                    particleController.EmitSparkles(hit.point, 1);
                }
            }
        }
    }
    
    void PlaySquishSound()
    {
        if (audioSource == null || squishSounds == null || squishSounds.Length == 0)
            return;
        
        AudioClip clip = squishSounds[Random.Range(0, squishSounds.Length)];
        audioSource.pitch = Random.Range(minPitchVariation, maxPitchVariation);
        audioSource.PlayOneShot(clip);
    }
    
    void PlayHappySound()
    {
        if (audioSource == null || happySounds == null || happySounds.Length == 0)
            return;
        
        AudioClip clip = happySounds[Random.Range(0, happySounds.Length)];
        audioSource.pitch = Random.Range(minPitchVariation, maxPitchVariation);
        audioSource.PlayOneShot(clip);
    }
    
    /// <summary>
    /// Get current interaction type
    /// </summary>
    public InteractionType GetCurrentInteraction()
    {
        return currentInteraction;
    }
    
    /// <summary>
    /// Check if user is currently touching
    /// </summary>
    public bool IsTouching()
    {
        return isTouching;
    }
}
