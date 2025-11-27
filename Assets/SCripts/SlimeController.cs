using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main controller for the virtual pet slime. Coordinates all subsystems.
/// </summary>
public class SlimeController : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private JellyMesh jellyMesh;
    [SerializeField] private SlimeFaceController faceController;
    [SerializeField] private TouchInteractionSystem touchSystem;
    [SerializeField] private ParticleController particleController;
    [SerializeField] private Renderer slimeRenderer;
    [SerializeField] private DualLayerJelly dualLayerJelly;
    [SerializeField] private SlimeJumpController jumpController;
    
    [Header("Visual Effects")]
    [SerializeField] private Color glowColor = new Color(0.5f, 1f, 0.8f, 1f); // LIGHT GREEN DEFAULT
    [SerializeField] private float baseEmissionIntensity = 3f;
    [SerializeField] private float fresnelPower = 3f;
    [SerializeField] private bool forceColorOverride = true;
    
    [Header("Idle Breathing")]
    [SerializeField] private float breatheSpeed = 1.5f;
    [SerializeField] private float breatheIntensityMin = 1.5f;
    [SerializeField] private float breatheIntensityMax = 3f;
    [SerializeField] private float idleWobbleInterval = 3f;
    
    [Header("Dynamic Color Breathing")]
    [SerializeField] private bool useDualLayerBreathing = true;
    [SerializeField] private float colorBreathingSpeed = 1.2f;
    [SerializeField] private float colorShiftIntensity = 1.35f;
    [SerializeField] private float alphaBreatheAmount = 0.12f;
    [SerializeField] private float emissionBreathingMin = 2f;
    [SerializeField] private float emissionBreathingMax = 5f;
    
    [Header("Interaction Response")]
    [SerializeField] private float interactionEmissionBoost = 5f;
    [SerializeField] private float interactionDuration = 0.5f;
    
    private Material slimeMaterial;
    private bool isInteracting = false;
    private float interactionTimer = 0f;
    private float nextIdleWobbleTime = 0f;
    
    void Start()
    {
        // Get required components
        if (jellyMesh == null)
            jellyMesh = GetComponent<JellyMesh>();
        
        if (faceController == null)
            faceController = GetComponent<SlimeFaceController>();
        
        if (touchSystem == null)
            touchSystem = GetComponent<TouchInteractionSystem>();
        
        if (particleController == null)
            particleController = GetComponent<ParticleController>();
        
        if (slimeRenderer == null)
            slimeRenderer = GetComponent<Renderer>();
        
        // NO material modification - use shared material as-is
        Debug.Log("<color=lime>✓ SlimeController: Using shared material without modification</color>");
    }
    
    void Update()
    {
        // All animations disabled - simple virtual pet mode
        UpdateInteraction();
    }
    
    void SetupMaterial()
    {
        // FORCE light green color
        Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
        glowColor = lightGreen;
        
        // Set main color
        slimeMaterial.SetColor("_Color", lightGreen);
        
        // Enable and set emission
        slimeMaterial.EnableKeyword("_EMISSION");
        slimeMaterial.SetColor("_EmissionColor", lightGreen * baseEmissionIntensity);
        
        // Set emission intensity if available
        if (slimeMaterial.HasProperty("_EmissionIntensity"))
        {
            slimeMaterial.SetFloat("_EmissionIntensity", baseEmissionIntensity);
        }
        
        // Setup custom shader properties
        if (slimeMaterial.HasProperty("_FresnelPower"))
        {
            slimeMaterial.SetFloat("_FresnelPower", fresnelPower);
        }
        
        // Fresnel color should also be light green
        if (slimeMaterial.HasProperty("_FresnelColor"))
        {
            slimeMaterial.SetColor("_FresnelColor", lightGreen * 1.5f);
        }
        
        Debug.Log($"SlimeController: Set light green color");
    }
    
    void ForceColorOverride()
    {
        Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
        
        // Force on main material
        if (slimeMaterial != null)
        {
            slimeMaterial.SetColor("_Color", lightGreen);
            slimeMaterial.SetColor("_EmissionColor", lightGreen * baseEmissionIntensity);
        }
        
        // Force on dual layer if exists
        if (dualLayerJelly != null)
        {
            dualLayerJelly.SetColor(lightGreen);
        }
        
        Debug.Log("FORCED color override to light green!");
    }
    
    IEnumerator DelayedColorOverride()
    {
        yield return new WaitForSeconds(0.1f);
        if (forceColorOverride)
        {
            ForceColorOverride();
        }
    }
    
    void CheckIdleWobble()
    {
        // DISABLED - No automatic wobble animations
    }
    
    void UpdateInteraction()
    {
        if (isInteracting)
        {
            interactionTimer -= Time.deltaTime;
            if (interactionTimer <= 0f)
            {
                isInteracting = false;
            }
        }
    }
    
    void UpdateEmission()
    {
        if (slimeMaterial == null) return;
        
        // If using dual-layer system, let it handle breathing
        if (dualLayerJelly != null && useDualLayerBreathing)
        {
            UpdateDualLayerEmission();
            return;
        }
        
        // Legacy single-layer emission
        float emissionIntensity = baseEmissionIntensity;
        
        if (isInteracting)
        {
            // Boost emission during interaction
            float t = interactionTimer / interactionDuration;
            emissionIntensity += interactionEmissionBoost * t;
        }
        else
        {
            // Gentle pulse during idle
            float pulse = Mathf.Sin(Time.time * breatheSpeed) * 0.5f + 0.5f;
            emissionIntensity = Mathf.Lerp(breatheIntensityMin, breatheIntensityMax, pulse);
        }
        
        slimeMaterial.SetColor("_EmissionColor", glowColor * emissionIntensity);
    }
    
    /// <summary>
    /// Updates emission for dual-layer system with enhanced breathing
    /// </summary>
    void UpdateDualLayerEmission()
    {
        float time = Time.time * colorBreathingSpeed;
        float pulse = Mathf.Sin(time) * 0.5f + 0.5f;
        
        // Calculate base emission intensity with breathing
        float emissionIntensity = Mathf.Lerp(emissionBreathingMin, emissionBreathingMax, pulse);
        
        if (isInteracting)
        {
            // Boost emission during interaction
            float t = interactionTimer / interactionDuration;
            emissionIntensity += interactionEmissionBoost * t;
        }
        
        // Update both layers
        Material outerMat = dualLayerJelly.GetOuterMaterial();
        Material innerMat = dualLayerJelly.GetInnerMaterial();
        
        if (outerMat != null)
        {
            // Ensure glow color is light green
            Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
            glowColor = lightGreen;
            
            // Color shift for breathing effect (clamped to stay green)
            Color brighterGreen = new Color(0.7f, 1f, 0.95f, 1f);
            Color breatheColor = Color.Lerp(lightGreen, brighterGreen, pulse * 0.5f);
            
            // Alpha breathing
            float alphaVariation = Mathf.Sin(pulse * Mathf.PI * 2f) * alphaBreatheAmount;
            breatheColor.a = Mathf.Clamp01(0.85f + alphaVariation);
            outerMat.SetColor("_Color", breatheColor);
            
            outerMat.SetColor("_EmissionColor", lightGreen * emissionIntensity);
        }
        
        if (innerMat != null)
        {
            // Inner layer breathes more intensely but stays green
            Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
            float innerPulse = Mathf.Sin(time * 1.15f) * 0.5f + 0.5f;
            
            // Brighter green for inner core
            Color brighterGreen = new Color(0.7f, 1f, 0.95f, 1f);
            Color innerBreatheColor = Color.Lerp(lightGreen, brighterGreen, innerPulse * 0.7f);
            
            float innerAlphaVariation = Mathf.Sin(innerPulse * Mathf.PI * 2f) * (alphaBreatheAmount * 1.5f);
            innerBreatheColor.a = Mathf.Clamp01(0.4f + innerAlphaVariation);
            innerMat.SetColor("_Color", innerBreatheColor);
            
            innerMat.SetColor("_EmissionColor", lightGreen * emissionIntensity * 4f);
        }
    }
    
    /// <summary>
    /// Called when player interacts with slime (legacy - now handled by TouchInteractionSystem)
    /// </summary>
    public void OnInteract()
    {
        isInteracting = true;
        interactionTimer = interactionDuration;
        
        // Add global wobble to jelly mesh
        if (jellyMesh != null)
        {
            jellyMesh.AddGlobalWobble(Vector3.up * 2f);
        }
        
        // Emit happy particles
        if (particleController != null)
        {
            particleController.EmitHappyEffect(transform.position + Vector3.up);
        }
    }
    
    /// <summary>
    /// Get the slime's current emotion from face controller
    /// </summary>
    public SlimeFaceController.SlimeEmotion GetEmotion()
    {
        if (faceController != null)
            return faceController.GetCurrentEmotion();
        return SlimeFaceController.SlimeEmotion.Happy;
    }
    
    /// <summary>
    /// Set the slime's emotion
    /// </summary>
    public void SetEmotion(SlimeFaceController.SlimeEmotion emotion)
    {
        if (faceController != null)
            faceController.SetEmotion(emotion);
    }
    
    /// <summary>
    /// Change the slime's glow color
    /// </summary>
    public void SetGlowColor(Color newColor)
    {
        glowColor = newColor;
        
        // Update dual-layer system if available
        if (dualLayerJelly != null && useDualLayerBreathing)
        {
            dualLayerJelly.SetColor(newColor);
        }
        else if (slimeMaterial != null)
        {
            slimeMaterial.SetColor("_EmissionColor", glowColor * baseEmissionIntensity);
            slimeMaterial.SetColor("_Color", newColor);
        }
        
        if (particleController != null)
        {
            particleController.SetSparkleColor(newColor);
        }
    }
    
    /// <summary>
    /// Smoothly transition to a new color (dual-layer support)
    /// </summary>
    public void TransitionToColor(Color targetColor, float duration)
    {
        if (dualLayerJelly != null && useDualLayerBreathing)
        {
            dualLayerJelly.TransitionToColor(targetColor, duration);
        }
        else
        {
            StartCoroutine(ColorTransitionCoroutine(targetColor, duration));
        }
    }
    
    IEnumerator ColorTransitionCoroutine(Color targetColor, float duration)
    {
        Color startColor = glowColor;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            SetGlowColor(Color.Lerp(startColor, targetColor, t));
            yield return null;
        }
        
        SetGlowColor(targetColor);
    }
    
    /// <summary>
    /// Set the breathing intensity for color effects
    /// </summary>
    public void SetBreathingIntensity(float intensity)
    {
        if (dualLayerJelly != null)
        {
            dualLayerJelly.SetBreathingIntensity(intensity);
        }
    }
}
