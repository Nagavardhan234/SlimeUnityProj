using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Utility to fix sprite import settings for emotion animations
/// Run via Tools > Fix Sprite Import Settings or automatically fixes on first load
/// </summary>
[InitializeOnLoad]
public class FixSpriteImportSettings : AssetPostprocessor
{
    private static readonly string markerPath = "Assets/Resources/itch/.sprites_fixed";
    
    // Auto-run on editor load if not already fixed
    static FixSpriteImportSettings()
    {
        if (!File.Exists(markerPath))
        {
            EditorApplication.delayCall += () =>
            {
                Debug.Log("Auto-fixing sprite import settings...");
                FixAllSprites();
            };
        }
    }
    
    [MenuItem("Tools/Fix Sprite Import Settings")]
    static void FixAllSpritesMenu()
    {
        FixAllSprites();
    }
    
    static void FixAllSprites()
    {
        string basePath = "Assets/Resources/itch/187x187";
        
        if (!Directory.Exists(basePath))
        {
            Debug.LogError($"Path not found: {basePath}");
            return;
        }

        int fixedCount = 0;
        string[] pngFiles = Directory.GetFiles(basePath, "*.png", SearchOption.AllDirectories);
        
        Debug.Log($"Found {pngFiles.Length} PNG files. Checking import settings...");
        
        foreach (string pngFile in pngFiles)
        {
            // Convert to Unity path
            string assetPath = pngFile.Replace("\\", "/");
            
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                // Check if already a sprite
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.maxTextureSize = 512;
                    importer.textureCompression = TextureImporterCompression.Compressed;
                    
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    fixedCount++;
                    
                    if (fixedCount % 50 == 0)
                    {
                        Debug.Log($"Progress: {fixedCount}/{pngFiles.Length} sprites fixed...");
                    }
                }
            }
        }
        
        // Create marker file so we don't run again
        if (fixedCount > 0)
        {
            File.WriteAllText(markerPath, "Sprites fixed on " + System.DateTime.Now);
            AssetDatabase.Refresh();
        }
        
        Debug.Log($"✓ Fixed {fixedCount} sprite import settings! Animations should now load properly.");
        
        if (fixedCount > 0)
        {
            EditorUtility.DisplayDialog("Success", 
                $"Fixed {fixedCount} sprite textures.\n\nSprite animations should now work!\n\nPlease wait for Unity to finish reimporting...", 
                "OK");
        }
        else
        {
            Debug.Log("All sprites already configured correctly!");
        }
    }
}
