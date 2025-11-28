using UnityEngine;

/// <summary>
/// Simplified color enforcer - applies matte finish for calm aesthetic.
/// No emission, no glow - pure, clean color only.
/// </summary>
[DefaultExecutionOrder(1000)] // Runs AFTER all other scripts
public class ForceJellyColor : MonoBehaviour
{
    [Header("Forced Color")]
    [SerializeField] private Color forcedColor = new Color(0.5f, 1f, 0.8f, 1f); // Light green
    [SerializeField] [Range(0f, 1f)] private float smoothness = 0.4f; // Matte finish
    [SerializeField] [Range(0f, 1f)] private float metallic = 0f; // Non-metallic
    
    private int forceAttempts = 0;
    private const int maxForceAttempts = 10; // Reduced - only force initially
    
    void Start()
    {
        ForceColor();
        
        // Force again after delays
        Invoke(nameof(ForceColor), 0.1f);
        Invoke(nameof(ForceColor), 0.3f);
        Invoke(nameof(ForceColor), 0.5f);
    }
    
    void Update()
    {
        // Keep forcing color every frame for first 100 frames
        if (forceAttempts < maxForceAttempts)
        {
            ForceColor();
            forceAttempts++;
        }
    }
    
    void ForceColor()
    {
        // Get ALL renderers (self and children)
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer rend in renderers)
        {
            // Skip eyes
            if (rend.gameObject.name.Contains("Eye") || rend.gameObject.name.Contains("Pupil"))
                continue;
            
            Material mat = rend.material; // Creates instance
            
            // Simple, clean color application
            mat.SetColor("_Color", forcedColor);
            mat.color = forcedColor;
            
            // Matte finish - no glow, no shine
            if (mat.HasProperty("_Smoothness"))
                mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness"))
                mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic"))
                mat.SetFloat("_Metallic", metallic);
            
            // DISABLE all emission and glow effects
            mat.DisableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.black);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            
            if (mat.HasProperty("_EmissionIntensity"))
                mat.SetFloat("_EmissionIntensity", 0f);
            
            // Disable Fresnel glow
            if (mat.HasProperty("_FresnelColor"))
                mat.SetColor("_FresnelColor", Color.black);
            if (mat.HasProperty("_FresnelPower"))
                mat.SetFloat("_FresnelPower", 0f);
            
            // Disable SSS glow
            if (mat.HasProperty("_SSSColor"))
                mat.SetColor("_SSSColor", Color.black);
            if (mat.HasProperty("_SSSIntensity"))
                mat.SetFloat("_SSSIntensity", 0f);
        }
        
        if (forceAttempts == 0)
        {
            Debug.Log($"<color=green>✓ Applied clean matte finish (no emission/glow)</color>");
        }
    }
    
    /// <summary>
    /// Change the forced color at runtime
    /// </summary>
    public void ChangeColor(Color newColor)
    {
        forcedColor = newColor;
        ForceColor();
    }
}
