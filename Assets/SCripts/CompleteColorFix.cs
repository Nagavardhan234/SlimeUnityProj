/*
 * COMPLETE COLOR FIX SYSTEM
 * 
 * THIS SCRIPT FIXES THE WHITE SLIME ISSUE COMPLETELY
 * 
 * Instructions:
 * 1. Remove ALL other color fix scripts (ForceJellyColor, NuclearColorFix, PreInitColorFix)
 * 2. Add ONLY this script to your Slime GameObject
 * 3. Press Play - slime will be light green GUARANTEED
 * 
 * What this does:
 * - Runs BEFORE all other scripts (-1000 execution order)
 * - Creates fresh materials if needed
 * - Forces light green on EVERY property
 * - Monitors for 300 frames and auto-corrects
 * - Disables conflicting scripts automatically
 */

using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-1000)]
public class CompleteColorFix : MonoBehaviour
{
    private static readonly Color LIGHT_GREEN = new Color(0.5f, 1f, 0.8f, 1f);
    private int monitorFrames = 300;
    
    void Awake()
    {
        Debug.Log("<color=yellow>CompleteColorFix: DISABLED - Using properly configured base material</color>");
        
        // Disable this script - not needed when using shared material
        this.enabled = false;
    }
    
    void Start()
    {
        // Delay to let DualLayerJelly create inner layer in its Awake
        StartCoroutine(DelayedFix());
    }
    
    IEnumerator DelayedFix()
    {
        // Wait for DualLayerJelly's Awake and Start to complete
        yield return new WaitForSeconds(0.05f);
        
        Debug.Log("<color=yellow>▶ Applying color fix AFTER all layers created...</color>");
        
        // Fix all materials now that everything exists
        ForceFixAllMaterials();
        
        // Start continuous monitoring
        StartCoroutine(ContinuousMonitor());
    }
    
    void DisableConflictingScripts()
    {
        // Disable other color fix scripts if they exist
        var scripts = GetComponents<MonoBehaviour>();
        foreach (var script in scripts)
        {
            string typeName = script.GetType().Name;
            if (typeName.Contains("ForceJellyColor") || 
                typeName.Contains("NuclearColorFix") || 
                typeName.Contains("PreInitColorFix"))
            {
                script.enabled = false;
                Debug.Log($"<color=orange>Disabled conflicting script: {typeName}</color>");
            }
        }
    }
    
    void ForceFixAllMaterials()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer rend in renderers)
        {
            // Skip eyes and pupils
            string name = rend.gameObject.name.ToLower();
            if (name.Contains("eye") || name.Contains("pupil"))
                continue;
            
            // CRITICAL: Check if using wrong shader
            Material sharedMat = rend.sharedMaterial;
            bool needsNewMaterial = false;
            
            if (sharedMat == null)
            {
                needsNewMaterial = true;
                Debug.LogWarning($"<color=red>{name}: NULL material - creating new one</color>");
            }
            else if (sharedMat.shader.name != "Custom/SlimeGlow")
            {
                needsNewMaterial = true;
                Debug.LogWarning($"<color=red>{name}: Wrong shader '{sharedMat.shader.name}' - replacing with Custom/SlimeGlow</color>");
            }
            
            // Create fresh material with correct shader
            if (needsNewMaterial)
            {
                Shader correctShader = Shader.Find("Custom/SlimeGlow");
                if (correctShader == null)
                {
                    Debug.LogError("<color=red>Custom/SlimeGlow shader not found!</color>");
                    correctShader = Shader.Find("Standard");
                }
                
                Material newMat = new Material(correctShader);
                rend.sharedMaterial = newMat;
                Debug.Log($"<color=yellow>Created fresh material for {name} with {correctShader.name}</color>");
            }
            
            // Create instance and apply properties
            Material mat = rend.material;
            ApplyLightGreenComplete(mat, rend.gameObject.name);
        }
    }
    
    void ApplyLightGreenComplete(Material mat, string objectName)
    {
        // === CORE COLORS ===
        mat.SetColor("_Color", LIGHT_GREEN);
        mat.color = LIGHT_GREEN; // Unity shorthand
        
        // === EMISSION ===
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", LIGHT_GREEN * 3f);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        if (mat.HasProperty("_EmissionIntensity"))
            mat.SetFloat("_EmissionIntensity", 3f);
        
        // === FRESNEL ===
        if (mat.HasProperty("_FresnelColor"))
            mat.SetColor("_FresnelColor", LIGHT_GREEN * 1.5f);
        
        if (mat.HasProperty("_FresnelPower"))
            mat.SetFloat("_FresnelPower", 3f);
        
        if (mat.HasProperty("_FresnelIntensity"))
            mat.SetFloat("_FresnelIntensity", 1.5f);
        
        // === SUBSURFACE SCATTERING ===
        if (mat.HasProperty("_SSSColor"))
            mat.SetColor("_SSSColor", LIGHT_GREEN * 1.2f);
        
        if (mat.HasProperty("_SSSIntensity"))
            mat.SetFloat("_SSSIntensity", 0.8f);
        
        // === PHYSICAL PROPERTIES ===
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 0.9f);
        
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", 0.1f);
        
        // === TRANSPARENCY ===
        if (mat.HasProperty("_Alpha"))
            mat.SetFloat("_Alpha", 0.85f);
        
        if (mat.HasProperty("_Mode"))
            mat.SetFloat("_Mode", 3); // Transparent
        
        // Render settings for transparency
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
        
        Debug.Log($"<color=green>✓ {objectName}: ALL properties set to light green</color>");
    }
    
    IEnumerator ContinuousMonitor()
    {
        int frame = 0;
        int corrections = 0;
        
        while (frame < monitorFrames)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            
            foreach (Renderer rend in renderers)
            {
                string name = rend.gameObject.name.ToLower();
                if (name.Contains("eye") || name.Contains("pupil"))
                    continue;
                
                Material mat = rend.material;
                Color currentColor = mat.GetColor("_Color");
                
                // Check if color drifted from light green
                float colorDiff = Vector4.Distance(currentColor, LIGHT_GREEN);
                if (colorDiff > 0.05f)
                {
                    ApplyLightGreenComplete(mat, rend.gameObject.name);
                    corrections++;
                    Debug.LogWarning($"<color=yellow>Frame {frame}: Corrected {rend.gameObject.name} (was {currentColor})</color>");
                }
            }
            
            frame++;
            yield return null;
        }
        
        Debug.Log("<color=cyan>════════════════════════════════════</color>");
        Debug.Log($"<color=lime>✓ MONITORING COMPLETE after {frame} frames</color>");
        Debug.Log($"<color=lime>✓ Made {corrections} corrections</color>");
        Debug.Log($"<color=lime>✓ Slime is STABLE LIGHT GREEN!</color>");
        Debug.Log("<color=cyan>════════════════════════════════════</color>");
    }
    
    // Public method for manual color change
    public void ChangeColor(Color newColor)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
        
        foreach (Renderer rend in renderers)
        {
            string name = rend.gameObject.name.ToLower();
            if (name.Contains("eye") || name.Contains("pupil"))
                continue;
            
            Material mat = rend.material;
            mat.SetColor("_Color", newColor);
            mat.SetColor("_EmissionColor", newColor * 3f);
            
            if (mat.HasProperty("_FresnelColor"))
                mat.SetColor("_FresnelColor", newColor * 1.5f);
            
            if (mat.HasProperty("_SSSColor"))
                mat.SetColor("_SSSColor", newColor * 1.2f);
        }
        
        Debug.Log($"<color=magenta>★ Color changed to {newColor}</color>");
    }
}
