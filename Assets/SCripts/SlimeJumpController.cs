using System.Collections;
using UnityEngine;

/// <summary>
/// Controls slime jumping animation with natural jelly wobble.
/// Shows how outer layer wobbles like real jelly with speed control.
/// </summary>
public class SlimeJumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float jumpDuration = 0.8f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private bool autoJump = false;
    [SerializeField] private float autoJumpInterval = 3f;
    
    [Header("Wobble Settings")]
    [SerializeField] private float wobbleSpeed = 15f;
    [SerializeField] private float wobbleDecay = 5f;
    [SerializeField] private float squashAmount = 0.3f;
    [SerializeField] private float stretchAmount = 0.4f;
    [SerializeField] private float impactForce = 8f;
    
    [Header("Jelly Physics")]
    [SerializeField] private JellyMesh jellyMesh;
    [SerializeField] private float jellyImpactMultiplier = 1.5f;
    [SerializeField] private float jellyAnticipationForce = 3f;
    
    [Header("Particles")]
    [SerializeField] private ParticleController particleController;
    [SerializeField] private bool emitParticlesOnLand = true;
    
    private Vector3 startPosition;
    private bool isJumping = false;
    private float wobbleAmount = 0f;
    private Vector3 wobbleVelocity = Vector3.zero;
    private Vector3 originalScale;
    private float nextAutoJumpTime;
    
    void Start()
    {
        startPosition = transform.position;
        originalScale = transform.localScale;
        
        if (jellyMesh == null)
            jellyMesh = GetComponent<JellyMesh>();
        
        if (particleController == null)
            particleController = GetComponent<ParticleController>();
        
        if (autoJump)
        {
            nextAutoJumpTime = Time.time + autoJumpInterval;
        }
    }
    
    void Update()
    {
        // Auto jump if enabled
        if (autoJump && !isJumping && Time.time >= nextAutoJumpTime)
        {
            Jump();
            nextAutoJumpTime = Time.time + autoJumpInterval;
        }
        
        // Manual jump with spacebar
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            Jump();
        }
        
        // Apply wobble physics
        UpdateWobble();
    }
    
    /// <summary>
    /// Trigger a jump
    /// </summary>
    public void Jump()
    {
        if (!isJumping)
        {
            StartCoroutine(JumpCoroutine());
        }
    }
    
    IEnumerator JumpCoroutine()
    {
        isJumping = true;
        
        // Anticipation phase - squash down
        yield return StartCoroutine(SquashAnticipation());
        
        // Jump up phase
        float elapsed = 0f;
        float halfDuration = jumpDuration * 0.5f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            
            // Rising - stretch upward
            float heightProgress = jumpCurve.Evaluate(t);
            float currentHeight = heightProgress * jumpHeight;
            
            transform.position = startPosition + Vector3.up * currentHeight;
            
            // Stretch effect while rising
            float stretch = Mathf.Lerp(1f, 1f + stretchAmount, t);
            float compress = Mathf.Lerp(1f, 1f - stretchAmount * 0.5f, t);
            transform.localScale = new Vector3(
                originalScale.x * compress,
                originalScale.y * stretch,
                originalScale.z * compress
            );
            
            // Apply upward force to jelly mesh
            if (jellyMesh != null)
            {
                jellyMesh.AddGlobalWobble(Vector3.up * Time.deltaTime * jellyAnticipationForce);
            }
            
            yield return null;
        }
        
        // Fall down phase
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            
            // Falling - compress downward
            float heightProgress = jumpCurve.Evaluate(1f - t);
            float currentHeight = heightProgress * jumpHeight;
            
            transform.position = startPosition + Vector3.up * currentHeight;
            
            // Compress effect while falling
            float stretch = Mathf.Lerp(1f + stretchAmount, 1f, t);
            float compress = Mathf.Lerp(1f - stretchAmount * 0.5f, 1f, t);
            transform.localScale = new Vector3(
                originalScale.x * compress,
                originalScale.y * stretch,
                originalScale.z * compress
            );
            
            // Apply downward force as falling
            if (jellyMesh != null && t > 0.5f)
            {
                jellyMesh.AddGlobalWobble(Vector3.down * Time.deltaTime * jellyAnticipationForce * 2f);
            }
            
            yield return null;
        }
        
        // Landing impact
        yield return StartCoroutine(LandingImpact());
        
        isJumping = false;
    }
    
    IEnumerator SquashAnticipation()
    {
        float duration = 0.15f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Squash down
            float squash = Mathf.Lerp(1f, 1f - squashAmount, t);
            float expand = Mathf.Lerp(1f, 1f + squashAmount * 0.5f, t);
            
            transform.localScale = new Vector3(
                originalScale.x * expand,
                originalScale.y * squash,
                originalScale.z * expand
            );
            
            // Apply downward force to jelly mesh
            if (jellyMesh != null)
            {
                jellyMesh.AddGlobalWobble(Vector3.down * Time.deltaTime * jellyAnticipationForce);
            }
            
            yield return null;
        }
    }
    
    IEnumerator LandingImpact()
    {
        // Initial impact squash
        float duration = 0.2f;
        float elapsed = 0f;
        
        // Strong wobble from impact
        wobbleAmount = impactForce;
        wobbleVelocity = new Vector3(
            Random.Range(-impactForce, impactForce),
            0,
            Random.Range(-impactForce, impactForce)
        );
        
        // Apply massive force to jelly mesh for realistic impact
        if (jellyMesh != null)
        {
            jellyMesh.AddGlobalWobble(Vector3.down * jellyImpactMultiplier * impactForce);
            
            // Add radial wobble from center
            Vector3 center = transform.position;
            jellyMesh.ApplyPushForceAtPoint(center, impactForce * jellyImpactMultiplier);
        }
        
        // Emit landing particles
        if (emitParticlesOnLand && particleController != null)
        {
            Vector3 landingPos = transform.position - Vector3.up * 0.5f;
            particleController.EmitTapEffect(landingPos);
            particleController.EmitSparkles(landingPos, 10);
        }
        
        // Impact squash
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Squash then bounce back
            float squashCurve = Mathf.Sin(t * Mathf.PI);
            float squash = 1f - (squashAmount * 1.5f * squashCurve);
            float expand = 1f + (squashAmount * 0.7f * squashCurve);
            
            transform.localScale = new Vector3(
                originalScale.x * expand,
                originalScale.y * squash,
                originalScale.z * expand
            );
            
            yield return null;
        }
        
        // Return to original scale
        transform.localScale = originalScale;
    }
    
    void UpdateWobble()
    {
        if (isJumping) return;
        
        // Decay wobble over time
        wobbleAmount = Mathf.Lerp(wobbleAmount, 0f, Time.deltaTime * wobbleDecay);
        wobbleVelocity = Vector3.Lerp(wobbleVelocity, Vector3.zero, Time.deltaTime * wobbleDecay);
        
        if (wobbleAmount > 0.01f)
        {
            // Apply wobble to scale
            float wobbleX = Mathf.Sin(Time.time * wobbleSpeed + wobbleVelocity.x) * wobbleAmount;
            float wobbleY = Mathf.Cos(Time.time * wobbleSpeed * 1.3f) * wobbleAmount;
            float wobbleZ = Mathf.Sin(Time.time * wobbleSpeed * 0.8f + wobbleVelocity.z) * wobbleAmount;
            
            Vector3 wobbleScale = new Vector3(
                1f + wobbleX * 0.1f,
                1f + wobbleY * 0.1f,
                1f + wobbleZ * 0.1f
            );
            
            transform.localScale = Vector3.Scale(originalScale, wobbleScale);
        }
        else
        {
            // Ensure scale returns to original when wobble stops
            if (Vector3.Distance(transform.localScale, originalScale) > 0.001f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, Time.deltaTime * 5f);
            }
        }
    }
    
    /// <summary>
    /// Set jump height
    /// </summary>
    public void SetJumpHeight(float height)
    {
        jumpHeight = height;
    }
    
    /// <summary>
    /// Set wobble speed (higher = faster oscillation)
    /// </summary>
    public void SetWobbleSpeed(float speed)
    {
        wobbleSpeed = speed;
    }
    
    /// <summary>
    /// Set impact force (affects landing wobble intensity)
    /// </summary>
    public void SetImpactForce(float force)
    {
        impactForce = force;
    }
    
    /// <summary>
    /// Enable/disable auto jump
    /// </summary>
    public void SetAutoJump(bool enabled, float interval = 3f)
    {
        autoJump = enabled;
        autoJumpInterval = interval;
        if (enabled)
        {
            nextAutoJumpTime = Time.time + interval;
        }
    }
}
