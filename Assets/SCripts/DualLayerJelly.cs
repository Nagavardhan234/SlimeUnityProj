using System.Collections;
using UnityEngine;

/// <summary>
/// Manages dual-layer jelly effect with inner glowing core and outer membrane.
/// Creates realistic depth with independent breathing and wobble.
/// </summary>
public class DualLayerJelly : MonoBehaviour
{
    [Header("Layer References")]
    [SerializeField] private GameObject outerLayer;
    [SerializeField] private GameObject innerLayer;
    [SerializeField] private Renderer outerRenderer;
    [SerializeField] private Renderer innerRenderer;
    
    [Header("Layer Properties")]
    [SerializeField] private float innerLayerScale = 0.6f; // Smaller to stay inside
    [SerializeField] private float outerAlpha = 0.85f;
    [SerializeField] private float innerAlpha = 0.4f;
    [SerializeField] private float innerEmissionMultiplier = 4f;
    
    [Header("Color Breathing")]
    [SerializeField] private bool enableColorBreathing = false; // Disabled - SlimeController handles breathing
    [SerializeField] private float breathingSpeed = 1.2f;
    [SerializeField] private float breathingIntensity = 1f;
    [SerializeField] private float colorShiftAmount = 1.2f; // Reduced to prevent white
    [SerializeField] private float alphaBreatheRange = 0.12f;
    [SerializeField] private float emissionBreatheMin = 2f;
    [SerializeField] private float emissionBreatheMax = 5f;
    [SerializeField] private float innerBreathingMultiplier = 1.5f;
    
    [Header("Wobble Offset")]
    [SerializeField] private float wobbleOffsetAmount = 0.02f; // Reduced to prevent poking out
    [SerializeField] private float wobbleFrequencyX = 1.3f;
    [SerializeField] private float wobbleFrequencyY = 1.7f;
    [SerializeField] private float wobbleFrequencyZ = 1.1f;
    
    private Material outerMaterial;
    private Material innerMaterial;
    private Color baseColor = new Color(0.5f, 1f, 0.8f, 1f); // Light green default
    private Color brighterColor;
    private Vector3 innerLayerBaseLocalPosition = Vector3.zero;
    
    void Awake()
    {
        // DISABLED - Single layer mode, no dynamic material creation
        // SetupDualLayers();
        
        Debug.Log("<color=yellow>DualLayerJelly: DISABLED - Using single layer with shared material</color>");
    }
    
    void Start()
    {
        // DISABLED - CompleteColorFix handles all colors
        // baseColor = new Color(0.5f, 1f, 0.8f, 1f);
        // brighterColor = new Color(
        //     Mathf.Min(baseColor.r * colorShiftAmount, 0.8f),
        //     Mathf.Min(baseColor.g * colorShiftAmount, 1f),
        //     Mathf.Min(baseColor.b * colorShiftAmount, 0.95f),
        //     1f
        // );
        // ForceSetLightGreenColor();
        // StartCoroutine(DelayedColorForce());
        // Debug.Log($"Base Color: {baseColor}, Brighter Color: {brighterColor}");
    }
    
    IEnumerator DelayedColorForce()
    {
        yield return new WaitForSeconds(0.1f);
        ForceSetLightGreenColor();
    }
    
    void Update()
    {
        // FULLY DISABLED - No animations, no wobble, no color breathing
    }
    
    /// <summary>
    /// Sets up the dual-layer system with inner and outer layers
    /// </summary>
    void SetupDualLayers()
    {
        // Setup outer layer (this object)
        outerLayer = gameObject;
        outerRenderer = GetComponent<Renderer>();
        
        if (outerRenderer == null)
        {
            Debug.LogError("DualLayerJelly: No Renderer found on outer layer!");
            return;
        }
        
        // CREATE FRESH MATERIAL FROM SHADER (not from saved .mat file)
        Shader slimeShader = Shader.Find("Custom/SlimeGlow");
        if (slimeShader != null)
        {
            outerMaterial = new Material(slimeShader);
            outerRenderer.material = outerMaterial;
            Debug.Log("<color=lime>✓ Created FRESH material from Custom/SlimeGlow shader</color>");
        }
        else
        {
            outerMaterial = outerRenderer.material;
            Debug.LogWarning("<color=yellow>Custom/SlimeGlow shader not found, using existing material</color>");
        }
        
        // FORCE light green on all properties
        Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
        outerMaterial.SetColor("_Color", lightGreen);
        outerMaterial.SetColor("_EmissionColor", lightGreen * 3f);
        outerMaterial.EnableKeyword("_EMISSION");
        
        if (outerMaterial.HasProperty("_FresnelColor"))
            outerMaterial.SetColor("_FresnelColor", lightGreen * 1.5f);
        if (outerMaterial.HasProperty("_SSSColor"))
            outerMaterial.SetColor("_SSSColor", lightGreen * 1.2f);
        
        // Create inner layer as child
        CreateInnerLayer();
        
        // Setup materials with full properties
        SetupMaterials();
    }
    
    /// <summary>
    /// Creates the inner layer GameObject as a child
    /// </summary>
    void CreateInnerLayer()
    {
        // Check if inner layer already exists
        Transform existingInner = transform.Find("InnerLayer");
        if (existingInner != null)
        {
            innerLayer = existingInner.gameObject;
        }
        else
        {
            // Create new inner layer
            innerLayer = new GameObject("InnerLayer");
            innerLayer.transform.SetParent(transform);
            innerLayer.transform.localPosition = Vector3.zero;
            innerLayer.transform.localRotation = Quaternion.identity;
            innerLayer.transform.localScale = Vector3.one * innerLayerScale;
            
            // Copy mesh from outer layer
            MeshFilter outerMeshFilter = GetComponent<MeshFilter>();
            if (outerMeshFilter != null)
            {
                MeshFilter innerMeshFilter = innerLayer.AddComponent<MeshFilter>();
                innerMeshFilter.mesh = outerMeshFilter.mesh;
                
                innerRenderer = innerLayer.AddComponent<MeshRenderer>();
            }
            else
            {
                Debug.LogError("DualLayerJelly: No MeshFilter found on outer layer!");
                return;
            }
        }
        
        // Get or setup renderer
        if (innerRenderer == null)
        {
            innerRenderer = innerLayer.GetComponent<Renderer>();
        }
        
        if (innerRenderer != null)
        {
            // Create unique material instance for inner layer
            innerMaterial = new Material(outerMaterial.shader);
            
            // FORCE LIGHT GREEN - don't copy from outer (it's white at this point)
            Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
            
            innerMaterial.SetColor("_Color", lightGreen);
            innerMaterial.SetColor("_EmissionColor", lightGreen * 4f);
            innerMaterial.EnableKeyword("_EMISSION");
            
            if (innerMaterial.HasProperty("_FresnelColor"))
                innerMaterial.SetColor("_FresnelColor", lightGreen * 1.5f);
            
            if (innerMaterial.HasProperty("_SSSColor"))
                innerMaterial.SetColor("_SSSColor", lightGreen * 1.2f);
            
            if (innerMaterial.HasProperty("_EmissionIntensity"))
                innerMaterial.SetFloat("_EmissionIntensity", 5f);
            
            innerRenderer.material = innerMaterial;
            Debug.Log($"<color=lime>✓ InnerLayer: FORCED light green {lightGreen}</color>");
        }
    }
    
    /// <summary>
    /// Sets up material properties for both layers - FORCE LIGHT GREEN
    /// </summary>
    void SetupMaterials()
    {
        if (outerMaterial == null || innerMaterial == null) return;
        
        // FORCE LIGHT GREEN - ignore existing colors
        Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
        
        // Outer layer: light green with transparency
        Color outerColor = lightGreen;
        outerColor.a = outerAlpha;
        outerMaterial.SetColor("_Color", outerColor);
        
        // Inner layer: light green more transparent
        Color innerColor = lightGreen;
        innerColor.a = innerAlpha;
        innerMaterial.SetColor("_Color", innerColor);
        
        // Enable emission for both
        outerMaterial.EnableKeyword("_EMISSION");
        innerMaterial.EnableKeyword("_EMISSION");
        
        // Set emission colors to light green
        if (outerMaterial.HasProperty("_EmissionColor"))
        {
            outerMaterial.SetColor("_EmissionColor", lightGreen * 3f);
            innerMaterial.SetColor("_EmissionColor", lightGreen * innerEmissionMultiplier * 3f);
        }
        
        // Set emission intensity if property exists
        if (outerMaterial.HasProperty("_EmissionIntensity"))
        {
            outerMaterial.SetFloat("_EmissionIntensity", 3f);
            innerMaterial.SetFloat("_EmissionIntensity", 5f);
        }
        
        // Setup transparency
        SetupTransparency(outerMaterial);
        SetupTransparency(innerMaterial);
    }
    
    /// <summary>
    /// FORCE light green color on all materials, override everything
    /// </summary>
    void ForceSetLightGreenColor()
    {
        Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
        baseColor = lightGreen;
        // Clamp brighter color to stay in green range
        brighterColor = new Color(0.7f, 1f, 0.95f, 1f);
        
        if (outerMaterial != null)
        {
            Color outer = lightGreen;
            outer.a = outerAlpha;
            outerMaterial.SetColor("_Color", outer);
            
            if (outerMaterial.HasProperty("_EmissionColor"))
                outerMaterial.SetColor("_EmissionColor", lightGreen * 3f);
            if (outerMaterial.HasProperty("_EmissionIntensity"))
                outerMaterial.SetFloat("_EmissionIntensity", 3f);
        }
        
        if (innerMaterial != null)
        {
            Color inner = lightGreen;
            inner.a = innerAlpha;
            innerMaterial.SetColor("_Color", inner);
            
            if (innerMaterial.HasProperty("_EmissionColor"))
                innerMaterial.SetColor("_EmissionColor", lightGreen * innerEmissionMultiplier * 3f);
            if (innerMaterial.HasProperty("_EmissionIntensity"))
                innerMaterial.SetFloat("_EmissionIntensity", 5f);
        }
        
        Debug.Log($"FORCED Light Green Color: R={lightGreen.r}, G={lightGreen.g}, B={lightGreen.b}");
    }
    
    /// <summary>
    /// Configures material for transparency
    /// </summary>
    void SetupTransparency(Material mat)
    {
        // Set rendering mode to transparent
        mat.SetFloat("_Mode", 3); // Transparent mode
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
    
    /// <summary>
    /// Updates dynamic color breathing effect
    /// </summary>
    void UpdateColorBreathing()
    {
        if (outerMaterial == null || innerMaterial == null) return;
        
        float time = Time.time * breathingSpeed;
        
        // Outer layer breathing
        float outerPulse = Mathf.Sin(time) * 0.5f + 0.5f;
        UpdateLayerBreathing(outerMaterial, outerPulse, outerAlpha, breathingIntensity);
        
        // Inner layer breathing (more intense)
        float innerPulse = Mathf.Sin(time * 1.15f) * 0.5f + 0.5f; // Slightly offset frequency
        UpdateLayerBreathing(innerMaterial, innerPulse, innerAlpha, breathingIntensity * innerBreathingMultiplier);
    }
    
    /// <summary>
    /// Updates breathing effect for a specific layer
    /// </summary>
    void UpdateLayerBreathing(Material mat, float pulse, float baseAlpha, float intensity)
    {
        // Ensure we have valid colors
        if (baseColor == Color.white || baseColor == Color.clear)
        {
            baseColor = new Color(0.5f, 1f, 0.8f, 1f);
            brighterColor = new Color(0.7f, 1f, 0.95f, 1f);
        }
        
        // Color shift between base and brighter (with clamping)
        Color breatheColor = Color.Lerp(baseColor, brighterColor, pulse * Mathf.Clamp01(intensity * 0.5f));
        
        // Clamp RGB to prevent white overflow
        breatheColor.r = Mathf.Clamp(breatheColor.r, 0.4f, 0.8f);
        breatheColor.g = Mathf.Clamp(breatheColor.g, 0.8f, 1f);
        breatheColor.b = Mathf.Clamp(breatheColor.b, 0.7f, 0.95f);
        
        // Alpha breathing
        float breatheAlpha = baseAlpha + Mathf.Sin(pulse * Mathf.PI * 2f) * alphaBreatheRange;
        breatheAlpha = Mathf.Clamp01(breatheAlpha);
        breatheColor.a = breatheAlpha;
        
        mat.SetColor("_Color", breatheColor);
        
        // Emission breathing
        float emissionIntensity = Mathf.Lerp(emissionBreatheMin, emissionBreatheMax, pulse);
        Color emissionColor = baseColor * emissionIntensity;
        
        if (mat == innerMaterial)
        {
            emissionColor *= innerEmissionMultiplier;
        }
        
        mat.SetColor("_EmissionColor", emissionColor);
    }
    
    /// <summary>
    /// Updates inner layer wobble offset for organic movement
    /// </summary>
    void UpdateInnerLayerWobble()
    {
        if (innerLayer == null) return;
        
        float time = Time.time;
        
        // Calculate wobble offset with different sine frequencies
        Vector3 wobbleOffset = new Vector3(
            Mathf.Sin(time * wobbleFrequencyX) * wobbleOffsetAmount,
            Mathf.Sin(time * wobbleFrequencyY) * wobbleOffsetAmount,
            Mathf.Sin(time * wobbleFrequencyZ) * wobbleOffsetAmount
        );
        
        innerLayer.transform.localPosition = innerLayerBaseLocalPosition + wobbleOffset;
    }
    
    /// <summary>
    /// Sets the base color for both layers
    /// </summary>
    public void SetColor(Color newColor)
    {
        baseColor = newColor;
        brighterColor = baseColor * colorShiftAmount;
        
        if (outerMaterial != null)
        {
            Color outerColor = newColor;
            outerColor.a = outerAlpha;
            outerMaterial.SetColor("_Color", outerColor);
            
            // Also set emission color
            if (outerMaterial.HasProperty("_EmissionColor"))
            {
                outerMaterial.SetColor("_EmissionColor", newColor * 2f);
            }
        }
        
        if (innerMaterial != null)
        {
            Color innerColor = newColor;
            innerColor.a = innerAlpha;
            innerMaterial.SetColor("_Color", innerColor);
            
            // Inner gets brighter emission
            if (innerMaterial.HasProperty("_EmissionColor"))
            {
                innerMaterial.SetColor("_EmissionColor", newColor * innerEmissionMultiplier);
            }
        }
    }
    
    /// <summary>
    /// Smoothly transitions to a new color
    /// </summary>
    public void TransitionToColor(Color targetColor, float duration)
    {
        StartCoroutine(ColorTransitionCoroutine(targetColor, duration));
    }
    
    IEnumerator ColorTransitionCoroutine(Color targetColor, float duration)
    {
        Color startColor = baseColor;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            Color newColor = Color.Lerp(startColor, targetColor, t);
            SetColor(newColor);
            yield return null;
        }
        
        SetColor(targetColor);
    }
    
    /// <summary>
    /// Sets the intensity of the breathing effect
    /// </summary>
    public void SetBreathingIntensity(float intensity)
    {
        breathingIntensity = Mathf.Clamp01(intensity);
    }
    
    /// <summary>
    /// Enables or disables color breathing
    /// </summary>
    public void SetColorBreathingEnabled(bool enabled)
    {
        enableColorBreathing = enabled;
    }
    
    /// <summary>
    /// Gets the outer layer material
    /// </summary>
    public Material GetOuterMaterial()
    {
        return outerMaterial;
    }
    
    /// <summary>
    /// Gets the inner layer material
    /// </summary>
    public Material GetInnerMaterial()
    {
        return innerMaterial;
    }
}
