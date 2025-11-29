using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central animation controller managing slime emotions with layer-based property control.
/// Prevents conflicts between scripts by owning all animation state.
/// </summary>
public class SlimeAnimationManager : MonoBehaviour
{
    public enum EmotionType
    {
        // Body motion emotions
        Wobble, Shake, Pulse, Tiny, Tremble, Stretch, Squish, Float, Pop, Drip, Wave, Glow, Blink,
        // Full emotion expressions (body + eyes combined)
        Sad, Sleepy, Happy, Angry, Cry, Shy, Scared, Excited, Curious, Lonely, Playful, Embarrassed, Pain, Tired, Calm, Focus
    }
    
    public enum AnimationState
    {
        Idle,
        Emotion,
        Transitioning
    }
    
    [Header("References")]
    public SlimeController slimeController;
    public SpriteAnimationManager spriteAnimationManager;
    
    [Header("Current State")]
    public EmotionType currentEmotion = EmotionType.Pulse;
    public AnimationState state = AnimationState.Idle;
    [Range(0f, 1f)]
    public float transitionProgress = 0f;
    
    [Header("Settings")]
    public bool enableDebugKeys = true;
    public float defaultTransitionDuration = 0.5f;
    public bool autoReturnToIdle = false;
    public float emotionDuration = 5f;
    
    [Header("Layer Weights")]
    [Range(0f, 1f)] public float bodyLayerWeight = 1f;
    [Range(0f, 1f)] public float eyeLayerWeight = 1f;
    [Range(0f, 1f)] public float effectLayerWeight = 1f;
    
    // Private state
    private Material slimeMaterial;
    private Transform slimeTransform;
    private AnimationState previousState;
    private EmotionType previousEmotion;
    private float emotionTimer = 0f;
    private bool isTransitioning = false;
    
    // Animation curves for easing
    private AnimationCurve easeOutCubic;
    private AnimationCurve easeInQuint;
    private AnimationCurve elasticOut;
    
    // Property storage for blending
    private Dictionary<string, float> currentProperties = new Dictionary<string, float>();
    private Dictionary<string, float> targetProperties = new Dictionary<string, float>();
    
    void Start()
    {
        StartCoroutine(InitializeAfterSlime());
        InitializeAnimationCurves();
    }
    
    IEnumerator InitializeAfterSlime()
    {
        yield return null;
        
        if (slimeController == null)
        {
            slimeController = FindObjectOfType<SlimeController>();
        }
        
        if (spriteAnimationManager == null)
        {
            spriteAnimationManager = FindObjectOfType<SpriteAnimationManager>();
            if (spriteAnimationManager == null)
            {
                GameObject animObj = new GameObject("SpriteAnimationManager");
                spriteAnimationManager = animObj.AddComponent<SpriteAnimationManager>();
                spriteAnimationManager.slimeTransform = slimeTransform;
                spriteAnimationManager.canvas = FindObjectOfType<Canvas>();
            }
        }
        
        if (slimeController != null)
        {
            slimeMaterial = slimeController.GetSlimeMaterial();
            slimeTransform = slimeController.GetSlimeTransform();
            slimeController.SetIdleAnimationEnabled(false);
            
            if (slimeMaterial != null)
            {
                Debug.Log("SlimeAnimationManager: Initialized and took control!");
                InitializePropertyDefaults();
            }
        }
    }
    
    void InitializeAnimationCurves()
    {
        // Ease Out Cubic: Fast start, slow end (settling motions)
        easeOutCubic = new AnimationCurve(
            new Keyframe(0, 0, 0, 3),
            new Keyframe(1, 1, 0, 0)
        );
        
        // Ease In Quint: Slow start, fast end (anticipation)
        easeInQuint = new AnimationCurve(
            new Keyframe(0, 0, 0, 0),
            new Keyframe(1, 1, 5, 0)
        );
        
        // Elastic Out: Overshoot and bounce back
        elasticOut = AnimationCurve.EaseInOut(0, 0, 1, 1);
        for (int i = 0; i < 5; i++)
        {
            float t = 0.3f + i * 0.15f;
            float bounce = Mathf.Pow(0.7f, i) * (i % 2 == 0 ? 0.1f : -0.1f);
            elasticOut.AddKey(new Keyframe(t, 1f + bounce));
        }
    }
    
    void InitializePropertyDefaults()
    {
        currentProperties["_BreathingPulse"] = 1f;
        currentProperties["_SquishAmount"] = 0f;
        currentProperties["_BounceOffset"] = 0f;
        currentProperties["_WobbleAmount"] = 0.018f;
        currentProperties["_WobbleSpeed"] = 2f;
        currentProperties["_EyeEmotiveness"] = 1f;
        currentProperties["_BlinkAmount"] = 0f;
        currentProperties["_InnerGlowStrength"] = 3f;
        currentProperties["_EyeOffsetX"] = 0f;
        currentProperties["_EyeOffsetY"] = 0f;
        currentProperties["_TearAmount"] = 0f;
    }
    
    void Update()
    {
        if (slimeMaterial == null) return;
        
        // Debug controls
        if (enableDebugKeys)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CycleEmotion();
            }
        }
        
        // Update animation
        float deltaTime = Time.deltaTime;
        emotionTimer += deltaTime;
        
        // Auto return to idle
        if (autoReturnToIdle && state == AnimationState.Emotion && emotionTimer >= emotionDuration)
        {
            PlayEmotion(EmotionType.Pulse, 1f);
        }
        
        // Apply current emotion animation
        ApplyEmotionAnimation(currentEmotion, deltaTime);
        
        // Apply properties to material
        ApplyPropertiesToMaterial();
    }
    
    public void PlayEmotion(EmotionType emotion, float transitionDuration = -1f)
    {
        if (transitionDuration < 0) transitionDuration = defaultTransitionDuration;
        
        previousEmotion = currentEmotion;
        currentEmotion = emotion;
        state = AnimationState.Emotion;
        emotionTimer = 0f;
        
        Debug.Log($"Playing emotion: {emotion}");
        
        // Start transition if duration > 0
        if (transitionDuration > 0 && previousEmotion != currentEmotion)
        {
            StartCoroutine(TransitionToEmotion(previousEmotion, emotion, transitionDuration));
        }
    }
    
    IEnumerator TransitionToEmotion(EmotionType from, EmotionType to, float duration)
    {
        isTransitioning = true;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transitionProgress = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }
        
        transitionProgress = 1f;
        isTransitioning = false;
    }
    
    void CycleEmotion()
    {
        int nextIndex = ((int)currentEmotion + 1) % System.Enum.GetValues(typeof(EmotionType)).Length;
        PlayEmotion((EmotionType)nextIndex);
    }
    
    void ApplyEmotionAnimation(EmotionType emotion, float deltaTime)
    {
        float time = Time.time;
        
        // Reset transform to baseline before applying emotion
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = Vector3.zero;
            slimeTransform.localRotation = Quaternion.identity;
            slimeTransform.localScale = new Vector3(3.2f, 3.2f, 1f);
        }
        
        switch (emotion)
        {
            case EmotionType.Wobble:
                ApplyWobble(time);
                break;
            case EmotionType.Shake:
                ApplyShake(time);
                break;
            case EmotionType.Pulse:
                ApplyPulse(time);
                break;
            case EmotionType.Tiny:
                ApplyTiny(time);
                break;
            case EmotionType.Tremble:
                ApplyTremble(time);
                break;
            case EmotionType.Stretch:
                ApplyStretch(time);
                break;
            case EmotionType.Squish:
                ApplySquish(time);
                break;
            case EmotionType.Float:
                ApplyFloat(time);
                break;
            case EmotionType.Pop:
                ApplyPop(time);
                break;
            case EmotionType.Drip:
                ApplyDrip(time);
                break;
            case EmotionType.Wave:
                ApplyWave(time);
                break;
            case EmotionType.Glow:
                ApplyGlow(time);
                break;
            case EmotionType.Blink:
                ApplyBlinkEmotion(time);
                break;
            // Full emotions
            case EmotionType.Sad:
                ApplySad(time);
                break;
            case EmotionType.Sleepy:
                ApplySleepy(time);
                break;
            case EmotionType.Happy:
                ApplyHappy(time);
                break;
            case EmotionType.Angry:
                ApplyAngry(time);
                break;
            case EmotionType.Cry:
                ApplyCry(time);
                break;
            case EmotionType.Shy:
                ApplyShy(time);
                break;
            case EmotionType.Scared:
                ApplyScared(time);
                break;
            case EmotionType.Excited:
                ApplyExcited(time);
                break;
            case EmotionType.Curious:
                ApplyCurious(time);
                break;
            case EmotionType.Lonely:
                ApplyLonely(time);
                break;
            case EmotionType.Playful:
                ApplyPlayful(time);
                break;
            case EmotionType.Embarrassed:
                ApplyEmbarrassed(time);
                break;
            case EmotionType.Pain:
                ApplyPain(time);
                break;
            case EmotionType.Tired:
                ApplyTired(time);
                break;
            case EmotionType.Calm:
                ApplyCalm(time);
                break;
            case EmotionType.Focus:
                ApplyFocus(time);
                break;
        }
    }
    
    // === BODY MOTION ANIMATIONS ===
    
    void ApplyWobble(float time)
    {
        SetProperty("_WobbleAmount", 0.05f);
        SetProperty("_WobbleSpeed", 3f);
        if (slimeTransform != null)
        {
            float wobble = Mathf.Sin(time * 3f) * 5f;
            slimeTransform.localRotation = Quaternion.Euler(0, 0, wobble);
        }
    }
    
    void ApplyShake(float time)
    {
        if (slimeTransform != null)
        {
            float shakeX = Random.Range(-0.06f, 0.06f);
            float shakeY = Random.Range(-0.03f, 0.03f);
            slimeTransform.localPosition = new Vector3(shakeX, shakeY, 0);
        }
        SetProperty("_WobbleAmount", 0.08f);
        SetProperty("_WobbleSpeed", 15f);
    }
    
    void ApplyPulse(float time)
    {
        float pulse = 1f + Mathf.Sin(time * 3f) * 0.08f;
        SetProperty("_BreathingPulse", pulse);
        SetProperty("_WobbleAmount", 0.01f);
    }
    
    void ApplyTiny(float time)
    {
        SetProperty("_BreathingPulse", 0.7f);
        SetProperty("_EyeEmotiveness", 0.6f);
        SetProperty("_WobbleAmount", 0.005f);
    }
    
    void ApplyTremble(float time)
    {
        float tremble = Mathf.Sin(time * 40f) * 0.015f;
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(0, tremble, 0);
        }
        SetProperty("_WobbleAmount", 0.03f);
        SetProperty("_WobbleSpeed", 8f);
    }
    
    void ApplyStretch(float time)
    {
        float scaleX = Mathf.Lerp(0.9f, 1.15f, (Mathf.Sin(time * 2f) + 1f) / 2f);
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(3.2f * scaleX, 3.2f, 1);
        }
        SetProperty("_WobbleAmount", 0.02f);
    }
    
    void ApplySquish(float time)
    {
        float squish = Mathf.Lerp(0f, 0.6f, (Mathf.Sin(time * 3f) + 1f) / 2f);
        SetProperty("_SquishAmount", squish);
        SetProperty("_BounceOffset", Mathf.Sin(time * 3f) * 0.1f);
    }
    
    void ApplyFloat(float time)
    {
        float yOffset = Mathf.Sin(time * 1f) * 0.25f;  // 3x amplitude fix
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(0, yOffset, 0);
        }
        SetProperty("_WobbleAmount", 0.025f);
        SetProperty("_WobbleSpeed", 1.5f);
        SetProperty("_ShadowIntensity", 0.15f + Mathf.Sin(time * 1f) * -0.1f);  // Shadow pulses with height
    }
    
    void ApplyPop(float time)
    {
        // Kawaii 3-phase timing: anticipate → explode → settle
        float phase = (time * 0.5f) % 1f;
        float pop;
        
        if (phase < 0.1f)  // Anticipation
        {
            pop = Mathf.Lerp(1f, 0.92f, phase / 0.1f);
            SetProperty("_EyeEmotiveness", 0.9f);
        }
        else if (phase < 0.15f)  // POP!
        {
            float t = (phase - 0.1f) / 0.05f;
            pop = Mathf.Lerp(0.92f, 1.28f, t);
            SetProperty("_EyeEmotiveness", 1.5f);  // Eyes widen
        }
        else  // Settle with damped oscillation
        {
            float t = (phase - 0.15f) / 0.85f;
            pop = 1f + 0.28f * Mathf.Exp(-5f * t) * Mathf.Cos(12f * t);
            float eyeSettle = 1.2f * Mathf.Exp(-3f * t);
            SetProperty("_EyeEmotiveness", Mathf.Max(1f, eyeSettle));
        }
        
        SetProperty("_BreathingPulse", Mathf.Min(pop, 1.1f));  // Cap to prevent clipping
        SetProperty("_WobbleAmount", 0.04f);
        SetProperty("_WobbleSpeed", 6f);
    }
    
    void ApplyDrip(float time)
    {
        // Use top squish instead of bottom to avoid clipping
        float drip = Mathf.Max(0, Mathf.Sin(time * 0.7f)) * 0.25f;
        SetProperty("_TopSquish", drip);
        SetProperty("_SquishAmount", drip * 0.3f);
        SetProperty("_BottomSquish", 0.3f);  // Base sitting position
    }
    
    void ApplyWave(float time)
    {
        float wave = Mathf.Sin(time * 4f) * 8f;
        if (slimeTransform != null)
        {
            slimeTransform.localRotation = Quaternion.Euler(0, 0, wave);
        }
        SetProperty("_WobbleAmount", 0.06f);
        SetProperty("_WobbleSpeed", 4f);
    }
    
    void ApplyGlow(float time)
    {
        // Reduced base to avoid HDR clamping
        float glow = 2.5f + Mathf.Sin(time * 2f) * 1.2f;
        SetProperty("_InnerGlowStrength", glow);
        SetProperty("_ParticleGlow", glow * 0.6f);
        SetProperty("_HighlightIntensity", 1.4f + Mathf.Sin(time * 3f) * 0.4f);
    }
    
    void ApplyBlinkEmotion(float time)
    {
        float blink = Mathf.Abs(Mathf.Sin(time * 2f));
        SetProperty("_BlinkAmount", 1f - blink);
        SetProperty("_WobbleAmount", 0.015f);
    }
    
    // === FULL EMOTION EXPRESSIONS ===
    
    void ApplySad(float time)
    {
        // Body: melting, small
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(2.72f, 2.72f, 1f);  // 0.85x
        }
        SetProperty("_BottomSquish", 0.4f);
        SetProperty("_BreathingPulse", 1f + Mathf.Sin(time * 0.5f) * 0.02f);  // Slow shallow breathing
        
        // Eyes: drooping, small, tears
        SetProperty("_EyeEmotiveness", 0.75f);
        SetProperty("_EyeOffsetY", -0.05f);
        SetProperty("_TearAmount", 0.3f);
        
        // Effects: dim
        SetProperty("_InnerGlowStrength", 1.5f);
        SetProperty("_ParticleGlow", 0.5f);
        
        // Sprite Animation: occasional tears
        if (spriteAnimationManager != null && Random.value < 0.05f)
        {
            spriteAnimationManager.SpawnTear(Vector2.zero, 1);
        }
    }
    
    void ApplySleepy(float time)
    {
        // Body: slow deep breathing, resting
        SetProperty("_BreathingPulse", 1f + Mathf.Sin(time * 0.3f) * 0.05f);
        SetProperty("_BottomSquish", 0.35f);
        
        // Eyes: half-closed, squinted
        SetProperty("_BlinkAmount", 0.7f);
        SetProperty("_EyeSquintAmount", 0.6f);
        SetProperty("_EyeEmotiveness", 0.85f);
        
        // Sprite Animation: Z's floating upward
        if (spriteAnimationManager != null && Random.value < 0.08f)
        {
            spriteAnimationManager.SpawnSpiritIcon(Vector2.zero);
        }
    }
    
    void ApplyHappy(float time)
    {
        // Body: bouncy pulse
        float bounce = Mathf.Sin(time * 2f);
        SetProperty("_BreathingPulse", 1f + Mathf.Abs(bounce) * 0.08f);
        SetProperty("_WobbleAmount", 0.08f);
        
        // Eyes: big, bright
        SetProperty("_EyeEmotiveness", 1.35f);
        
        // Effects: maximum sparkle
        SetProperty("_ParticleGlow", 3.5f);
        SetProperty("_InnerGlowStrength", 3.5f);
        
        // Sprite Animation: musical notes
        if (spriteAnimationManager != null && Random.value < 0.06f)
        {
            spriteAnimationManager.SpawnMusicalNote(Vector2.zero);
        }
    }
    
    void ApplyAngry(float time)
    {
        // Body: tense, shaking
        float shake = Random.Range(-0.02f, 0.02f);
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(shake, shake * 0.5f, 0);
        }
        SetProperty("_BreathingPulse", 1f + Mathf.Sin(time * 3f) * 0.02f);  // Rapid tense
        SetProperty("_BottomSquish", 0.1f);  // Firmly planted
        
        // Eyes: narrowed, intense
        SetProperty("_EyeEmotiveness", 0.85f);
        SetProperty("_PupilScale", 0.7f);
        SetProperty("_EyeOffsetY", 0.02f);
        
        // Effects: red shift, dark
        SetProperty("_ColorShift", 30f);  // Toward red
        
        // Sprite Animation: anger symbol, angry veins, stress lines
        if (spriteAnimationManager != null && Random.value < 0.06f)
        {
            spriteAnimationManager.SpawnAngerSymbol(Vector2.zero);
        }
        if (spriteAnimationManager != null && Random.value < 0.05f)
        {
            spriteAnimationManager.SpawnAngryVeins(Vector2.zero);
        }
        if (spriteAnimationManager != null && Random.value < 0.04f)
        {
            spriteAnimationManager.SpawnStressLines(Vector2.zero);
        }
    }
    
    void ApplyCry(float time)
    {
        // Body: collapsed, trembling
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(2.4f, 2.4f, 1f);  // 0.75x crumpled
        }
        SetProperty("_BottomSquish", 0.5f);
        
        // Irregular breathing (sobbing)
        float sob = Mathf.PerlinNoise(time * 2f, 0) * 0.3f;
        SetProperty("_BreathingPulse", 0.8f + sob);
        
        // Eyes: scrunched, lots of tears
        SetProperty("_EyeEmotiveness", 0.6f);
        SetProperty("_EyeSquintAmount", 0.7f);
        SetProperty("_TearAmount", 1f);
        
        // Effects: very dim
        SetProperty("_InnerGlowStrength", 1f);
        
        // Sprite Animation: heavy tears
        if (spriteAnimationManager != null && Random.value < 0.15f)
        {
            spriteAnimationManager.SpawnTear(Vector2.zero, 2);
        }
    }
    
    void ApplyShy(float time)
    {
        // Body: shrinking, leaning back
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(2.816f, 2.816f, 1f);  // 0.88x
            slimeTransform.localRotation = Quaternion.Euler(0, 0, -8f);  // Lean back
        }
        SetProperty("_BottomSquish", 0.25f);
        SetProperty("_BreathingPulse", 1f + Mathf.Sin(time * 2f) * 0.03f);  // Rapid shallow
        
        // Eyes: looking away, small
        SetProperty("_EyeOffsetX", 0.15f);
        SetProperty("_EyeOffsetY", 0.02f);
        SetProperty("_EyeEmotiveness", 0.9f);
        
        // Effects: big blush pulsing
        float blushPulse = Mathf.PingPong(time * 3f, 0.14f) + 0.08f;
        SetProperty("_BlushSize", blushPulse);
        
        // Sprite Animation: occasional sweat drop + blush bubble
        if (spriteAnimationManager != null && Random.value < 0.04f)
        {
            spriteAnimationManager.SpawnSweat(Vector2.zero, 1);
        }
        if (spriteAnimationManager != null && Random.value < 0.02f)
        {
            spriteAnimationManager.SpawnBlushBubble(Vector2.zero);
        }
    }
    
    void ApplyScared(float time)
    {
        // Body: cowering, intense shake
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(2.24f, 2.24f, 1f);  // 0.7x
            float shake = Mathf.Sin(time * 12f) * 0.03f;
            slimeTransform.localPosition = new Vector3(shake, shake * 0.7f, 0);
        }
        SetProperty("_BottomSquish", 0.15f);
        
        // Erratic breathing
        float panic = Mathf.PerlinNoise(time * 3f, 1f);
        SetProperty("_BreathingPulse", 0.95f + panic * 0.13f);
        
        // Eyes: WIDE with fear
        SetProperty("_EyeEmotiveness", 1.5f);
        SetProperty("_PupilScale", 1.3f);  // Dilated
        
        // Looking around rapidly
        float lookX = Mathf.Sin(time * 5f) * 0.1f;
        SetProperty("_EyeOffsetX", lookX);
        
        // Sprite Animation: exclamation marks and sweat
        if (spriteAnimationManager != null && Random.value < 0.03f)
        {
            spriteAnimationManager.SpawnExclamation(Vector2.zero);
        }
        if (spriteAnimationManager != null && Random.value < 0.05f)
        {
            spriteAnimationManager.SpawnSweat(Vector2.zero, 1);
        }
    }
    
    void ApplyExcited(float time)
    {
        // Body: bouncing wildly
        float bounce = Mathf.Abs(Mathf.Sin(time * 3f));
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(0, bounce * 0.25f, 0);
        }
        SetProperty("_BreathingPulse", 0.9f + bounce * 0.3f);
        SetProperty("_WobbleAmount", 0.12f);
        
        // Rotation wobble
        if (slimeTransform != null)
        {
            float wobble = Mathf.Sin(time * 4f) * 12f;
            slimeTransform.localRotation = Quaternion.Euler(0, 0, wobble);
        }
        
        // Eyes: HUGE
        SetProperty("_EyeEmotiveness", 1.6f);
        
        // Effects: maximum
        SetProperty("_InnerGlowStrength", 4.5f);
        SetProperty("_ParticleGlow", 3.8f);
        
        // Sprite Animation: hearts, musical notes, and sparkles
        if (spriteAnimationManager != null && Random.value < 0.1f)
        {
            spriteAnimationManager.SpawnHeart(Vector2.zero);
        }
        if (spriteAnimationManager != null && Random.value < 0.08f)
        {
            spriteAnimationManager.SpawnMusicalNote(Vector2.zero);
        }
        if (spriteAnimationManager != null && Random.value < 0.12f)
        {
            spriteAnimationManager.SpawnSparkles(Vector2.zero, 2);
        }
    }
    
    void ApplyCurious(float time)
    {
        // Body: leaning forward
        if (slimeTransform != null)
        {
            slimeTransform.localRotation = Quaternion.Euler(0, 0, 5f);
        }
        SetProperty("_BreathingPulse", 1f + Mathf.Sin(time * 1.2f) * 0.03f);
        
        // Eyes: one wider (asymmetric)
        SetProperty("_EyeEmotiveness", 1.15f);
        
        // Looking around slowly
        float lookX = Mathf.Sin(time * 0.8f) * 0.08f;
        float lookY = Mathf.Cos(time * 0.6f) * 0.05f;
        SetProperty("_EyeOffsetX", lookX);
        SetProperty("_EyeOffsetY", lookY);
        
        // Sprite Animation: question mark
        if (spriteAnimationManager != null && Random.value < 0.03f)
        {
            spriteAnimationManager.SpawnQuestionMark(Vector2.zero);
        }
    }
    
    void ApplyLonely(float time)
    {
        // Body: deflated, sagging
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(2.624f, 2.624f, 1f);  // 0.82x
        }
        SetProperty("_BottomSquish", 0.45f);
        
        // Irregular slow breathing (sighing)
        float sigh = Mathf.PerlinNoise(time * 0.3f, 2f);
        SetProperty("_BreathingPulse", 1f + sigh * 0.05f);
        
        // Eyes: small, looking down
        SetProperty("_EyeEmotiveness", 0.7f);
        SetProperty("_EyeOffsetY", -0.08f);
        
        // Effects: very dim
        SetProperty("_InnerGlowStrength", 1.2f);
        SetProperty("_ParticleGlow", 0.3f);
    }
    
    void ApplyPlayful(float time)
    {
        // Body: random hops
        float hop = Mathf.Max(0, Mathf.Sin(time * 1.5f + Mathf.PerlinNoise(time * 0.3f, 3f) * 5f));
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(0, hop * 0.2f, 0);
        }
        SetProperty("_WobbleAmount", 0.09f);
        
        // Random rotations
        float turn = Mathf.PerlinNoise(time * 0.5f, 4f) * 30f - 15f;
        if (slimeTransform != null)
        {
            slimeTransform.localRotation = Quaternion.Euler(0, 0, turn);
        }
        
        // Eyes: varied expressions (winking)
        float wink = Mathf.Floor(Mathf.PingPong(time * 2f, 2f));
        SetProperty("_EyeEmotiveness", 1.1f + wink * 0.2f);
        
        // Effects: colorful
        SetProperty("_ParticleGlow", 3f);
        
        // Sprite Animation: musical notes and hearts
        if (spriteAnimationManager != null && Random.value < 0.07f)
        {
            spriteAnimationManager.SpawnMusicalNote(Vector2.zero);
        }
        if (spriteAnimationManager != null && Random.value < 0.04f)
        {
            spriteAnimationManager.SpawnHeart(Vector2.zero);
        }
    }
    
    void ApplyEmbarrassed(float time)
    {
        // Body: shrinking away
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(2.4f, 2.4f, 1f);  // 0.75x wants to disappear
            slimeTransform.localRotation = Quaternion.Euler(0, 0, -12f);  // Turning away
        }
        SetProperty("_BottomSquish", 0.5f);  // Melting from shame
        
        // Nervous wobble
        SetProperty("_WobbleAmount", 0.05f);
        SetProperty("_WobbleSpeed", 4f);
        
        // Eyes: squinted, looking away
        SetProperty("_EyeEmotiveness", 0.6f);
        SetProperty("_EyeOffsetX", 0.2f);
        SetProperty("_EyeOffsetY", -0.1f);
        
        // Effects: MASSIVE blush
        float blush = 0.25f + Mathf.PingPong(time * 4f, 0.05f);
        SetProperty("_BlushSize", blush);
        
        // Sprite Animation: sweat drops and blush bubble
        if (spriteAnimationManager != null && Random.value < 0.08f)
        {
            spriteAnimationManager.SpawnSweat(Vector2.zero, 1);
        }
        if (spriteAnimationManager != null && Random.value < 0.03f)
        {
            spriteAnimationManager.SpawnBlushBubble(Vector2.zero);
        }
    }
    
    void ApplyPain(float time)
    {
        // Body: asymmetric, wounded
        SetProperty("_AsymmetryAmount", 0.3f);
        
        // Strained breathing
        SetProperty("_BreathingPulse", 0.98f + Mathf.Sin(time * 2f) * 0.06f);
        
        // Periodic wince
        float wince = Mathf.Max(0, Mathf.Sin(time * 1.3f));
        if (slimeTransform != null)
        {
            float scale = 1f - wince * 0.15f;
            slimeTransform.localScale = new Vector3(3.2f * scale, 3.2f * scale, 1f);
        }
        
        // Eyes: squinted, pained
        SetProperty("_EyeSquintAmount", 0.8f);
        SetProperty("_EyeEmotiveness", 0.7f);
        SetProperty("_TearAmount", 0.5f);
        
        // Effects: weak
        SetProperty("_InnerGlowStrength", 1.5f);
        
        // Sprite Animation: pain tears
        if (spriteAnimationManager != null && Random.value < 0.06f)
        {
            spriteAnimationManager.SpawnTear(Vector2.zero, 1);
        }
    }
    
    void ApplyTired(float time)
    {
        // Body: resting, slow breathing
        SetProperty("_BottomSquish", 0.4f);
        SetProperty("_BreathingPulse", 0.95f + Mathf.Sin(time * 0.4f) * 0.06f);
        
        // Swaying (about to collapse)
        float sway = Mathf.Sin(time * 0.3f) * 4f;
        if (slimeTransform != null)
        {
            slimeTransform.localRotation = Quaternion.Euler(0, 0, sway);
            slimeTransform.localScale = new Vector3(2.944f, 2.944f, 1f);  // 0.92x
        }
        
        // Eyes: heavy lids, half-closed
        SetProperty("_EyeSquintAmount", 0.7f);
        SetProperty("_BlinkAmount", 0.5f);  // Baseline half-closed
        SetProperty("_EyeOffsetY", -0.06f);  // Drooping
        
        // Effects: reduced
        SetProperty("_ParticleGlow", 0.8f);
        
        // Sprite Animation: Z's (less frequent than Sleepy)
        if (spriteAnimationManager != null && Random.value < 0.04f)
        {
            spriteAnimationManager.SpawnSpiritIcon(Vector2.zero);
        }
    }
    
    void ApplyCalm(float time)
    {
        // Body: slow regular breathing, seated comfortably
        SetProperty("_BreathingPulse", 0.99f + Mathf.Sin(time * 0.7f) * 0.03f);
        SetProperty("_BottomSquish", 0.3f);
        SetProperty("_WobbleAmount", 0.01f);  // Minimal, stable
        
        // Eyes: relaxed, gentle
        SetProperty("_EyeEmotiveness", 0.95f);
        SetProperty("_EyeSquintAmount", 0.2f);  // Gentle
        
        // Effects: soft warm glow
        SetProperty("_InnerGlowStrength", 2.5f);
        SetProperty("_ColorShift", 10f);  // +5% yellow warmth
    }
    
    void ApplyFocus(float time)
    {
        // Body: minimal movement, firm base
        SetProperty("_BreathingPulse", 1f + Mathf.Sin(time * 1.5f) * 0.01f);
        SetProperty("_BottomSquish", 0.15f);
        SetProperty("_WobbleAmount", 0.005f);  // Very steady
        
        // Slight forward lean
        if (slimeTransform != null)
        {
            slimeTransform.localRotation = Quaternion.Euler(0, 0, 3f);
        }
        
        // Eyes: alert, locked gaze
        SetProperty("_EyeEmotiveness", 1.15f);
        SetProperty("_PupilScale", 0.8f);  // Concentrated
        
        // Effects: sharp clarity
        SetProperty("_HighlightIntensity", 2f);
        SetProperty("_InnerGlowStrength", 3.5f);
    }
    
    // === UTILITY METHODS ===
    
    void SetProperty(string propertyName, float value)
    {
        currentProperties[propertyName] = value;
    }
    
    void ApplyPropertiesToMaterial()
    {
        if (slimeMaterial == null) return;
        
        foreach (var prop in currentProperties)
        {
            slimeMaterial.SetFloat(prop.Key, prop.Value);
        }
    }
}
