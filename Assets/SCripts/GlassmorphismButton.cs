using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Advanced glassmorphism button with gradient, glow, particles, and satisfying feedback
/// </summary>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class GlassmorphismButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Gradient Colors")]
    public Color topColor = new Color(1f, 0.72f, 0.3f, 1f); // #FFB74D
    public Color bottomColor = new Color(1f, 0.57f, 0f, 1f); // #FF9100
    public Color glowColor = new Color(1f, 0.79f, 0.28f, 1f); // #FFC947
    
    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float pressScale = 0.92f;
    public float animationSpeed = 0.12f;
    
    [Header("Glow Settings")]
    public float glowIntensityIdle = 0.5f;
    public float glowIntensityHover = 1.0f;
    public float glowPulseSpeed = 1.2f; // 72 BPM heartbeat
    
    [Header("Particle Effects")]
    public bool enableParticles = true;
    public int particleCount = 10;
    public GameObject particlePrefab;
    
    private Button button;
    private Image image;
    private Material buttonMaterial;
    private Vector3 originalScale;
    private RectTransform rectTransform;
    
    private float currentScale = 1f;
    private float targetScale = 1f;
    private float currentGlow = 0.5f;
    private float targetGlow = 0.5f;
    
    private bool isHovering = false;
    private bool isPressed = false;
    private Coroutine pulseCoroutine;
    
    void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        
        SetupGradientMaterial();
        StartHeartbeatPulse();
    }
    
    void SetupGradientMaterial()
    {
        // Create material with gradient shader
        Shader gradientShader = Shader.Find("UI/GradientButton");
        if (gradientShader != null)
        {
            buttonMaterial = new Material(gradientShader);
            buttonMaterial.SetColor("_TopColor", topColor);
            buttonMaterial.SetColor("_BottomColor", bottomColor);
            buttonMaterial.SetColor("_GlowColor", glowColor);
            buttonMaterial.SetFloat("_GlowIntensity", glowIntensityIdle);
            
            image.material = buttonMaterial;
        }
        else
        {
            // Fallback to simple gradient if shader not found
            Debug.LogWarning($"GlassmorphismButton: Gradient shader not found for {gameObject.name}, using fallback");
        }
    }
    
    void Update()
    {
        // Smooth scale animation
        currentScale = Mathf.Lerp(currentScale, targetScale, animationSpeed);
        transform.localScale = originalScale * currentScale;
        
        // Smooth glow animation
        currentGlow = Mathf.Lerp(currentGlow, targetGlow, animationSpeed);
        if (buttonMaterial != null)
        {
            buttonMaterial.SetFloat("_GlowIntensity", currentGlow);
        }
    }
    
    void StartHeartbeatPulse()
    {
        // Subtle idle pulse at 72 BPM (human resting heart rate)
        pulseCoroutine = StartCoroutine(HeartbeatPulseCoroutine());
    }
    
    IEnumerator HeartbeatPulseCoroutine()
    {
        float pulseTime = 1.2f; // 72 BPM = 0.833s, slight slow for calm feel
        
        while (true)
        {
            if (!isHovering && !isPressed)
            {
                float t = Mathf.Sin(Time.time * Mathf.PI * 2f / pulseTime);
                float pulseMagnitude = 0.015f; // Very subtle
                targetScale = 1f + t * pulseMagnitude;
                targetGlow = glowIntensityIdle + t * 0.2f;
            }
            
            yield return null;
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isHovering = true;
        targetScale = hoverScale;
        targetGlow = glowIntensityHover;
        
        // Play hover sound (if audio system exists)
        PlayHoverSound();
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (!isPressed)
        {
            targetScale = 1f;
            targetGlow = glowIntensityIdle;
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isPressed = true;
        targetScale = pressScale;
        
        // Trigger press animation and particles
        StartCoroutine(PressEffectCoroutine());
        
        if (enableParticles)
        {
            SpawnPressParticles(eventData.position);
        }
        
        PlayPressSound();
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        targetScale = isHovering ? hoverScale : 1f;
    }
    
    IEnumerator PressEffectCoroutine()
    {
        // Quick press, then bounce back
        yield return new WaitForSeconds(0.06f);
        
        if (isHovering)
        {
            targetScale = hoverScale * 1.02f; // Slight overshoot
            yield return new WaitForSeconds(0.12f);
            targetScale = hoverScale;
        }
    }
    
    void SpawnPressParticles(Vector2 screenPosition)
    {
        // Convert screen position to world position
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform,
            screenPosition,
            Camera.main,
            out Vector3 worldPosition
        );
        
        // Spawn particles in radial pattern
        for (int i = 0; i < particleCount; i++)
        {
            float angle = (i / (float)particleCount) * 360f * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            // Create simple particle GameObject
            GameObject particle = CreateSimpleParticle(worldPosition, direction);
            
            // Animate particle
            StartCoroutine(AnimateParticle(particle, direction));
        }
    }
    
    GameObject CreateSimpleParticle(Vector3 position, Vector2 direction)
    {
        GameObject particle = new GameObject("Particle");
        particle.transform.SetParent(transform.parent, false);
        particle.transform.position = position;
        
        // Add image component
        Image particleImage = particle.AddComponent<Image>();
        particleImage.color = glowColor;
        particleImage.raycastTarget = false;
        
        // Small circle
        RectTransform rt = particle.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(8, 8);
        
        return particle;
    }
    
    IEnumerator AnimateParticle(GameObject particle, Vector2 direction)
    {
        float lifetime = Random.Range(0.6f, 1.0f);
        float speed = Random.Range(50f, 100f);
        Vector3 velocity = direction * speed;
        
        Image particleImage = particle.GetComponent<Image>();
        RectTransform rt = particle.GetComponent<RectTransform>();
        
        float elapsed = 0f;
        Vector3 startPos = particle.transform.position;
        
        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;
            
            // Move with gravity
            velocity.y -= 200f * Time.deltaTime;
            particle.transform.position += velocity * Time.deltaTime;
            
            // Fade out
            Color col = particleImage.color;
            col.a = 1f - t;
            particleImage.color = col;
            
            // Shrink
            float scale = 1f - t * 0.5f;
            rt.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        Destroy(particle);
    }
    
    void PlayHoverSound()
    {
        // TODO: Integrate with audio system
        // AudioManager.PlaySound("button_hover");
    }
    
    void PlayPressSound()
    {
        // TODO: Integrate with audio system
        // AudioManager.PlaySound("button_press");
    }
    
    void OnDestroy()
    {
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }
        
        if (buttonMaterial != null)
        {
            Destroy(buttonMaterial);
        }
    }
    
    // Public methods for external control
    public void SetColors(Color top, Color bottom, Color glow)
    {
        topColor = top;
        bottomColor = bottom;
        glowColor = glow;
        
        if (buttonMaterial != null)
        {
            buttonMaterial.SetColor("_TopColor", topColor);
            buttonMaterial.SetColor("_BottomColor", bottomColor);
            buttonMaterial.SetColor("_GlowColor", glowColor);
        }
    }
    
    public void TriggerSuccessFeedback()
    {
        StartCoroutine(SuccessFeedbackCoroutine());
    }
    
    IEnumerator SuccessFeedbackCoroutine()
    {
        // Burst of particles
        Vector2 center = rectTransform.position;
        for (int i = 0; i < 20; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            GameObject particle = CreateSimpleParticle(center, direction);
            StartCoroutine(AnimateParticle(particle, direction));
        }
        
        // Scale pulse
        float originalTarget = targetScale;
        targetScale = 1.2f;
        yield return new WaitForSeconds(0.15f);
        targetScale = originalTarget;
    }
}
