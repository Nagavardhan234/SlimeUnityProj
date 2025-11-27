using UnityEngine;

/// <summary>
/// AGGRESSIVE color enforcer - runs after everything else to force light green.
/// Add this script LAST to override any white material issues.
/// </summary>
[DefaultExecutionOrder(1000)] // Runs AFTER all other scripts
public class ForceJellyColor : MonoBehaviour
{
    [Header("Forced Color")]
    [SerializeField] private Color forcedColor = new Color(0.5f, 1f, 0.8f, 1f); // Light green
    [SerializeField] private float emissionIntensity = 3f;
    
    private int forceAttempts = 0;
    private const int maxForceAttempts = 100; // Force for 100 frames
    
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
            
            // AGGRESSIVE: Set color multiple ways
            mat.SetColor("_Color", forcedColor);
            mat.color = forcedColor; // Unity's shorthand
            
            // Force emission AGGRESSIVELY
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", forcedColor * emissionIntensity);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            
            if (mat.HasProperty("_EmissionIntensity"))
                mat.SetFloat("_EmissionIntensity", emissionIntensity);
            
            // Force Fresnel to match
            if (mat.HasProperty("_FresnelColor"))
                mat.SetColor("_FresnelColor", forcedColor * 1.5f);
            
            // Force SSS color
            if (mat.HasProperty("_SSSColor"))
                mat.SetColor("_SSSColor", forcedColor * 1.2f);
            
            // Verify color was set
            Color verifyColor = mat.GetColor("_Color");
            if (verifyColor != forcedColor && forceAttempts < 10)
            {
                Debug.LogWarning($"Color mismatch on {rend.gameObject.name}: {verifyColor} != {forcedColor}");
            }
        }
        
        if (forceAttempts == 0 || forceAttempts == maxForceAttempts - 1)
        {
            Debug.Log($"<color=green>✓ FORCED LIGHT GREEN (attempt {forceAttempts + 1})</color>");
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
