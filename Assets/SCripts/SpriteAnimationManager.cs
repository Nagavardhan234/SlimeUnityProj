using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Manages sprite sheet animations for slime emotions.
/// Dynamically loads sprites from Assets/itch/187x187/ folder.
/// Replaces simple particle system with professional sprite animations.
/// </summary>
public class SpriteAnimationManager : MonoBehaviour
{
    [Header("References")]
    public Transform slimeTransform;
    public Canvas canvas;
    public GameObject animationPlayerPrefab;
    
    [Header("Animation Settings")]
    public int poolSize = 15;
    public float defaultFrameRate = 24f;
    
    [Header("Debug")]
    public bool logLoadingInfo = true;
    
    // Animation data cache
    private Dictionary<SpriteAnimationPlayer.AnimationType, SpriteAnimationPlayer.AnimationData> animationCache;
    
    // Object pool
    private Queue<GameObject> objectPool;
    
    // Loading state
    private bool allAnimationsLoaded = false;
    
    // Sprite loading paths (Resources folder)
    private readonly Dictionary<SpriteAnimationPlayer.AnimationType, string> animationPaths = new Dictionary<SpriteAnimationPlayer.AnimationType, string>
    {
        { SpriteAnimationPlayer.AnimationType.TearDrop, "itch/187x187/tear_drop" },
        { SpriteAnimationPlayer.AnimationType.SweatDrop, "itch/187x187/sweat_drop" },
        { SpriteAnimationPlayer.AnimationType.MusicNotes, "itch/187x187/music_notes" },
        { SpriteAnimationPlayer.AnimationType.HeartLove, "itch/187x187/heart_love" },
        { SpriteAnimationPlayer.AnimationType.QuestionMarks, "itch/187x187/question_marks" },
        { SpriteAnimationPlayer.AnimationType.ExclamationMarks, "itch/187x187/exclamation_marks" },
        { SpriteAnimationPlayer.AnimationType.Sparkles, "itch/187x187/Sparkles" },
        { SpriteAnimationPlayer.AnimationType.BlushBubble, "itch/187x187/blush_bubble" },
        { SpriteAnimationPlayer.AnimationType.AngerSymbol, "itch/187x187/anger_symbol" },
        { SpriteAnimationPlayer.AnimationType.StressLines, "itch/187x187/stress_lines" },
        { SpriteAnimationPlayer.AnimationType.SpiritIcon, "itch/187x187/spirit_icon" },
        { SpriteAnimationPlayer.AnimationType.ShockRays, "itch/187x187/shock_rays" },
        { SpriteAnimationPlayer.AnimationType.AngryVeins, "itch/187x187/angry_veins" },
        { SpriteAnimationPlayer.AnimationType.FrustrationScribble, "itch/187x187/frustration_scribble" }
    };
    
    void Start()
    {
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        
        animationCache = new Dictionary<SpriteAnimationPlayer.AnimationType, SpriteAnimationPlayer.AnimationData>();
        objectPool = new Queue<GameObject>();
        
        InitializeObjectPool();
        StartCoroutine(LoadAllAnimations());
    }
    
    void InitializeObjectPool()
    {
        if (animationPlayerPrefab == null)
        {
            animationPlayerPrefab = CreateDefaultPlayerPrefab();
        }
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(animationPlayerPrefab, canvas.transform);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }
    
    GameObject CreateDefaultPlayerPrefab()
    {
        GameObject prefab = new GameObject("AnimationPlayer");
        RectTransform rt = prefab.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(187, 187);
        
        Image img = prefab.AddComponent<Image>();
        img.raycastTarget = false;
        
        prefab.AddComponent<SpriteAnimationPlayer>();
        
        return prefab;
    }
    
    IEnumerator LoadAllAnimations()
    {
        foreach (var kvp in animationPaths)
        {
            yield return StartCoroutine(LoadAnimation(kvp.Key, kvp.Value));
        }
        
        allAnimationsLoaded = true;
        
        if (logLoadingInfo)
        {
            Debug.Log($"SpriteAnimationManager: Loaded {animationCache.Count} animations successfully!");
        }
    }
    
    public bool IsLoaded()
    {
        return allAnimationsLoaded;
    }
    
    public bool IsAnimationLoaded(SpriteAnimationPlayer.AnimationType type)
    {
        return animationCache.ContainsKey(type);
    }
    
    IEnumerator LoadAnimation(SpriteAnimationPlayer.AnimationType type, string folderPath)
    {
        // Load all sprites dynamically from Resources folder
        List<Sprite> sprites = new List<Sprite>();
        int frameIndex = 0;
        
        while (true)
        {
            string spritePath = $"{folderPath}/Scene1_{frameIndex:D3}";
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            
            if (sprite == null)
            {
                break;
            }
            
            sprites.Add(sprite);
            frameIndex++;
            
            // Yield every 10 frames to avoid freezing
            if (frameIndex % 10 == 0)
            {
                yield return null;
            }
        }
        
        if (sprites.Count > 0)
        {
            SpriteAnimationPlayer.AnimationData animData = new SpriteAnimationPlayer.AnimationData
            {
                type = type,
                frames = sprites.ToArray(),
                frameCount = sprites.Count,
                frameRate = GetFrameRateForAnimation(type),
                loop = GetLoopSettingForAnimation(type)
            };
            
            animationCache[type] = animData;
            
            if (logLoadingInfo)
            {
                Debug.Log($"Loaded {type}: {sprites.Count} frames from {folderPath}");
            }
        }
        else
        {
            if (logLoadingInfo)
            {
                Debug.LogWarning($"No sprites found for {type} at {folderPath}");
            }
        }
    }
    
    float GetFrameRateForAnimation(SpriteAnimationPlayer.AnimationType type)
    {
        switch (type)
        {
            case SpriteAnimationPlayer.AnimationType.TearDrop:
            case SpriteAnimationPlayer.AnimationType.SweatDrop:
                return 20f;  // Slower for falling
            
            case SpriteAnimationPlayer.AnimationType.MusicNotes:
            case SpriteAnimationPlayer.AnimationType.HeartLove:
                return 24f;  // Standard kawaii bounce
            
            case SpriteAnimationPlayer.AnimationType.ExclamationMarks:
            case SpriteAnimationPlayer.AnimationType.QuestionMarks:
                return 30f;  // Snappy appearance
            
            case SpriteAnimationPlayer.AnimationType.Sparkles:
                return 28f;  // Fast sparkle
            
            case SpriteAnimationPlayer.AnimationType.BlushBubble:
                return 18f;  // Slow pulse
            
            case SpriteAnimationPlayer.AnimationType.AngerSymbol:
            case SpriteAnimationPlayer.AnimationType.AngryVeins:
                return 32f;  // Intense pulsing
            
            case SpriteAnimationPlayer.AnimationType.StressLines:
            case SpriteAnimationPlayer.AnimationType.FrustrationScribble:
                return 26f;  // Rapid stress
            
            case SpriteAnimationPlayer.AnimationType.SpiritIcon:
            case SpriteAnimationPlayer.AnimationType.ShockRays:
                return 24f;  // Standard
            
            default:
                return defaultFrameRate;
        }
    }
    
    bool GetLoopSettingForAnimation(SpriteAnimationPlayer.AnimationType type)
    {
        switch (type)
        {
            case SpriteAnimationPlayer.AnimationType.BlushBubble:
            case SpriteAnimationPlayer.AnimationType.StressLines:
            case SpriteAnimationPlayer.AnimationType.AngryVeins:
                return true;  // Continuous effects
            
            default:
                return false;  // One-shot animations
        }
    }
    
    public void PlayAnimation(SpriteAnimationPlayer.AnimationType type, Vector2 position, Vector2 velocity, float duration = 3f)
    {
        if (!IsAnimationLoaded(type))
        {
            // Silently skip if not loaded yet
            return;
        }
        
        GameObject obj = GetPooledObject();
        if (obj == null)
        {
            if (logLoadingInfo)
            {
                Debug.LogWarning("Object pool exhausted!");
            }
            return;
        }
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = position;
        
        SpriteAnimationPlayer player = obj.GetComponent<SpriteAnimationPlayer>();
        player.PlayAnimation(animationCache[type], velocity, duration, true);
        
        obj.SetActive(true);
    }
    
    GameObject GetPooledObject()
    {
        if (objectPool.Count > 0)
        {
            return objectPool.Dequeue();
        }
        
        // Create new if pool exhausted
        if (animationPlayerPrefab != null)
        {
            GameObject obj = Instantiate(animationPlayerPrefab, canvas.transform);
            return obj;
        }
        
        return null;
    }
    
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
    }
    
    // Convenience methods matching emotion needs
    public void SpawnTear(Vector2 position, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = position + new Vector2(Random.Range(-30f, 30f), Random.Range(80f, 120f));
            Vector2 vel = new Vector2(Random.Range(-30f, 30f), Random.Range(-50f, -100f));
            PlayAnimation(SpriteAnimationPlayer.AnimationType.TearDrop, pos, vel, Random.Range(2f, 3f));
        }
    }
    
    public void SpawnSweat(Vector2 position, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 pos = position + new Vector2(Random.Range(100f, 150f), Random.Range(80f, 120f));
            Vector2 vel = new Vector2(Random.Range(-10f, 10f), Random.Range(-100f, -150f));
            PlayAnimation(SpriteAnimationPlayer.AnimationType.SweatDrop, pos, vel, Random.Range(1.5f, 2.5f));
        }
    }
    
    public void SpawnMusicalNote(Vector2 position)
    {
        Vector2 pos = position + new Vector2(Random.Range(-80f, 80f), Random.Range(50f, 100f));
        Vector2 vel = new Vector2(Random.Range(-50f, 50f), Random.Range(80f, 150f));
        PlayAnimation(SpriteAnimationPlayer.AnimationType.MusicNotes, pos, vel, 2f);
    }
    
    public void SpawnHeart(Vector2 position)
    {
        Vector2 pos = position + new Vector2(Random.Range(-60f, 60f), Random.Range(100f, 150f));
        Vector2 vel = new Vector2(Random.Range(-40f, 40f), Random.Range(100f, 180f));
        PlayAnimation(SpriteAnimationPlayer.AnimationType.HeartLove, pos, vel, 2.5f);
    }
    
    public void SpawnQuestionMark(Vector2 position)
    {
        Vector2 pos = position + new Vector2(0, 220f);
        Vector2 vel = new Vector2(0, 40f);
        PlayAnimation(SpriteAnimationPlayer.AnimationType.QuestionMarks, pos, vel, 2f);
    }
    
    public void SpawnExclamation(Vector2 position)
    {
        Vector2 pos = position + new Vector2(0, 220f);
        Vector2 vel = new Vector2(0, 60f);
        PlayAnimation(SpriteAnimationPlayer.AnimationType.ExclamationMarks, pos, vel, 1.5f);
    }
    
    public void SpawnSparkles(Vector2 position, int count = 3)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float speed = Random.Range(100f, 200f);
            Vector2 vel = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
            Vector2 pos = position + Random.insideUnitCircle * 50f;
            PlayAnimation(SpriteAnimationPlayer.AnimationType.Sparkles, pos, vel, Random.Range(1f, 2f));
        }
    }
    
    public void SpawnBlushBubble(Vector2 position)
    {
        Vector2 pos = position + new Vector2(120f, 0);
        PlayAnimation(SpriteAnimationPlayer.AnimationType.BlushBubble, pos, Vector2.zero, 3f);
    }
    
    public void SpawnAngerSymbol(Vector2 position)
    {
        Vector2 pos = position + new Vector2(-100f, 180f);
        PlayAnimation(SpriteAnimationPlayer.AnimationType.AngerSymbol, pos, Vector2.zero, 2f);
    }
    
    public void SpawnStressLines(Vector2 position)
    {
        PlayAnimation(SpriteAnimationPlayer.AnimationType.StressLines, position, Vector2.zero, 3f);
    }
    
    public void SpawnAngryVeins(Vector2 position)
    {
        Vector2 pos = position + new Vector2(80f, 150f);
        PlayAnimation(SpriteAnimationPlayer.AnimationType.AngryVeins, pos, Vector2.zero, 2.5f);
    }
    
    public void SpawnSpiritIcon(Vector2 position)
    {
        Vector2 pos = position + new Vector2(0, 250f);
        Vector2 vel = new Vector2(0, 100f);
        PlayAnimation(SpriteAnimationPlayer.AnimationType.SpiritIcon, pos, vel, 3f);
    }
    
    public void ClearAllAnimations()
    {
        foreach (Transform child in canvas.transform)
        {
            SpriteAnimationPlayer player = child.GetComponent<SpriteAnimationPlayer>();
            if (player != null)
            {
                player.StopAnimation();
                child.gameObject.SetActive(false);
            }
        }
    }
}
