using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to create properly configured SlimeMaterial with correct shader and light green colors.
/// This ensures the base material asset has correct values before any runtime instantiation.
/// </summary>
public class MaterialSetup : AssetPostprocessor
{
    [MenuItem("Slime/Create Proper Slime Material")]
    public static void CreateSlimeMaterial()
    {
        // Find the shader
        Shader slimeShader = Shader.Find("Custom/SlimeGlow");
        if (slimeShader == null)
        {
            Debug.LogError("Custom/SlimeGlow shader not found! Make sure SlimeGlowShader.shader exists.");
            return;
        }
        
        // Create new material from shader
        Material slimeMat = new Material(slimeShader);
        
        // Set light green color for all properties
        Color lightGreen = new Color(0.5f, 1f, 0.8f, 1f);
        
        slimeMat.SetColor("_Color", lightGreen);
        slimeMat.SetColor("_EmissionColor", lightGreen * 3f);
        slimeMat.SetColor("_FresnelColor", lightGreen * 1.5f);
        slimeMat.SetColor("_SSSColor", lightGreen * 1.2f);
        
        slimeMat.SetFloat("_EmissionIntensity", 3f);
        slimeMat.SetFloat("_Smoothness", 0.9f);
        slimeMat.SetFloat("_Metallic", 0.1f);
        slimeMat.SetFloat("_Alpha", 0.85f);
        slimeMat.SetFloat("_FresnelPower", 3f);
        slimeMat.SetFloat("_FresnelIntensity", 1.5f);
        slimeMat.SetFloat("_SSSPower", 4f);
        slimeMat.SetFloat("_SSSIntensity", 0.8f);
        slimeMat.SetFloat("_NoiseScale", 3f);
        slimeMat.SetFloat("_NoiseIntensity", 0.15f);
        slimeMat.SetFloat("_NoiseSpeed", 0.3f);
        slimeMat.SetFloat("_RimAlpha", 0.12f);
        
        // Enable emission
        slimeMat.EnableKeyword("_EMISSION");
        slimeMat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        // Save to Assets/Materials folder
        string folderPath = "Assets/Materials";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        
        string assetPath = folderPath + "/SlimeMaterial.mat";
        
        // Check if material already exists
        Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (existingMat != null)
        {
            // Update existing material
            EditorUtility.CopySerialized(slimeMat, existingMat);
            EditorUtility.SetDirty(existingMat);
            Debug.Log($"<color=lime>✓ Updated existing SlimeMaterial at {assetPath}</color>");
        }
        else
        {
            // Create new material asset
            AssetDatabase.CreateAsset(slimeMat, assetPath);
            Debug.Log($"<color=lime>✓ Created new SlimeMaterial at {assetPath}</color>");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Select the material in project window
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        EditorGUIUtility.PingObject(Selection.activeObject);
        
        Debug.Log("<color=cyan>════════════════════════════════════</color>");
        Debug.Log("<color=lime>✓ SlimeMaterial created with light green (0.5, 1, 0.8)</color>");
        Debug.Log("<color=lime>✓ All shader properties configured</color>");
        Debug.Log("<color=lime>✓ Assign this to your Slime's MeshRenderer</color>");
        Debug.Log("<color=cyan>════════════════════════════════════</color>");
    }
}
