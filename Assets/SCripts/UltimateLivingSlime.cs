using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ultimate Living Slime - Professional biologically-inspired animation system.
/// Implements multi-layer breathing, emotional intelligence, personality traits,
/// realistic eye behavior, and emergent lifelike characteristics.
/// </summary>
public class UltimateLivingSlime : MonoBehaviour
{
    #region Emotion System
    
    /// <summary>
    /// Four-dimensional emotional model (Russell's Circumplex extended)
    /// </summary>
    [System.Serializable]
    public class EmotionalState
    {
        [Range(-1f, 1f)] public float valence = 0f;      // Negative ← → Positive
        [Range(0f, 1f)] public float arousal = 0.5f;     // Calm → Excited
        [Range(0f, 1f)] public float dominance = 0.5f;   // Submissive → Assertive
        [Range(0f, 1f)] public float engagement = 0.5f;  // Withdrawn → Curious
        
        [Range(0f, 1f)] public float intensity = 0.5f;   // Emotion strength
        
        public EmotionalState(float v, float a, float d, float e, float i = 0.5f)
        {
            valence = v; arousal = a; dominance = d; engagement = e; intensity = i;
        }
        
        public EmotionalState Clone()
        {
            return new EmotionalState(valence, arousal, dominance, engagement, intensity);
        }
    }
    
    /// <summary>
    /// Named emotion presets for easy testing
    /// </summary>
    public enum EmotionPreset
    {
        Neutral,
        Happy,
        Sad,
        Angry,
        Scared,
        Excited,
        Tired,
        Curious,
        Shy,
        Playful,
        Content,
        Lonely,
        Embarrassed,
        Pensive,
        Hopeful,
        Worried,
        // Natural pet states
        Drowsy,
        Sleeping,
        Hungry,
        Affectionate,
        Grumpy,
        Surprised
    }
    
    #endregion
    
    #region Personality System
    
    [System.Serializable]
    public class PersonalityTraits
    {
        [Range(0f, 1f)] public float extroversion = 0.5f;    // Social energy
        [Range(0f, 1f)] public float sensitivity = 0.5f;     // Emotional reactivity
        [Range(0f, 1f)] public float curiosity = 0.5f;       // Exploration drive
        [Range(0f, 1f)] public float affection = 0.5f;       // Warmth toward user
        [Range(0f, 1f)] public float energyLevel = 0.5f;     // Baseline activity
    }
    
    #endregion
    
    #region Inspector Settings
    
    [Header("References")]
    public SlimeController slimeController;
    public SpriteAnimationManager spriteAnimationManager;
    
    [Header("Current Emotional State")]
    public EmotionalState currentEmotion = new EmotionalState(0, 0.5f, 0.5f, 0.5f, 0.3f);
    public EmotionalState targetEmotion = new EmotionalState(0, 0.5f, 0.5f, 0.5f, 0.3f);
    
    [Header("Testing Controls")]
    public EmotionPreset testEmotionPreset = EmotionPreset.Neutral;
    [Tooltip("Minimum duration an emotion must play before changing")]
    public float minimumEmotionDuration = 3f;
    [Tooltip("Transition speed between emotions")]
    public float emotionTransitionSpeed = 1f;
    public bool lockEmotionDuringDuration = true;
    
    [Header("Personality")]
    public PersonalityTraits personality = new PersonalityTraits();
    
    [Header("Biological Systems")]
    [Range(0f, 1f)] public float currentEnergy = 1f;
    [Range(0.5f, 2f)] public float breathRate = 1f;
    public bool enableIdleMicroAnimations = true;
    
    [Header("Needs System (Virtual Pet)")]
    [Range(0f, 1f)] public float attentionMeter = 1f;  // Decreases when ignored
    [Range(0f, 1f)] public float happinessMeter = 0.8f; // Increases with good interactions
    [Range(0f, 1f)] public float hungerMeter = 1f;      // Decreases over time
    public float lastInteractionTime = 0f;
    
    [Header("Debug Display")]
    public string currentEmotionName = "Neutral";
    public float emotionTimer = 0f;
    [HideInInspector] public float emotionLockTimer = 0f;
    [HideInInspector] public bool canChangeEmotion = true;
    
    #endregion
    
    #region Private State
    
    private Material slimeMaterial;
    private Transform slimeTransform;
    
    // Breathing system
    [HideInInspector] public float breathPhase = 0f;           // 0-1 through breath cycle (public for debug)
    private float breathVariation = 0f;       // Perlin noise for irregularity
    private float nextBreathVariationTime = 0f;
    
    // Eye system
    private Vector2 gazeTarget = Vector2.zero;
    private float nextSaccadeTime = 0f;
    private float blinkTimer = 0f;
    private float nextBlinkTime = 2f;
    private bool isBlinking = false;
    private float blinkPhase = 0f;
    
    // Idle micro-animations
    private float nextMicroShiftTime = 0f;
    private float microShiftTarget = 0f;
    private float nextEyeDartTime = 0f;
    
    // Emotion transition
    private EmotionPreset previousEmotionPreset = EmotionPreset.Neutral;
    private bool isTransitioning = false;
    
    // Energy system
    private float energyDepletionRate = 0.05f;  // Per second at intensity 1.0
    private float energyRecoveryRate = 0.02f;   // Per second when calm
    
    // Residual effects (post-emotion lingering)
    private float tearPuffiness = 0f;           // After crying
    private float excitementResidue = 0f;       // After excited
    private float tensionResidue = 0f;          // After angry
    
    // Natural visual effects (shader-based, no sprites)
    private float eyeWetness = 0f;              // Tear film glossiness
    private float eyeRedness = 0f;              // Redness from crying
    private float blushIntensity = 0f;          // Natural cheek blush
    private float internalHeartGlow = 0f;       // Love/affection indicator
    private float sweatingAmount = 0f;          // Anxiety wetness
    private float sleepDepth = 0f;              // 0=awake, 1=deep sleep
    
    // Animation history for emergence
    private Queue<EmotionalState> emotionHistory = new Queue<EmotionalState>();
    private const int maxHistorySize = 20;
    
    #endregion
    
    void Start()
    {
        StartCoroutine(InitializeAfterSlime());
    }
    
    IEnumerator InitializeAfterSlime()
    {
        yield return null;

        // Get references
        if (slimeController == null)
        {
            slimeController = FindObjectOfType<SlimeController>();
        }
        
        if (slimeController != null)
        {
            slimeMaterial = slimeController.GetSlimeMaterial();
            slimeTransform = slimeController.GetSlimeTransform();
            slimeController.SetIdleAnimationEnabled(false);
            
            if (slimeMaterial != null)
            {
                Debug.Log("UltimateLivingSlime: Initialized successfully! Use dropdown to test emotions.");
                InitializeDefaultState();
            }
        }
        else
        {
            Debug.LogError("UltimateLivingSlime: SlimeController not found!");
        }
        
        if (spriteAnimationManager == null)
        {
            spriteAnimationManager = FindObjectOfType<SpriteAnimationManager>();
            if (spriteAnimationManager == null)
            {
                GameObject animObj = new GameObject("SpriteAnimationManager");
                spriteAnimationManager = animObj.AddComponent<SpriteAnimationManager>();
                spriteAnimationManager.canvas = FindObjectOfType<Canvas>();
            }
        }
        
        // Set slime transform reference for sprite manager
        if (spriteAnimationManager != null && slimeTransform != null)
        {
            spriteAnimationManager.slimeTransform = slimeTransform;
        }
    }
    
    void InitializeDefaultState()
    {
        // Set neutral starting state
        currentEmotion = GetEmotionPresetValues(EmotionPreset.Neutral);
        targetEmotion = currentEmotion.Clone();
        
        // Initialize timers
        nextBlinkTime = Random.Range(2f, 4f);
        nextSaccadeTime = Random.Range(0.5f, 2f);
        nextMicroShiftTime = Random.Range(8f, 12f);
        nextEyeDartTime = Random.Range(2f, 5f);
        
        // Reset residuals
        tearPuffiness = 0f;
        excitementResidue = 0f;
        tensionResidue = 0f;
        
        // Initialize needs
        lastInteractionTime = Time.time;
    }
    
    void Update()
    {
        if (slimeMaterial == null || slimeTransform == null) return;
        
        // Emotion testing control
        HandleEmotionTesting();
        
        // Core update loops
        UpdateEmotionTransition();
        UpdateBreathingSystem();
        UpdateEyeSystem();
        UpdateIdleMicroAnimations();
        UpdateEnergySystem();
        UpdateResidualEffects();
        
        // Update needs system
        UpdateNeedsSystem();
        
        // Apply all systems to slime
        ApplyEmotionalStateToSlime();
        ApplyBreathingToSlime();
        ApplyEyeStateToSlime();
        ApplyBodyMotionToSlime();  // NEW: Dramatic body animations
        ApplyBodyLanguageToSlime();
        ApplyNaturalVisualEffects(); // NEW: Organic shader-based effects
        
        // Spawn synchronized sprite animations
        // UpdateSpriteAnimations(); // DISABLED - Replaced with natural shader effects
        
        // Track history for emergence
        TrackEmotionHistory();
    }
    
    #region Emotion Testing & Transition
    
    void HandleEmotionTesting()
    {
        // Check if preset changed in inspector
        if (testEmotionPreset != previousEmotionPreset)
        {
            if (canChangeEmotion || !lockEmotionDuringDuration)
            {
                SetEmotionPreset(testEmotionPreset);
                previousEmotionPreset = testEmotionPreset;
            }
            else
            {
                // Revert dropdown if locked
                testEmotionPreset = previousEmotionPreset;
                Debug.Log($"Emotion locked for {emotionLockTimer:F1}s more. Current: {currentEmotionName}");
            }
        }
        
        // Update emotion lock timer
        if (emotionLockTimer > 0f)
        {
            emotionLockTimer -= Time.deltaTime;
            if (emotionLockTimer <= 0f)
            {
                canChangeEmotion = true;
            }
        }
        
        emotionTimer += Time.deltaTime;
        canChangeEmotion = emotionLockTimer <= 0f;
    }
    
    public void SetEmotionPreset(EmotionPreset preset)
    {
        targetEmotion = GetEmotionPresetValues(preset);
        currentEmotionName = preset.ToString();
        isTransitioning = true;
        
        // Lock emotion for minimum duration
        if (lockEmotionDuringDuration)
        {
            emotionLockTimer = minimumEmotionDuration;
            canChangeEmotion = false;
        }
        
        emotionTimer = 0f;
        
        Debug.Log($"Transitioning to: {preset} (locked for {minimumEmotionDuration}s)");
    }
    
    EmotionalState GetEmotionPresetValues(EmotionPreset preset)
    {
        // Define emotion coordinates in 4D space + intensity
        switch (preset)
        {
            case EmotionPreset.Happy:
                return new EmotionalState(0.8f, 0.6f, 0.6f, 0.7f, 0.7f);
            case EmotionPreset.Sad:
                return new EmotionalState(-0.7f, 0.3f, 0.3f, 0.2f, 0.6f);
            case EmotionPreset.Angry:
                return new EmotionalState(-0.6f, 0.9f, 0.9f, 0.8f, 0.8f);
            case EmotionPreset.Scared:
                return new EmotionalState(-0.8f, 0.95f, 0.1f, 0.7f, 0.9f);
            case EmotionPreset.Excited:
                return new EmotionalState(0.9f, 1f, 0.7f, 0.9f, 0.9f);
            case EmotionPreset.Tired:
                return new EmotionalState(-0.2f, 0.1f, 0.3f, 0.2f, 0.5f);
            case EmotionPreset.Curious:
                return new EmotionalState(0.3f, 0.6f, 0.5f, 0.95f, 0.6f);
            case EmotionPreset.Shy:
                return new EmotionalState(0.1f, 0.4f, 0.2f, 0.4f, 0.6f);
            case EmotionPreset.Playful:
                return new EmotionalState(0.7f, 0.75f, 0.6f, 0.85f, 0.7f);
            case EmotionPreset.Content:
                return new EmotionalState(0.5f, 0.3f, 0.5f, 0.4f, 0.4f);
            case EmotionPreset.Lonely:
                return new EmotionalState(-0.5f, 0.25f, 0.3f, 0.3f, 0.6f);
            case EmotionPreset.Embarrassed:
                return new EmotionalState(-0.3f, 0.6f, 0.15f, 0.5f, 0.75f);
            case EmotionPreset.Pensive:
                return new EmotionalState(0.0f, 0.35f, 0.5f, 0.6f, 0.4f);
            case EmotionPreset.Hopeful:
                return new EmotionalState(0.4f, 0.5f, 0.45f, 0.7f, 0.5f);
            case EmotionPreset.Worried:
                return new EmotionalState(-0.4f, 0.65f, 0.35f, 0.65f, 0.65f);
            
            // Natural pet states
            case EmotionPreset.Drowsy:
                return new EmotionalState(-0.1f, 0.15f, 0.4f, 0.2f, 0.4f);
            case EmotionPreset.Sleeping:
                return new EmotionalState(0.2f, 0.05f, 0.3f, 0.1f, 0.3f);
            case EmotionPreset.Hungry:
                return new EmotionalState(-0.3f, 0.5f, 0.4f, 0.8f, 0.7f);
            case EmotionPreset.Affectionate:
                return new EmotionalState(0.9f, 0.4f, 0.5f, 0.9f, 0.8f);
            case EmotionPreset.Grumpy:
                return new EmotionalState(-0.4f, 0.3f, 0.6f, 0.3f, 0.5f);
            case EmotionPreset.Surprised:
                return new EmotionalState(0.1f, 0.95f, 0.3f, 1f, 0.9f);
            
            default: // Neutral
                return new EmotionalState(0f, 0.5f, 0.5f, 0.5f, 0.3f);
        }
    }
    
    void UpdateEmotionTransition()
    {
        if (!isTransitioning) return;
        
        // Smooth interpolation to target emotion
        float transitionSpeed = emotionTransitionSpeed * Time.deltaTime;
        
        currentEmotion.valence = Mathf.Lerp(currentEmotion.valence, targetEmotion.valence, transitionSpeed);
        currentEmotion.arousal = Mathf.Lerp(currentEmotion.arousal, targetEmotion.arousal, transitionSpeed);
        currentEmotion.dominance = Mathf.Lerp(currentEmotion.dominance, targetEmotion.dominance, transitionSpeed);
        currentEmotion.engagement = Mathf.Lerp(currentEmotion.engagement, targetEmotion.engagement, transitionSpeed);
        currentEmotion.intensity = Mathf.Lerp(currentEmotion.intensity, targetEmotion.intensity, transitionSpeed);
        
        // Check if transition complete
        float distance = Vector4.Distance(
            new Vector4(currentEmotion.valence, currentEmotion.arousal, currentEmotion.dominance, currentEmotion.engagement),
            new Vector4(targetEmotion.valence, targetEmotion.arousal, targetEmotion.dominance, targetEmotion.engagement)
        );
        
        if (distance < 0.05f)
        {
            isTransitioning = false;
        }
    }
    
    #endregion
    
    #region Dramatic Body Motion System
    
    void ApplyBodyMotionToSlime()
    {
        float time = Time.time;
        
        // Reset transform to baseline first (prevent conflicts)
        slimeTransform.localPosition = Vector3.zero;
        slimeTransform.localRotation = Quaternion.identity;
        slimeTransform.localScale = new Vector3(3.2f, 3.2f, 1f);
        
        // Map emotional state to body motions (with INCREASED amplitudes for visibility)
        // High arousal + positive valence = bouncing/pulsing
        if (currentEmotion.arousal > 0.75f && currentEmotion.valence > 0.6f)
        {
            ApplyBouncePulse(time, currentEmotion.intensity);
        }
        // High arousal + negative valence + high dominance = shake (angry)
        else if (currentEmotion.arousal > 0.8f && currentEmotion.valence < -0.4f && currentEmotion.dominance > 0.7f)
        {
            ApplyAngryShake(time, currentEmotion.intensity);
        }
        // High arousal + low dominance = tremble (scared)
        else if (currentEmotion.arousal > 0.8f && currentEmotion.dominance < 0.3f)
        {
            ApplyScaredTremble(time, currentEmotion.intensity);
        }
        // Low arousal + negative valence = drip/sag (sad/tired)
        else if (currentEmotion.arousal < 0.4f && currentEmotion.valence < -0.3f)
        {
            ApplyDripSag(time, currentEmotion.intensity);
        }
        // Mid arousal + high engagement = curious stretch
        else if (currentEmotion.engagement > 0.8f)
        {
            ApplyCuriousStretch(time, currentEmotion.intensity);
        }
        // Default: subtle wobble based on arousal
        else
        {
            ApplyBaseWobble(time, currentEmotion.arousal, currentEmotion.intensity);
        }
    }
    
    void ApplyBouncePulse(float time, float intensity)
    {
        // Excited bouncing with squish on landing (INCREASED amplitude 3x)
        float bounceFreq = 2f + intensity;
        float bouncePhase = (time * bounceFreq) % 1f;
        
        float verticalOffset = 0f;
        float scaleY = 1f;
        float scaleX = 1f;
        
        if (bouncePhase < 0.2f)  // Rising
        {
            float t = bouncePhase / 0.2f;
            verticalOffset = Mathf.Sin(t * Mathf.PI * 0.5f) * 0.4f * intensity;  // 0.4 units up
            scaleY = 1f + t * 0.15f * intensity;  // Stretch
            scaleX = 1f - t * 0.1f * intensity;   // Narrow
        }
        else if (bouncePhase < 0.4f)  // Falling
        {
            float t = (bouncePhase - 0.2f) / 0.2f;
            verticalOffset = Mathf.Cos(t * Mathf.PI * 0.5f) * 0.4f * intensity;
            scaleY = 1.15f - t * 0.15f * intensity;
            scaleX = 0.9f + t * 0.1f * intensity;
        }
        else if (bouncePhase < 0.5f)  // Squish on landing
        {
            float t = (bouncePhase - 0.4f) / 0.1f;
            scaleY = 1f - t * 0.3f * intensity;  // Squish down
            scaleX = 1f + t * 0.3f * intensity;  // Widen
        }
        else  // Recover
        {
            float t = (bouncePhase - 0.5f) / 0.5f;
            scaleY = 0.7f + t * 0.3f;
            scaleX = 1.3f - t * 0.3f;
        }
        
        // Apply dramatic transform changes
        slimeTransform.localPosition = Vector3.up * verticalOffset;
        slimeTransform.localScale = new Vector3(3.2f * scaleX, 3.2f * scaleY, 1f);
        
        // Shader wobble
        slimeMaterial.SetFloat("_WobbleAmount", 0.08f * intensity);
        slimeMaterial.SetFloat("_WobbleSpeed", 5f + intensity * 3f);
    }
    
    void ApplyAngryShake(float time, float intensity)
    {
        // Violent shaking with rotation (INCREASED amplitude 5x)
        float shakeX = Random.Range(-0.15f, 0.15f) * intensity;
        float shakeY = Random.Range(-0.08f, 0.08f) * intensity;
        float shakeRot = Random.Range(-8f, 8f) * intensity;
        
        slimeTransform.localPosition = new Vector3(shakeX, shakeY, 0);
        slimeTransform.localRotation = Quaternion.Euler(0, 0, shakeRot);
        
        // Aggressive wobble
        slimeMaterial.SetFloat("_WobbleAmount", 0.15f * intensity);
        slimeMaterial.SetFloat("_WobbleSpeed", 18f);
    }
    
    void ApplyScaredTremble(float time, float intensity)
    {
        // Rapid trembling (INCREASED frequency and amplitude)
        float trembleFreq = 45f;
        float trembleX = Mathf.Sin(time * trembleFreq) * 0.04f * intensity;
        float trembleY = Mathf.Sin(time * trembleFreq * 1.3f) * 0.03f * intensity;
        
        slimeTransform.localPosition = new Vector3(trembleX, trembleY, 0);
        
        // Rapid micro-wobble
        slimeMaterial.SetFloat("_WobbleAmount", 0.06f * intensity);
        slimeMaterial.SetFloat("_WobbleSpeed", 12f);
    }
    
    void ApplyDripSag(float time, float intensity)
    {
        // Slow dripping motion with top squish (INCREASED amplitude)
        float dripPhase = (time * 0.6f) % 1f;
        float topSquish = Mathf.Max(0, Mathf.Sin(dripPhase * Mathf.PI)) * 0.35f * intensity;
        
        slimeMaterial.SetFloat("_TopSquish", topSquish);
        slimeMaterial.SetFloat("_BottomSquish", 0.25f);
        
        // Slight downward sag in scale
        float sagScale = 1f - 0.1f * intensity;
        slimeTransform.localScale = new Vector3(3.2f, 3.2f * sagScale, 1f);
    }
    
    void ApplyCuriousStretch(float time, float intensity)
    {
        // Upward stretching motion (INCREASED amplitude)
        float stretchPhase = (time * 1.5f) % 1f;
        float stretch = Mathf.Sin(stretchPhase * Mathf.PI) * 0.3f * intensity;
        
        float scaleY = 1f + stretch;
        float scaleX = 1f - stretch * 0.5f;  // Narrow when stretching
        
        slimeTransform.localScale = new Vector3(3.2f * scaleX, 3.2f * scaleY, 1f);
        slimeTransform.localPosition = Vector3.up * stretch * 0.15f;
        
        // Lean forward
        slimeTransform.localRotation = Quaternion.Euler(0, 0, 3f * intensity);
    }
    
    void ApplyBaseWobble(float time, float arousal, float intensity)
    {
        // Rhythmic wobble based on arousal (INCREASED amplitude)
        float wobbleFreq = Mathf.Lerp(1.5f, 4f, arousal);
        float wobbleAngle = Mathf.Sin(time * wobbleFreq) * Mathf.Lerp(3f, 12f, arousal) * intensity;
        
        slimeTransform.localRotation = Quaternion.Euler(0, 0, wobbleAngle);
        
        // Shader wobble
        float wobbleAmount = Mathf.Lerp(0.02f, 0.1f, arousal) * intensity;
        slimeMaterial.SetFloat("_WobbleAmount", wobbleAmount);
        slimeMaterial.SetFloat("_WobbleSpeed", wobbleFreq * 2f);
    }
    
    #endregion
    
    #region Multi-Layer Breathing System
    
    void UpdateBreathingSystem()
    {
        // Breath rate influenced by arousal (excited = faster, tired = slower)
        float emotionalBreathModifier = Mathf.Lerp(0.5f, 2f, currentEmotion.arousal);
        breathRate = emotionalBreathModifier * Mathf.Lerp(0.8f, 1.2f, personality.energyLevel);
        
        // Add irregular variation (sighs, pauses)
        if (Time.time > nextBreathVariationTime)
        {
            breathVariation = Random.Range(-0.2f, 0.3f); // Occasional deep breath or pause
            nextBreathVariationTime = Time.time + Random.Range(8f, 15f);
        }
        
        // Update breath phase (0-1 cycle)
        breathPhase += Time.deltaTime * breathRate * (1f + breathVariation);
        breathPhase %= 1f;
    }
    
    void ApplyBreathingToSlime()
    {
        // Multi-layer breathing with cascading delays
        float breathCycle = breathPhase * Mathf.PI * 2f;
        
        // Primary breath (diaphragm) - leads the motion
        float diaphragmExpansion = Mathf.Sin(breathCycle) * 0.03f * currentEmotion.intensity;
        
        // Secondary breath (mid-body) - follows with 0.15s delay
        float midBodyPhase = (breathPhase - 0.15f) % 1f;
        float midBodyExpansion = Mathf.Sin(midBodyPhase * Mathf.PI * 2f) * 0.02f * currentEmotion.intensity;
        
        // Tertiary breath (shoulders) - follows with additional 0.1s delay
        float shoulderPhase = (breathPhase - 0.25f) % 1f;
        float shoulderRise = Mathf.Sin(shoulderPhase * Mathf.PI * 2f) * 0.01f * currentEmotion.intensity;
        
        // Combined breathing pulse
        float totalBreathPulse = 1f + diaphragmExpansion + midBodyExpansion + shoulderRise;
        
        // Apply personality modulation
        totalBreathPulse *= Mathf.Lerp(0.98f, 1.02f, personality.energyLevel);
        
        // Set breathing pulse with intensity scaling
        slimeMaterial.SetFloat("_BreathingPulse", totalBreathPulse);
        
        // Bottom squish for diaphragm movement (belly breathing)
        float bellySquish = 0.15f + Mathf.Max(0, Mathf.Sin(breathCycle)) * 0.1f * (1f - currentEmotion.arousal);
        slimeMaterial.SetFloat("_BottomSquish", bellySquish);
    }
    
    #endregion
    
    #region Realistic Eye System
    
    void UpdateEyeSystem()
    {
        // Saccadic eye movement (jump between fixation points)
        if (Time.time > nextSaccadeTime && !isBlinking)
        {
            // New gaze target influenced by emotional state
            float gazeRange = Mathf.Lerp(0.05f, 0.15f, currentEmotion.engagement);
            gazeTarget = new Vector2(
                Random.Range(-gazeRange, gazeRange),
                Random.Range(-gazeRange, gazeRange)
            );
            
            // Saccade timing influenced by engagement
            float saccadeInterval = Mathf.Lerp(3f, 0.5f, currentEmotion.engagement);
            nextSaccadeTime = Time.time + Random.Range(saccadeInterval * 0.8f, saccadeInterval * 1.2f);
        }
        
        // Autonomous blink system
        UpdateBlinkSystem();
        
        // Pupil dilation based on arousal
        float pupilSize = Mathf.Lerp(0.7f, 1.3f, currentEmotion.arousal);
        slimeMaterial.SetFloat("_PupilScale", pupilSize);
    }
    
    void UpdateBlinkSystem()
    {
        if (isBlinking)
        {
            // Blink animation
            blinkPhase += Time.deltaTime * 10f; // 0.1s blink duration
            
            if (blinkPhase >= 1f)
            {
                isBlinking = false;
                blinkPhase = 0f;
                
                // Schedule next blink (Poisson distribution)
                float blinkInterval = Mathf.Lerp(4f, 2f, currentEmotion.arousal);
                nextBlinkTime = Time.time + Random.Range(blinkInterval * 0.7f, blinkInterval * 1.3f);
            }
        }
        else
        {
            // Check if time to blink
            if (Time.time > nextBlinkTime)
            {
                // Synchronize blinks with exhale phase (biological realism)
                if (breathPhase > 0.5f && breathPhase < 0.7f)
                {
                    isBlinking = true;
                    blinkPhase = 0f;
                }
            }
        }
    }
    
    void ApplyEyeStateToSlime()
    {
        // Apply gaze offset
        slimeMaterial.SetFloat("_EyeOffsetX", gazeTarget.x);
        slimeMaterial.SetFloat("_EyeOffsetY", gazeTarget.y);
        
        // Apply blink
        if (isBlinking)
        {
            // Smooth ease-in-out blink curve
            float blinkCurve = Mathf.Sin(blinkPhase * Mathf.PI);
            slimeMaterial.SetFloat("_BlinkAmount", blinkCurve);
        }
        else
        {
            // Add tear puffiness after crying
            float baseEyeOpen = 1f - tearPuffiness * 0.3f;
            slimeMaterial.SetFloat("_BlinkAmount", 1f - baseEyeOpen);
        }
        
        // Eye size based on emotion (fear = wide, sad = small)
        float eyeSize = 1f;
        if (currentEmotion.valence < -0.5f && currentEmotion.arousal < 0.4f) // Sad
        {
            eyeSize = 0.75f;
        }
        else if (currentEmotion.arousal > 0.8f && currentEmotion.dominance < 0.3f) // Scared
        {
            eyeSize = 1.5f;
        }
        else if (currentEmotion.arousal > 0.8f && currentEmotion.valence > 0.7f) // Excited
        {
            eyeSize = 1.4f;
        }
        
        eyeSize *= Mathf.Lerp(0.95f, 1.05f, personality.affection);
        slimeMaterial.SetFloat("_EyeEmotiveness", eyeSize);
        
        // Eye squint for intensity
        float squintAmount = 0f;
        if (currentEmotion.dominance > 0.7f) // Angry
        {
            squintAmount = 0.6f * currentEmotion.intensity;
        }
        else if (currentEmotion.dominance < 0.3f && currentEmotion.arousal > 0.5f) // Shy/Embarrassed
        {
            squintAmount = 0.4f * currentEmotion.intensity;
        }
        
        slimeMaterial.SetFloat("_EyeSquintAmount", squintAmount);
    }
    
    #endregion
    
    #region Idle Micro-Animations
    
    void UpdateIdleMicroAnimations()
    {
        if (!enableIdleMicroAnimations) return;
        
        // Only apply idle animations when emotion intensity is low (to avoid conflicts with dramatic motions)
        if (currentEmotion.intensity > 0.4f || currentEmotion.arousal > 0.6f) return;
        
        // Subtle weight shifts (comfort seeking)
        if (Time.time > nextMicroShiftTime)
        {
            microShiftTarget = Random.Range(-2f, 2f);
            nextMicroShiftTime = Time.time + Random.Range(8f, 12f);
        }
        
        float currentShift = Mathf.Lerp(
            slimeTransform.localRotation.eulerAngles.z,
            microShiftTarget,
            Time.deltaTime * 0.5f
        );
        
        // Micro-wobbles (living tissue vibration) - only when very calm
        float microWobble = Mathf.PerlinNoise(Time.time * 10f, 0) * 0.01f;
        slimeMaterial.SetFloat("_WobbleAmount", 0.01f + microWobble);
        slimeMaterial.SetFloat("_WobbleSpeed", 15f);
        
        // Apply subtle rotation shift only during calm states
        slimeTransform.localRotation = Quaternion.Euler(0, 0, currentShift);
    }
    
    #endregion
    
    #region Energy & Fatigue System
    
    void UpdateEnergySystem()
    {
        // Energy depletion based on emotional intensity
        float depletionRate = energyDepletionRate * currentEmotion.intensity * currentEmotion.arousal;
        currentEnergy -= depletionRate * Time.deltaTime;
        
        // Energy recovery during calm states
        if (currentEmotion.arousal < 0.4f && currentEmotion.intensity < 0.5f)
        {
            currentEnergy += energyRecoveryRate * Time.deltaTime;
        }
        
        // Clamp energy
        currentEnergy = Mathf.Clamp01(currentEnergy);
        
        // Low energy reduces animation intensity
        if (currentEnergy < 0.3f)
        {
            // Force more calm/tired states when exhausted
            if (!isTransitioning && canChangeEmotion)
            {
                // Gradually shift toward tired
                targetEmotion.arousal *= 0.95f;
                targetEmotion.intensity *= 0.98f;
            }
        }
    }
    
    void UpdateResidualEffects()
    {
        // Decay residual effects over time
        tearPuffiness = Mathf.Lerp(tearPuffiness, 0f, Time.deltaTime * 0.1f);
        excitementResidue = Mathf.Lerp(excitementResidue, 0f, Time.deltaTime * 0.15f);
        tensionResidue = Mathf.Lerp(tensionResidue, 0f, Time.deltaTime * 0.12f);
        
        // Build residuals based on current emotion
        if (currentEmotion.valence < -0.6f && currentEmotion.intensity > 0.6f) // Crying
        {
            tearPuffiness = Mathf.Min(1f, tearPuffiness + Time.deltaTime * 0.5f);
        }
        
        if (currentEmotion.arousal > 0.85f && currentEmotion.valence > 0.7f) // Excited
        {
            excitementResidue = Mathf.Min(1f, excitementResidue + Time.deltaTime * 0.4f);
        }
        
        if (currentEmotion.dominance > 0.8f && currentEmotion.intensity > 0.7f) // Angry
        {
            tensionResidue = Mathf.Min(1f, tensionResidue + Time.deltaTime * 0.3f);
        }
    }
    
    #endregion
    
    #region Emotional Expression Application
    
    void ApplyEmotionalStateToSlime()
    {
        // Glow intensity from valence (happy = bright, sad = dim)
        float glowIntensity = Mathf.Lerp(1.2f, 3.5f, (currentEmotion.valence + 1f) / 2f);
        glowIntensity *= currentEmotion.intensity;
        slimeMaterial.SetFloat("_InnerGlowStrength", glowIntensity);
        slimeMaterial.SetFloat("_ParticleGlow", glowIntensity * 0.8f);
        
        // Color shift based on emotion
        float colorShift = 0f;
        if (currentEmotion.dominance > 0.7f) // Angry = red shift
        {
            colorShift = 30f * currentEmotion.intensity;
        }
        else if (currentEmotion.valence > 0.6f) // Happy = warm shift
        {
            colorShift = 10f * currentEmotion.intensity;
        }
        slimeMaterial.SetFloat("_ColorShift", colorShift);
    }
    
    void ApplyBodyLanguageToSlime()
    {
        // NOTE: DO NOT override transform here - ApplyBodyMotionToSlime handles it!
        // This method now only applies shader-based modulations
        
        // Wobble amount from arousal (only if not already set by body motion)
        if (currentEmotion.intensity < 0.5f)
        {
            float wobbleAmount = Mathf.Lerp(0.01f, 0.12f, currentEmotion.arousal) * currentEmotion.intensity;
            slimeMaterial.SetFloat("_WobbleAmount", wobbleAmount);
            
            float wobbleSpeed = Mathf.Lerp(2f, 8f, currentEmotion.arousal);
            slimeMaterial.SetFloat("_WobbleSpeed", wobbleSpeed);
        }
    }
    
    #endregion
    
    #region Synchronized Sprite Animations
    
    Vector2 GetSlimeCanvasPosition()
    {
        if (slimeTransform == null || spriteAnimationManager == null || spriteAnimationManager.canvas == null)
            return Vector2.zero;
        
        // Get slime world position
        Vector3 worldPos = slimeTransform.position;
        
        // Convert to canvas position
        RectTransform canvasRect = spriteAnimationManager.canvas.GetComponent<RectTransform>();
        Vector2 viewportPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector2 canvasPos = new Vector2(
            (viewportPos.x - 0.5f) * canvasRect.sizeDelta.x,
            (viewportPos.y - 0.5f) * canvasRect.sizeDelta.y
        );
        
        return canvasPos;
    }
    
    void UpdateSpriteAnimations()
    {
        if (spriteAnimationManager == null || !spriteAnimationManager.IsLoaded()) return;
        
        Vector2 slimePos = GetSlimeCanvasPosition();

        // Tears synchronized with breath (exhale phase only)
        if (currentEmotion.valence < -0.5f && currentEmotion.intensity > 0.5f)
        {
            if (breathPhase > 0.5f && breathPhase < 0.7f) // Exhale window
            {
                float tearProbability = Mathf.Lerp(0.02f, 0.15f, currentEmotion.intensity);
                if (Random.value < tearProbability)
                {
                    int tearCount = currentEmotion.intensity > 0.8f ? 2 : 1;
                    spriteAnimationManager.SpawnTear(slimePos, tearCount);
                }
            }
        }
        
        // Hearts during excited peak
        if (currentEmotion.arousal > 0.85f && currentEmotion.valence > 0.7f)
        {
            if (Random.value < 0.08f * currentEmotion.intensity)
            {
                spriteAnimationManager.SpawnHeart(slimePos);
            }
        }
        
        // Musical notes for happy/playful
        if (currentEmotion.valence > 0.6f && currentEmotion.arousal > 0.5f)
        {
            if (Random.value < 0.05f * currentEmotion.intensity)
            {
                spriteAnimationManager.SpawnMusicalNote(slimePos);
            }
        }
        
        // Sparkles synchronized with excitement peaks
        if (currentEmotion.arousal > 0.9f)
        {
            if (Random.value < 0.1f * currentEmotion.intensity)
            {
                spriteAnimationManager.SpawnSparkles(slimePos, 2);
            }
        }
        
        // Sweat for shy/embarrassed/scared
        if ((currentEmotion.dominance < 0.3f || currentEmotion.valence < -0.6f) && currentEmotion.arousal > 0.5f)
        {
            if (Random.value < 0.04f * currentEmotion.intensity)
            {
                spriteAnimationManager.SpawnSweat(slimePos, 1);
            }
        }
        
        // Blush bubble for shy/embarrassed
        if (currentEmotion.dominance < 0.3f && currentEmotion.arousal > 0.4f)
        {
            if (Random.value < 0.02f)
            {
                spriteAnimationManager.SpawnBlushBubble(slimePos);
            }
        }
        
        // Anger indicators
        if (currentEmotion.dominance > 0.8f && currentEmotion.intensity > 0.7f)
        {
            if (Random.value < 0.05f)
            {
                spriteAnimationManager.SpawnAngerSymbol(slimePos);
            }
            if (Random.value < 0.04f)
            {
                spriteAnimationManager.SpawnAngryVeins(slimePos);
            }
        }
        
        // Question marks for curious
        if (currentEmotion.engagement > 0.9f)
        {
            if (Random.value < 0.03f)
            {
                spriteAnimationManager.SpawnQuestionMark(slimePos);
            }
        }
        
        // Exclamation for scared
        if (currentEmotion.arousal > 0.9f && currentEmotion.dominance < 0.2f)
        {
            if (Random.value < 0.03f)
            {
                spriteAnimationManager.SpawnExclamation(slimePos);
            }
        }
    }
    
    #endregion
    
    #region Needs System (Virtual Pet Care)
    
    void UpdateNeedsSystem()
    {
        float deltaTime = Time.deltaTime;
        float timeSinceInteraction = Time.time - lastInteractionTime;
        
        // Attention depletes when ignored (loneliness)
        if (timeSinceInteraction > 30f) // After 30 seconds of no interaction
        {
            attentionMeter -= 0.02f * deltaTime;
        }
        
        // Hunger depletes naturally
        hungerMeter -= 0.01f * deltaTime; // Takes ~100 seconds to get hungry
        
        // Energy depletes based on activity (arousal)
        float activityDrain = currentEmotion.arousal * 0.03f * deltaTime;
        currentEnergy -= activityDrain;
        
        // Happiness influenced by needs being met
        if (attentionMeter < 0.3f || hungerMeter < 0.3f)
        {
            happinessMeter -= 0.05f * deltaTime; // Unhappy when needs unmet
        }
        else if (attentionMeter > 0.7f && hungerMeter > 0.7f)
        {
            happinessMeter += 0.02f * deltaTime; // Happy when well cared for
        }
        
        // Clamp all meters
        attentionMeter = Mathf.Clamp01(attentionMeter);
        happinessMeter = Mathf.Clamp01(happinessMeter);
        hungerMeter = Mathf.Clamp01(hungerMeter);
        currentEnergy = Mathf.Clamp01(currentEnergy);
        
        // Auto-adjust emotions based on needs
        AutoAdjustEmotionFromNeeds();
    }
    
    void AutoAdjustEmotionFromNeeds()
    {
        if (!isTransitioning && canChangeEmotion)
        {
            // Prioritize sleep when exhausted
            if (currentEnergy < 0.2f && currentEmotionName != "Sleeping" && currentEmotionName != "Drowsy")
            {
                if (currentEnergy < 0.1f)
                    SetEmotionPreset(EmotionPreset.Sleeping);
                else
                    SetEmotionPreset(EmotionPreset.Drowsy);
            }
            // Show hunger
            else if (hungerMeter < 0.3f && currentEmotionName != "Hungry")
            {
                SetEmotionPreset(EmotionPreset.Hungry);
            }
            // Show loneliness/sadness when attention is low
            else if (attentionMeter < 0.3f && currentEmotionName != "Sad" && currentEmotionName != "Lonely")
            {
                SetEmotionPreset(EmotionPreset.Lonely);
            }
        }
    }
    
    // Public methods for player interaction
    public void GiveAttention()
    {
        attentionMeter = Mathf.Min(1f, attentionMeter + 0.3f);
        happinessMeter = Mathf.Min(1f, happinessMeter + 0.2f);
        lastInteractionTime = Time.time;
        
        // React with affection if happy
        if (happinessMeter > 0.6f && canChangeEmotion)
        {
            SetEmotionPreset(EmotionPreset.Affectionate);
        }
    }
    
    public void Feed()
    {
        hungerMeter = 1f;
        happinessMeter = Mathf.Min(1f, happinessMeter + 0.25f);
        lastInteractionTime = Time.time;
        
        // React with contentment
        if (canChangeEmotion)
        {
            SetEmotionPreset(EmotionPreset.Content);
        }
    }
    
    #endregion
    
    #region Natural Visual Effects (No Sprite Overlays)
    
    void ApplyNaturalVisualEffects()
    {
        // Update effect intensities based on emotional state
        UpdateTearEffects();
        UpdateBlushEffect();
        UpdateHeartGlowEffect();
        UpdateSweatingEffect();
        UpdateSleepEffects();
        
        // Apply to shader (assuming shader properties exist or will be added)
        ApplyVisualEffectsToShader();
    }
    
    void UpdateTearEffects()
    {
        // Natural tears: eye wetness + redness when sad
        if (currentEmotion.valence < -0.5f && currentEmotion.intensity > 0.5f)
        {
            eyeWetness = Mathf.Lerp(eyeWetness, 0.8f, Time.deltaTime * 2f);
            eyeRedness = Mathf.Lerp(eyeRedness, 0.6f, Time.deltaTime * 1f);
            tearPuffiness = Mathf.Lerp(tearPuffiness, 0.7f, Time.deltaTime * 0.5f);
        }
        else
        {
            eyeWetness = Mathf.Lerp(eyeWetness, 0.1f, Time.deltaTime * 0.5f);
            eyeRedness = Mathf.Lerp(eyeRedness, 0f, Time.deltaTime * 0.3f);
            tearPuffiness = Mathf.Lerp(tearPuffiness, 0f, Time.deltaTime * 0.2f);
        }
    }
    
    void UpdateBlushEffect()
    {
        // Blush for shy, embarrassed, or affectionate states
        if ((currentEmotion.dominance < 0.3f && currentEmotion.arousal > 0.4f) || 
            currentEmotionName == "Affectionate")
        {
            blushIntensity = Mathf.Lerp(blushIntensity, 0.7f, Time.deltaTime * 3f);
        }
        else
        {
            blushIntensity = Mathf.Lerp(blushIntensity, 0f, Time.deltaTime * 2f);
        }
    }
    
    void UpdateHeartGlowEffect()
    {
        // Internal heart glow for love/affection
        if (currentEmotionName == "Affectionate" || currentEmotionName == "Happy" && happinessMeter > 0.8f)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f; // Pulsing
            internalHeartGlow = Mathf.Lerp(internalHeartGlow, 0.8f * pulse, Time.deltaTime * 4f);
        }
        else
        {
            internalHeartGlow = Mathf.Lerp(internalHeartGlow, 0f, Time.deltaTime * 2f);
        }
    }
    
    void UpdateSweatingEffect()
    {
        // Glistening wetness for anxiety/scared/hot states
        if (currentEmotion.arousal > 0.7f && (currentEmotion.dominance < 0.4f || currentEmotionName == "Scared"))
        {
            sweatingAmount = Mathf.Lerp(sweatingAmount, 0.6f, Time.deltaTime * 2f);
        }
        else
        {
            sweatingAmount = Mathf.Lerp(sweatingAmount, 0f, Time.deltaTime * 1f);
        }
    }
    
    void UpdateSleepEffects()
    {
        // Sleep depth affects multiple visual parameters
        if (currentEmotionName == "Sleeping")
        {
            sleepDepth = Mathf.Lerp(sleepDepth, 1f, Time.deltaTime * 0.5f); // Gradually fall into deep sleep
        }
        else if (currentEmotionName == "Drowsy")
        {
            sleepDepth = Mathf.Lerp(sleepDepth, 0.3f, Time.deltaTime * 1f);
        }
        else
        {
            sleepDepth = Mathf.Lerp(sleepDepth, 0f, Time.deltaTime * 2f);
        }
    }
    
    void ApplyVisualEffectsToShader()
    {
        // Apply all natural effects to shader properties
        // (These shader properties would need to be added to your slime shader)
        
        // Eye effects
        if (slimeMaterial.HasProperty("_EyeWetness"))
            slimeMaterial.SetFloat("_EyeWetness", eyeWetness);
        
        if (slimeMaterial.HasProperty("_EyeRedness"))
            slimeMaterial.SetFloat("_EyeRedness", eyeRedness);
        
        // Blush (soft pink gradient on cheeks)
        if (slimeMaterial.HasProperty("_BlushIntensity"))
            slimeMaterial.SetFloat("_BlushIntensity", blushIntensity);
        
        // Internal heart glow (visible through translucency)
        if (slimeMaterial.HasProperty("_HeartGlow"))
            slimeMaterial.SetFloat("_HeartGlow", internalHeartGlow);
        
        // Surface wetness/sweating
        if (slimeMaterial.HasProperty("_SurfaceWetness"))
            slimeMaterial.SetFloat("_SurfaceWetness", sweatingAmount);
        
        // Sleep effects (dimmed glow, closed eyes handled in eye system)
        if (slimeMaterial.HasProperty("_SleepDimming"))
            slimeMaterial.SetFloat("_SleepDimming", sleepDepth * 0.5f);
        
        // Modify existing glow based on needs/happiness
        float needsModifier = (attentionMeter + happinessMeter) * 0.5f;
        float glowMult = Mathf.Lerp(0.5f, 1.2f, needsModifier);
        
        // This modulates the existing inner glow strength
        float currentGlow = slimeMaterial.GetFloat("_InnerGlowStrength");
        slimeMaterial.SetFloat("_InnerGlowStrength", currentGlow * glowMult);
    }
    
    #endregion
    
    #region History Tracking
    
    void TrackEmotionHistory()
    {
        // Store emotion state every 3 seconds
        if (emotionTimer % 3f < Time.deltaTime)
        {
            emotionHistory.Enqueue(currentEmotion.Clone());
            
            if (emotionHistory.Count > maxHistorySize)
            {
                emotionHistory.Dequeue();
            }
        }
    }
    
    #endregion
}
