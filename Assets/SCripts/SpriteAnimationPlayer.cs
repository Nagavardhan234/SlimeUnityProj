using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Plays sprite sheet animations from the itch folder.
/// Dynamically loads sprites and animates them frame by frame.
/// </summary>
public class SpriteAnimationPlayer : MonoBehaviour
{
    public enum AnimationType
    {
        TearDrop,
        SweatDrop,
        MusicNotes,
        HeartLove,
        QuestionMarks,
        ExclamationMarks,
        Sparkles,
        BlushBubble,
        AngerSymbol,
        StressLines,
        SpiritIcon,
        ShockRays,
        AngryVeins,
        FrustrationScribble
    }
    
    [System.Serializable]
    public class AnimationData
    {
        public AnimationType type;
        public Sprite[] frames;
        public float frameRate = 24f;
        public bool loop = false;
        public int frameCount = 0;
    }
    
    private Image imageComponent;
    private AnimationData currentAnimation;
    private int currentFrame = 0;
    private float frameTimer = 0f;
    private bool isPlaying = false;
    private bool shouldDestroy = false;
    
    // Physics
    private Vector2 velocity;
    private float gravity = 500f;
    private float lifetime = 0f;
    private float maxLifetime = 3f;
    
    void Awake()
    {
        imageComponent = GetComponent<Image>();
        if (imageComponent == null)
        {
            imageComponent = gameObject.AddComponent<Image>();
        }
        imageComponent.raycastTarget = false;
    }
    
    void Update()
    {
        if (!isPlaying) return;
        
        // Update animation
        frameTimer += Time.deltaTime;
        float frameDuration = 1f / currentAnimation.frameRate;
        
        if (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            currentFrame++;
            
            if (currentFrame >= currentAnimation.frameCount)
            {
                if (currentAnimation.loop)
                {
                    currentFrame = 0;
                }
                else
                {
                    isPlaying = false;
                    if (shouldDestroy)
                    {
                        Destroy(gameObject);
                    }
                    return;
                }
            }
            
            if (currentFrame < currentAnimation.frames.Length && currentAnimation.frames[currentFrame] != null)
            {
                imageComponent.sprite = currentAnimation.frames[currentFrame];
            }
        }
        
        // Update physics
        lifetime += Time.deltaTime;
        
        // Apply gravity for falling animations
        if (currentAnimation.type == AnimationType.TearDrop || 
            currentAnimation.type == AnimationType.SweatDrop)
        {
            velocity.y -= gravity * Time.deltaTime;
        }
        
        // Move
        RectTransform rt = transform as RectTransform;
        rt.anchoredPosition += velocity * Time.deltaTime;
        
        // Fade out
        if (lifetime > maxLifetime * 0.7f)
        {
            float alpha = 1f - ((lifetime - maxLifetime * 0.7f) / (maxLifetime * 0.3f));
            Color c = imageComponent.color;
            c.a = Mathf.Max(0, alpha);
            imageComponent.color = c;
        }
        
        // Destroy when lifetime exceeded
        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
        }
    }
    
    public void PlayAnimation(AnimationData animData, Vector2 startVelocity, float duration = 3f, bool destroyOnComplete = true)
    {
        currentAnimation = animData;
        currentFrame = 0;
        frameTimer = 0f;
        isPlaying = true;
        velocity = startVelocity;
        maxLifetime = duration;
        lifetime = 0f;
        shouldDestroy = destroyOnComplete;
        
        if (currentAnimation.frames.Length > 0 && currentAnimation.frames[0] != null)
        {
            imageComponent.sprite = currentAnimation.frames[0];
        }
        
        imageComponent.color = Color.white;
    }
    
    public void StopAnimation()
    {
        isPlaying = false;
    }
}
