using UnityEngine;

public class TextEmotionAnimator : MonoBehaviour
{
    public enum TextEmotionType
    {
        Wobble,
        Shake,
        Pulse,
        Tiny,
        Tremble,
        Stretch,
        Squish,
        Float,
        Pop,
        Drip,
        Wave,
        Glow,
        Blink
    }

    [Header("Emotion Animation Settings")]
    public TextEmotionType emotionStyle = TextEmotionType.Wobble;
    [Tooltip("Press Space to cycle emotion type for debug.")]
    public bool enableDebugKey = true;
    
    [Header("Slime Control")]
    public SlimeController slimeController;
    
    [Header("Visual Feedback")]
    public string currentEmotionName = "Wobble";

    private Material slimeMaterial;
    private Transform slimeTransform;

    void Start()
    {
        StartCoroutine(InitializeAfterSlime());
    }
    
    System.Collections.IEnumerator InitializeAfterSlime()
    {
        // Wait one frame to ensure SlimeController.Start() has run
        yield return null;
        
        if (slimeController == null)
        {
            slimeController = FindObjectOfType<SlimeController>();
        }
        
        if (slimeController != null)
        {
            // Access slime components via public accessors
            slimeMaterial = slimeController.GetSlimeMaterial();
            slimeTransform = slimeController.GetSlimeTransform();
            
            // Disable SlimeController's idle animation so we have full control
            slimeController.SetIdleAnimationEnabled(false);
            
            if (slimeMaterial == null)
            {
                Debug.LogError("TextEmotionAnimator: Could not get slime material! Make sure SlimeController has created the slime.");
            }
            else
            {
                Debug.Log("TextEmotionAnimator: Successfully connected to slime! Change emotion dropdown or press Space.");
            }
        }
        else
        {
            Debug.LogError("TextEmotionAnimator: SlimeController not found in scene!");
        }
    }

    void Update()
    {
        // Debug key to cycle emotions
        if (enableDebugKey && Input.GetKeyDown(KeyCode.Space))
        {
            CycleEmotionType();
        }
        
        // Update display name
        currentEmotionName = emotionStyle.ToString();
        
        if (slimeMaterial == null) return;
        
        // Apply selected animation to actual slime
        switch (emotionStyle)
        {
            case TextEmotionType.Wobble:
                ApplyWobbleAnimation();
                break;
            case TextEmotionType.Shake:
                ApplyShakeAnimation();
                break;
            case TextEmotionType.Pulse:
                ApplyPulseAnimation();
                break;
            case TextEmotionType.Tiny:
                ApplyTinyAnimation();
                break;
            case TextEmotionType.Tremble:
                ApplyTrembleAnimation();
                break;
            case TextEmotionType.Stretch:
                ApplyStretchAnimation();
                break;
            case TextEmotionType.Squish:
                ApplySquishAnimation();
                break;
            case TextEmotionType.Float:
                ApplyFloatAnimation();
                break;
            case TextEmotionType.Pop:
                ApplyPopAnimation();
                break;
            case TextEmotionType.Drip:
                ApplyDripAnimation();
                break;
            case TextEmotionType.Wave:
                ApplyWaveAnimation();
                break;
            case TextEmotionType.Glow:
                ApplyGlowAnimation();
                break;
            case TextEmotionType.Blink:
                ApplyBlinkAnimation();
                break;
        }
    }

    void CycleEmotionType()
    {
        int next = ((int)emotionStyle + 1) % System.Enum.GetValues(typeof(TextEmotionType)).Length;
        emotionStyle = (TextEmotionType)next;
        Debug.Log("Emotion switched to: " + emotionStyle.ToString());
    }

    // --- EMOTION ANIMATION METHODS (Control Actual Slime) ---

    void ApplyWobbleAnimation()
    {
        // Gentle vertical sine wave movement
        float time = Time.time;
        slimeMaterial.SetFloat("_WobbleAmount", 0.05f);
        slimeMaterial.SetFloat("_WobbleSpeed", 3.0f);
        if (slimeTransform != null)
        {
            float wobble = Mathf.Sin(time * 3f) * 5f;
            slimeTransform.rotation = Quaternion.Euler(0, 0, wobble);
        }
    }

    void ApplyShakeAnimation()
    {
        // Rapid horizontal jitter simulating fear
        if (slimeTransform != null)
        {
            float shakeX = Random.Range(-3f, 3f);
            float shakeY = Random.Range(-1.5f, 1.5f);
            slimeTransform.localPosition = new Vector3(shakeX * 0.02f, shakeY * 0.02f, 0);
        }
        slimeMaterial.SetFloat("_WobbleAmount", 0.08f);
        slimeMaterial.SetFloat("_WobbleSpeed", 15.0f);
    }

    void ApplyPulseAnimation()
    {
        // Smooth scale up/down globally (breathing effect)
        float time = Time.time;
        float pulse = 1f + Mathf.Sin(time * 3f) * 0.08f;
        slimeMaterial.SetFloat("_BreathingPulse", pulse);
        slimeMaterial.SetFloat("_WobbleAmount", 0.01f);
    }

    void ApplyTinyAnimation()
    {
        // Reduce scale to 0.7 size with shy expression
        slimeMaterial.SetFloat("_BreathingPulse", 0.7f);
        slimeMaterial.SetFloat("_EyeEmotiveness", 0.6f);
        slimeMaterial.SetFloat("_WobbleAmount", 0.005f);
    }

    void ApplyTrembleAnimation()
    {
        // Random vertical shake small amplitude (cold trembling)
        float time = Time.time;
        float tremble = Mathf.Sin(time * 40f) * 0.015f;
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(0, tremble, 0);
        }
        slimeMaterial.SetFloat("_WobbleAmount", 0.03f);
        slimeMaterial.SetFloat("_WobbleSpeed", 8.0f);
    }

    void ApplyStretchAnimation()
    {
        // Horizontal scale cycling
        float time = Time.time;
        float scaleX = Mathf.Lerp(0.9f, 1.15f, (Mathf.Sin(time * 2f) + 1f) / 2f);
        if (slimeTransform != null)
        {
            slimeTransform.localScale = new Vector3(3.6f * scaleX, 3.6f, 1);
        }
        slimeMaterial.SetFloat("_WobbleAmount", 0.02f);
    }

    void ApplySquishAnimation()
    {
        // Vertical squish spring effect
        float time = Time.time;
        float squish = Mathf.Lerp(0f, 0.6f, (Mathf.Sin(time * 3f) + 1f) / 2f);
        slimeMaterial.SetFloat("_SquishAmount", squish);
        slimeMaterial.SetFloat("_BounceOffset", Mathf.Sin(time * 3f) * 0.1f);
    }

    void ApplyFloatAnimation()
    {
        // Soft up/down movement long period
        float time = Time.time;
        float yOffset = Mathf.Sin(time * 1f) * 0.08f;
        if (slimeTransform != null)
        {
            slimeTransform.localPosition = new Vector3(0, yOffset, 0);
        }
        slimeMaterial.SetFloat("_WobbleAmount", 0.025f);
        slimeMaterial.SetFloat("_WobbleSpeed", 1.5f);
    }

    void ApplyPopAnimation()
    {
        // Excited popping scale
        float time = Time.time;
        float pop = 1f + Mathf.Abs(Mathf.Sin(time * 5f)) * 0.15f;
        slimeMaterial.SetFloat("_BreathingPulse", pop);
        slimeMaterial.SetFloat("_EyeEmotiveness", 1.4f);
        slimeMaterial.SetFloat("_WobbleAmount", 0.04f);
        slimeMaterial.SetFloat("_WobbleSpeed", 6.0f);
    }

    void ApplyDripAnimation()
    {
        // Melting downward effect
        float time = Time.time;
        float drip = Mathf.Max(0, Mathf.Sin(time * 0.7f)) * 0.15f;
        slimeMaterial.SetFloat("_SquishAmount", drip * 0.5f);
        slimeMaterial.SetFloat("_BounceOffset", -drip);
        slimeMaterial.SetFloat("_BottomSquish", 0.4f + drip * 0.3f);
    }

    void ApplyWaveAnimation()
    {
        // Wave movement across slime
        float time = Time.time;
        float wave = Mathf.Sin(time * 4f) * 8f;
        if (slimeTransform != null)
        {
            slimeTransform.rotation = Quaternion.Euler(0, 0, wave);
        }
        slimeMaterial.SetFloat("_WobbleAmount", 0.06f);
        slimeMaterial.SetFloat("_WobbleSpeed", 4.0f);
    }

    void ApplyGlowAnimation()
    {
        // Increase glow intensity pulsing
        float time = Time.time;
        float glow = 3.0f + Mathf.Sin(time * 2f) * 1.5f;
        slimeMaterial.SetFloat("_InnerGlowStrength", glow);
        slimeMaterial.SetFloat("_ParticleGlow", glow * 0.8f);
        slimeMaterial.SetFloat("_HighlightIntensity", 1.4f + Mathf.Sin(time * 3f) * 0.4f);
    }

    void ApplyBlinkAnimation()
    {
        // Eye blinking loop
        float time = Time.time;
        float blink = Mathf.Abs(Mathf.Sin(time * 2f));
        slimeMaterial.SetFloat("_BlinkAmount", 1f - blink);
        slimeMaterial.SetFloat("_WobbleAmount", 0.015f);
    }
}
