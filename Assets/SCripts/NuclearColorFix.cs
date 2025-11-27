using UnityEngine;

/// <summary>
/// NUCLEAR OPTION: Completely replace material with fresh light green material
/// Use this if nothing else works - it destroys and recreates materials
/// </summary>
public class NuclearColorFix : MonoBehaviour
{
    [Header("Material Creation")]
    [SerializeField] private Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
    [SerializeField] private Shader targetShader;
    
    void Start()
    {
        // Find the shader
        if (targetShader == null)
        {
            targetShader = Shader.Find("Custom/SlimeGlow");
            if (targetShader == null)
            {
                targetShader = Shader.Find("Standard");
                Debug.LogWarning("Custom/SlimeGlow shader not found! Using Standard.");
            }
        }
        
        ReplaceMaterials();
        
        // Force again after Unity finishes initialization
        Invoke(nameof(ReplaceMaterials), 0.3f);
    }
    
    void ReplaceMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer rend in renderers)
        {
            // Skip eyes
            if (rend.gameObject.name.Contains("Eye") || rend.gameObject.name.Contains("Pupil"))
                continue;
            
            // CREATE COMPLETELY NEW MATERIAL
            Material newMat = new Material(targetShader);
            
            // Set light green properties
            newMat.name = "LightGreenSlime_Runtime";
            newMat.SetColor("_Color", lightGreen);
            
            // Enable and configure emission
            newMat.EnableKeyword("_EMISSION");
            newMat.SetColor("_EmissionColor", lightGreen * 3f);
            newMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            
            if (newMat.HasProperty("_EmissionIntensity"))
                newMat.SetFloat("_EmissionIntensity", 3f);
            
            // Transparency setup
            if (newMat.HasProperty("_Mode"))
                newMat.SetFloat("_Mode", 3); // Transparent
            
            if (newMat.HasProperty("_Alpha"))
                newMat.SetFloat("_Alpha", 0.85f);
            
            // Fresnel
            if (newMat.HasProperty("_FresnelColor"))
                newMat.SetColor("_FresnelColor", lightGreen * 1.5f);
            
            if (newMat.HasProperty("_FresnelPower"))
                newMat.SetFloat("_FresnelPower", 3f);
            
            if (newMat.HasProperty("_FresnelIntensity"))
                newMat.SetFloat("_FresnelIntensity", 1.5f);
            
            // SSS
            if (newMat.HasProperty("_SSSColor"))
                newMat.SetColor("_SSSColor", lightGreen * 1.2f);
            
            if (newMat.HasProperty("_SSSIntensity"))
                newMat.SetFloat("_SSSIntensity", 0.8f);
            
            // Physical properties
            if (newMat.HasProperty("_Smoothness"))
                newMat.SetFloat("_Smoothness", 0.9f);
            
            if (newMat.HasProperty("_Metallic"))
                newMat.SetFloat("_Metallic", 0.1f);
            
            // Transparency render settings
            newMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            newMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            newMat.SetInt("_ZWrite", 0);
            newMat.EnableKeyword("_ALPHABLEND_ON");
            newMat.renderQueue = 3000;
            
            // REPLACE the renderer's material
            rend.material = newMat;
            
            Debug.Log($"<color=cyan>NUCLEAR FIX: Replaced material on {rend.gameObject.name} with fresh light green material</color>");
        }
        
        Debug.Log($"<color=lime>☢ NUCLEAR COLOR FIX COMPLETE - ALL MATERIALS RECREATED AS LIGHT GREEN ☢</color>");
    }
}
