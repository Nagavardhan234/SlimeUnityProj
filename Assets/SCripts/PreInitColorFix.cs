using UnityEngine;
using System.Collections;

/// <summary>
/// ULTIMATE FIX: Runs BEFORE everything else and sets up materials correctly
/// This runs at execution order -100 to beat all other scripts
/// </summary>
[DefaultExecutionOrder(-100)]
public class PreInitColorFix : MonoBehaviour
{
    private static readonly Color LIGHT_GREEN = new Color(0.5f, 1f, 0.8f, 1f);
    
    void Awake()
    {
        Debug.Log("<color=yellow>▶ PreInitColorFix: Starting BEFORE all other scripts...</color>");
        FixAllMaterials();
    }
    
    void Start()
    {
        StartCoroutine(ContinuousColorFix());
    }
    
    void FixAllMaterials()
    {
        // Get ALL renderers including inactive
        Renderer[] allRenderers = Resources.FindObjectsOfTypeAll<Renderer>();
        
        foreach (Renderer rend in allRenderers)
        {
            // Only fix renderers in this GameObject's hierarchy
            if (!rend.transform.IsChildOf(transform) && rend.gameObject != gameObject)
                continue;
            
            // Skip eyes
            if (rend.gameObject.name.Contains("Eye") || rend.gameObject.name.Contains("Pupil"))
                continue;
            
            // Fix existing material or create new one
            Material mat = rend.sharedMaterial;
            if (mat == null)
            {
                mat = new Material(Shader.Find("Custom/SlimeGlow") ?? Shader.Find("Standard"));
                rend.sharedMaterial = mat;
            }
            
            // Create instance and force properties
            mat = rend.material;
            ApplyLightGreenProperties(mat);
            
            Debug.Log($"<color=green>✓ PreInit fixed: {rend.gameObject.name}</color>");
        }
    }
    
    void ApplyLightGreenProperties(Material mat)
    {
        // FORCE light green on ALL color properties
        mat.SetColor("_Color", LIGHT_GREEN);
        mat.SetColor("_EmissionColor", LIGHT_GREEN * 3f);
        
        if (mat.HasProperty("_FresnelColor"))
            mat.SetColor("_FresnelColor", LIGHT_GREEN * 1.5f);
        
        if (mat.HasProperty("_SSSColor"))
            mat.SetColor("_SSSColor", LIGHT_GREEN * 1.2f);
        
        // Force emission ON
        mat.EnableKeyword("_EMISSION");
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        if (mat.HasProperty("_EmissionIntensity"))
            mat.SetFloat("_EmissionIntensity", 3f);
        
        // Transparency
        if (mat.HasProperty("_Alpha"))
            mat.SetFloat("_Alpha", 0.85f);
        
        // Physical properties
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.9f);
        
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0.1f);
    }
    
    IEnumerator ContinuousColorFix()
    {
        // Fix color every frame for 5 seconds
        float endTime = Time.time + 5f;
        int frameCount = 0;
        
        while (Time.time < endTime)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            
            foreach (Renderer rend in renderers)
            {
                if (rend.gameObject.name.Contains("Eye") || rend.gameObject.name.Contains("Pupil"))
                    continue;
                
                Material mat = rend.material;
                
                // Check if color is wrong
                Color currentColor = mat.GetColor("_Color");
                if (Vector4.Distance(currentColor, LIGHT_GREEN) > 0.1f)
                {
                    ApplyLightGreenProperties(mat);
                    Debug.LogWarning($"Frame {frameCount}: Corrected color on {rend.gameObject.name} from {currentColor} to {LIGHT_GREEN}");
                }
            }
            
            frameCount++;
            yield return null;
        }
        
        Debug.Log($"<color=lime>✓ PreInitColorFix: Monitored and corrected colors for {frameCount} frames</color>");
    }
}
