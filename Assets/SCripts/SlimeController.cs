using UnityEngine;
using UnityEngine.UI;

public class SlimeController : MonoBehaviour
{
    [Header("Scene References")]
    public Canvas canvas;
    
    [Header("Background Settings - Warm Cream (Psychology: Calm/Safe/Bonding)")]
    public Color backgroundTopColor = new Color(1.0f, 0.96f, 0.90f, 1f);    // #FFF5E6 Warm Cream
    public Color backgroundBottomColor = new Color(1.0f, 0.91f, 0.84f, 1f); // #FFE8D6 Soft Peach
    
    [Header("Slime 3D Volume Colors")]
    public Color slimeCoreColor = new Color(0.5f, 1.0f, 1.0f, 1f);      // Brighter cyan
    public Color slimeEdgeColor = new Color(0.35f, 0.8f, 1.0f, 1f);    // Softer edge
    public Color slimeRimColor = new Color(0.7f, 1f, 1f, 1f);          // Brighter rim
    
    [Header("Animation Settings")]
    public bool enableIdleAnimation = true;
    [Range(0.5f, 3f)]
    public float breathingSpeed = 1.2f;
    [Range(0f, 0.3f)]
    public float breathingAmount = 0.08f;
    
    public bool enableBlinking = true;
    [Range(2f, 8f)]
    public float blinkInterval = 4f;
    [Range(0.1f, 0.5f)]
    public float blinkDuration = 0.15f;
    
    [Header("Eye Expression Control")]
    [Range(0.5f, 1.5f)]
    public float eyeEmotiveness = 1.0f; // Eye size variation for emotions (1.0 = normal, 1.3 = excited, 0.7 = sad)
    
    [Header("Manual Animation")]
    [Range(0f, 1f)]
    public float manualSquish = 0f;
    [Range(-0.3f, 0.3f)]
    public float manualBounce = 0f;
    
    // Private references
    private GameObject backgroundImage;
    private GameObject slimeDisplay;
    private GameObject slimeQuad;
    private Camera slimeCamera;
    private RenderTexture slimeRenderTexture;
    private Material slimeMaterial;
    
    // Animation state
    private float timeAccum = 0f;
    private float nextBlinkTime;
    private float blinkTimer = 0f;
    private bool isBlinking = false;
    
    void Start()
    {
        SetupScene();
        nextBlinkTime = Random.Range(2f, blinkInterval);
    }
    
    void Update()
    {
        if (slimeMaterial == null) return;
        
        timeAccum += Time.deltaTime;
        
        // Update shader time for animated effects (sparkles, wobble, etc.)
        slimeMaterial.SetFloat("_Time", timeAccum);
        
        // Idle breathing animation + pulse
        if (enableIdleAnimation)
        {
            float breath = Mathf.Sin(timeAccum * breathingSpeed) * breathingAmount;
            slimeMaterial.SetFloat("_SquishAmount", breath * 0.5f);
            slimeMaterial.SetFloat("_BounceOffset", breath * 0.3f);
            
            // BREATHING PULSE - scale 0.98 to 1.03 (alive feeling, constrained to prevent clipping)
            float pulse = 1.005f + Mathf.Sin(timeAccum * 1.1f) * 0.025f;
            slimeMaterial.SetFloat("_BreathingPulse", pulse);
            
            // Add subtle rotation wobble to the 3D quad for extra life
            float wobbleAngle = Mathf.Sin(timeAccum * 0.8f) * 2f;
            slimeQuad.transform.rotation = Quaternion.Euler(0, 0, wobbleAngle);
        }
        else
        {
            slimeMaterial.SetFloat("_SquishAmount", manualSquish);
            slimeMaterial.SetFloat("_BounceOffset", manualBounce);
            slimeQuad.transform.rotation = Quaternion.identity;
        }
        
        // Blinking system
        if (enableBlinking)
        {
            if (!isBlinking && timeAccum >= nextBlinkTime)
            {
                isBlinking = true;
                blinkTimer = 0f;
            }
            
            if (isBlinking)
            {
                blinkTimer += Time.deltaTime;
                float blinkCurve = Mathf.Sin((blinkTimer / blinkDuration) * Mathf.PI);
                slimeMaterial.SetFloat("_BlinkAmount", blinkCurve);
                
                if (blinkTimer >= blinkDuration)
                {
                    isBlinking = false;
                    nextBlinkTime = timeAccum + Random.Range(blinkInterval * 0.5f, blinkInterval * 1.5f);
                    slimeMaterial.SetFloat("_BlinkAmount", 0f);
                }
            }
        }
        else
        {
            slimeMaterial.SetFloat("_BlinkAmount", 0f);
        }
        
        // Update eye emotiveness
        slimeMaterial.SetFloat("_EyeEmotiveness", eyeEmotiveness);
    }
    
    public void SetupScene()
    {
        SetupCanvas();
        CreateBackground();
        CreateSlime3D();
    }
    
    void SetupCanvas()
    {
        if (canvas == null)
        {
            GameObject canvasObj = GameObject.Find("Canvas");
            if (canvasObj != null)
            {
                canvas = canvasObj.GetComponent<Canvas>();
            }
            else
            {
                canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }
        
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
    }
    
    void CreateBackground()
    {
        backgroundImage = new GameObject("BackgroundImage");
        backgroundImage.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = backgroundImage.AddComponent<RectTransform>();
        RawImage rawImage = backgroundImage.AddComponent<RawImage>();
        
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        backgroundImage.transform.SetAsFirstSibling();
        
        Material bgMaterial = new Material(Shader.Find("Procedural/BackgroundProcedural"));
        bgMaterial.SetColor("_TopColor", backgroundTopColor);
        bgMaterial.SetColor("_BottomColor", backgroundBottomColor);
        bgMaterial.SetFloat("_BubbleCount", 0);     // DISABLED - static background for chat
        bgMaterial.SetFloat("_BubbleSpeed", 0f);
        bgMaterial.SetFloat("_BubbleGlow", 0f);
        
        rawImage.material = bgMaterial;
        rawImage.color = Color.white;
    }
    
    void CreateSlime3D()
    {
        // Create RenderTexture (optimized for mobile)
        slimeRenderTexture = new RenderTexture(1080, 1080, 16, RenderTextureFormat.ARGB32);
        slimeRenderTexture.name = "SlimeRenderTexture";
        slimeRenderTexture.antiAliasing = 4;
        
        // Create camera for rendering slime
        GameObject cameraObj = new GameObject("SlimeCamera");
        slimeCamera = cameraObj.AddComponent<Camera>();
        slimeCamera.orthographic = true;
        slimeCamera.orthographicSize = 1.5f;
        slimeCamera.transform.position = new Vector3(0, 0, -10);
        slimeCamera.transform.rotation = Quaternion.identity;
        slimeCamera.clearFlags = CameraClearFlags.SolidColor;
        slimeCamera.backgroundColor = new Color(0, 0, 0, 0);
        slimeCamera.targetTexture = slimeRenderTexture;
        slimeCamera.cullingMask = 1 << LayerMask.NameToLayer("Default");
        slimeCamera.depth = -10;
        
        // Create 3D slime quad (3.2 scale leaves margin for animations)
        slimeQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        slimeQuad.name = "SlimeQuad3D";
        slimeQuad.transform.position = new Vector3(0, 0, 0);
        slimeQuad.transform.localScale = new Vector3(3.2f, 3.2f, 1);
        
        Collider collider = slimeQuad.GetComponent<Collider>();
        if (collider != null) DestroyImmediate(collider);
        
        // Create ADDICTIVE VIRTUAL PET - MAGICAL SLIME (bright purple-pink psychology)
        slimeMaterial = new Material(Shader.Find("Procedural/SlimeMagicalJelly"));
        slimeMaterial.SetColor("_CoreColor", new Color(0.616f, 0.482f, 1.0f, 0.65f));   // #9D7BFF TRANSPARENT
        slimeMaterial.SetColor("_EdgeColor", new Color(0.416f, 0.22f, 1.0f, 0.72f));   // #6A38FF Semi-transparent
        slimeMaterial.SetColor("_RimColor", new Color(0.9f, 0.65f, 1.0f, 1f));         // Stronger pink-purple rim
        slimeMaterial.SetColor("_InnerGlowColor", new Color(1.0f, 0.75f, 1.0f, 1f));  // Bright pink-white glow
        slimeMaterial.SetFloat("_RimPower", 2.2f);
        slimeMaterial.SetFloat("_FresnelPower", 1.6f);  // Glass-like
        slimeMaterial.SetFloat("_Translucency", 0.8f);   // Transparent jelly
        slimeMaterial.SetFloat("_Shininess", 0.96f);  // Very shiny
        slimeMaterial.SetFloat("_SpecularPower", 65f);  // Sharp highlights
        slimeMaterial.SetFloat("_InnerGlowStrength", 3.0f);  // STRONG magical glow
        
        // CUTENESS FEATURES
        slimeMaterial.SetColor("_BlushColor", new Color(1.0f, 0.4f, 0.65f, 0.75f));  // Brighter pink blush
        slimeMaterial.SetFloat("_BlushSize", 0.14f);  // Slightly larger
        slimeMaterial.SetFloat("_HighlightIntensity", 1.4f);  // More shine
        
        // Shape variation - gentle baby-like movement
        slimeMaterial.SetFloat("_WobbleAmount", 0.018f);
        slimeMaterial.SetFloat("_WobbleSpeed", 2.0f);
        slimeMaterial.SetFloat("_BottomSquish", 0.2f);
        slimeMaterial.SetFloat("_BreathingPulse", 1.0f);  // Will animate
        
        // Eyes - LARGER for baby schema, deeper colors for emotional depth
        slimeMaterial.SetColor("_EyeColor", new Color(0.08f, 0.15f, 0.35f, 1f));
        slimeMaterial.SetColor("_PupilColor", new Color(0.01f, 0.03f, 0.15f, 1f));
        slimeMaterial.SetColor("_EyeShine", Color.white);
        slimeMaterial.SetFloat("_EyeEmotiveness", eyeEmotiveness);
        
        // Magical Sparkle Particles - MORE & BRIGHTER
        slimeMaterial.SetFloat("_ParticleCount", 12f);  // More sparkles
        slimeMaterial.SetFloat("_ParticleSpeed", 0.6f);
        slimeMaterial.SetFloat("_ParticleGlow", 2.0f);  // Much brighter
        slimeMaterial.SetFloat("_ParticleTwinkle", 1.5f);  // Flash animation
        
        slimeQuad.GetComponent<Renderer>().material = slimeMaterial;
        
        // Create UI display (centered, large)
        slimeDisplay = new GameObject("SlimeDisplay");
        slimeDisplay.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = slimeDisplay.AddComponent<RectTransform>();
        RawImage rawImage = slimeDisplay.AddComponent<RawImage>();
        
        // Center on screen, perfect mobile portrait size
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(700, 700);
        
        rawImage.texture = slimeRenderTexture;
        rawImage.color = Color.white;
        
        // Add drop shadow for depth separation (floats pet forward 3D-like)
        Shadow shadow = slimeDisplay.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.15f);    // Soft 15% black
        shadow.effectDistance = new Vector2(0, -12);       // 12px downward
        shadow.useGraphicAlpha = true;
    }
    
    // Public accessors for external control
    public Material GetSlimeMaterial() { return slimeMaterial; }
    public Transform GetSlimeTransform() { return slimeQuad != null ? slimeQuad.transform : null; }
    public void SetIdleAnimationEnabled(bool enabled) { enableIdleAnimation = enabled; }
    
    // Public methods for external control
    public void TriggerBounce(float intensity = 1f)
    {
        if (slimeMaterial != null)
        {
            StartCoroutine(BounceAnimation(intensity));
        }
    }
    
    public void SetEmotion(float emotiveness)
    {
        eyeEmotiveness = Mathf.Clamp(emotiveness, 0.5f, 1.5f);
        if (slimeMaterial != null)
        {
            slimeMaterial.SetFloat("_EyeEmotiveness", eyeEmotiveness);
        }
    }
    
    System.Collections.IEnumerator BounceAnimation(float intensity)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float squish = Mathf.Sin(t * Mathf.PI) * intensity;
            float bounce = Mathf.Sin(t * Mathf.PI * 2f) * 0.2f * intensity;
            
            slimeMaterial.SetFloat("_SquishAmount", squish);
            slimeMaterial.SetFloat("_BounceOffset", bounce);
            
            yield return null;
        }
        
        slimeMaterial.SetFloat("_SquishAmount", 0f);
        slimeMaterial.SetFloat("_BounceOffset", 0f);
    }
    
    void OnDestroy()
    {
        if (slimeRenderTexture != null)
        {
            // Detach from camera before destroying
            if (slimeCamera != null)
            {
                slimeCamera.targetTexture = null;
            }
            slimeRenderTexture.Release();
            DestroyImmediate(slimeRenderTexture);
        }
    }
}
