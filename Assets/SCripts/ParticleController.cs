using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [Header("Particle Systems")]
    [SerializeField] private ParticleSystem sparkleParticles;
    [SerializeField] private ParticleSystem heartParticles;
    [SerializeField] private ParticleSystem touchEffectParticles;
    [SerializeField] private ParticleSystem ambientGlowParticles;
    
    [Header("Sparkle Settings")]
    [SerializeField] private Color sparkleColor = new Color(0.5f, 1f, 0.8f, 1f); // Light green
    [SerializeField] private int sparklesPerBurst = 5;
    [SerializeField] private float sparkleLifetime = 1f;
    [SerializeField] private float sparkleSize = 0.1f;
    
    [Header("Heart Settings")]
    [SerializeField] private Color heartColor = new Color(1f, 0.3f, 0.5f);
    [SerializeField] private int heartsPerBurst = 3;
    [SerializeField] private float heartLifetime = 1.5f;
    [SerializeField] private float heartSize = 0.2f;
    
    [Header("Touch Effect Settings")]
    [SerializeField] private int touchEffectCount = 10;
    [SerializeField] private float touchEffectLifetime = 0.3f;
    [SerializeField] private float touchEffectSize = 0.08f;
    
    [Header("Auto-Creation")]
    [SerializeField] private bool autoCreateParticleSystems = true;
    
    void Start()
    {
        // Wait a frame for DualLayerJelly to create materials
        StartCoroutine(InitializeParticles());
    }
    
    IEnumerator InitializeParticles()
    {
        yield return new WaitForEndOfFrame();
        
        // Get slime color from renderer
        Renderer slimeRenderer = GetComponent<Renderer>();
        if (slimeRenderer != null && slimeRenderer.material != null)
        {
            Color slimeColor = slimeRenderer.material.GetColor("_Color");
            sparkleColor = slimeColor;
            Debug.Log($"<color=lime>✓ Particles using slime color: {slimeColor}</color>");
        }
        else
        {
            // Fallback to light green
            sparkleColor = new Color(0.5f, 1f, 0.8f, 1f);
            Debug.Log("<color=yellow>⚠ Using default light green for particles</color>");
        }
        
        if (autoCreateParticleSystems)
        {
            CreateParticleSystems();
        }
    }
    
    void CreateParticleSystems()
    {
        // Create Sparkle Particles
        if (sparkleParticles == null)
        {
            GameObject sparkleObj = new GameObject("SparkleParticles");
            sparkleObj.transform.SetParent(transform);
            sparkleObj.transform.localPosition = Vector3.zero;
            sparkleParticles = sparkleObj.AddComponent<ParticleSystem>();
            ConfigureSparkleParticles();
        }
        
        // Create Heart Particles
        if (heartParticles == null)
        {
            GameObject heartObj = new GameObject("HeartParticles");
            heartObj.transform.SetParent(transform);
            heartObj.transform.localPosition = Vector3.zero;
            heartParticles = heartObj.AddComponent<ParticleSystem>();
            ConfigureHeartParticles();
        }
        
        // Create Touch Effect Particles
        if (touchEffectParticles == null)
        {
            GameObject touchObj = new GameObject("TouchEffectParticles");
            touchObj.transform.SetParent(transform);
            touchObj.transform.localPosition = Vector3.zero;
            touchEffectParticles = touchObj.AddComponent<ParticleSystem>();
            ConfigureTouchEffectParticles();
        }
        
        // Create Ambient Glow Particles
        if (ambientGlowParticles == null)
        {
            GameObject ambientObj = new GameObject("AmbientGlowParticles");
            ambientObj.transform.SetParent(transform);
            ambientObj.transform.localPosition = Vector3.zero;
            ambientGlowParticles = ambientObj.AddComponent<ParticleSystem>();
            ConfigureAmbientGlowParticles();
        }
        
        Debug.Log("Particle systems created for slime");
    }
    
    void ConfigureSparkleParticles()
    {
        var main = sparkleParticles.main;
        main.startLifetime = sparkleLifetime;
        main.startSpeed = 2f;
        main.startSize = sparkleSize;
        main.startColor = sparkleColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 100;
        
        var emission = sparkleParticles.emission;
        emission.enabled = false; // Burst only
        
        var shape = sparkleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        var colorOverLifetime = sparkleParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(sparkleColor, 0f), new GradientColorKey(sparkleColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        var sizeOverLifetime = sparkleParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.5f);
        curve.AddKey(0.5f, 1f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        
        var renderer = sparkleParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateAdditiveMaterial();
    }
    
    void ConfigureHeartParticles()
    {
        var main = heartParticles.main;
        main.startLifetime = heartLifetime;
        main.startSpeed = 1f;
        main.startSize = heartSize;
        main.startColor = heartColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 50;
        main.gravityModifier = -0.5f; // Float upward
        
        var emission = heartParticles.emission;
        emission.enabled = false;
        
        var shape = heartParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 25f;
        shape.radius = 0.05f;
        
        var colorOverLifetime = heartParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(heartColor, 0f), new GradientColorKey(heartColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        var sizeOverLifetime = heartParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve();
        curve.AddKey(0f, 0.5f);
        curve.AddKey(0.3f, 1.2f);
        curve.AddKey(1f, 0f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        
        var renderer = heartParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateAdditiveMaterial();
    }
    
    void ConfigureTouchEffectParticles()
    {
        var main = touchEffectParticles.main;
        main.startLifetime = touchEffectLifetime;
        main.startSpeed = 3f;
        main.startSize = touchEffectSize;
        main.startColor = new Color(1f, 1f, 1f, 0.8f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 200;
        
        var emission = touchEffectParticles.emission;
        emission.enabled = false;
        
        var shape = touchEffectParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Hemisphere;
        shape.radius = 0.05f;
        
        var colorOverLifetime = touchEffectParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        var velocityOverLifetime = touchEffectParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.speedModifier = new ParticleSystem.MinMaxCurve(0.5f);
        
        var renderer = touchEffectParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateAdditiveMaterial();
    }
    
    void ConfigureAmbientGlowParticles()
    {
        var main = ambientGlowParticles.main;
        main.startLifetime = 3f;
        main.startSpeed = 0.2f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor = new Color(sparkleColor.r, sparkleColor.g, sparkleColor.b, 0.5f);
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 30;
        main.loop = true;
        
        var emission = ambientGlowParticles.emission;
        emission.enabled = true;
        emission.rateOverTime = 3f; // Low continuous emission
        
        var shape = ambientGlowParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1.5f;
        
        var colorOverLifetime = ambientGlowParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(sparkleColor, 0f), new GradientColorKey(sparkleColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        var velocityOverLifetime = ambientGlowParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.orbitalY = new ParticleSystem.MinMaxCurve(0.1f);
        
        var renderer = ambientGlowParticles.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = CreateAdditiveMaterial();
    }
    
    Material CreateAdditiveMaterial()
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.SetInt("_BlendMode", 1); // Additive
        mat.SetColor("_Color", Color.white);
        mat.EnableKeyword("_EMISSION");
        return mat;
    }
    
    /// <summary>
    /// Emit sparkles at a specific world position
    /// </summary>
    public void EmitSparkles(Vector3 worldPosition, int count = -1)
    {
        if (sparkleParticles == null) return;
        
        if (count < 0) count = sparklesPerBurst;
        
        sparkleParticles.transform.position = worldPosition;
        sparkleParticles.Emit(count);
    }
    
    /// <summary>
    /// Emit hearts at a specific world position
    /// </summary>
    public void EmitHearts(Vector3 worldPosition, int count = -1)
    {
        if (heartParticles == null) return;
        
        if (count < 0) count = heartsPerBurst;
        
        heartParticles.transform.position = worldPosition;
        heartParticles.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Point up
        heartParticles.Emit(count);
    }
    
    /// <summary>
    /// Emit touch impact effect at a specific world position
    /// </summary>
    public void EmitTouchEffect(Vector3 worldPosition)
    {
        if (touchEffectParticles == null) return;
        
        touchEffectParticles.transform.position = worldPosition;
        touchEffectParticles.transform.rotation = Quaternion.LookRotation(worldPosition - transform.position);
        touchEffectParticles.Emit(touchEffectCount);
    }
    
    /// <summary>
    /// Emit tap effect (combination of sparkles and touch effect)
    /// </summary>
    public void EmitTapEffect(Vector3 worldPosition)
    {
        EmitTouchEffect(worldPosition);
        EmitSparkles(worldPosition, sparklesPerBurst);
    }
    
    /// <summary>
    /// Emit happy effect (hearts and sparkles)
    /// </summary>
    public void EmitHappyEffect(Vector3 worldPosition)
    {
        EmitHearts(worldPosition, heartsPerBurst);
        EmitSparkles(worldPosition, sparklesPerBurst * 2);
    }
    
    /// <summary>
    /// Change sparkle color dynamically
    /// </summary>
    public void SetSparkleColor(Color color)
    {
        sparkleColor = color;
        if (sparkleParticles != null)
        {
            var main = sparkleParticles.main;
            main.startColor = color;
        }
    }
    
    /// <summary>
    /// Change heart color dynamically
    /// </summary>
    public void SetHeartColor(Color color)
    {
        heartColor = color;
        if (heartParticles != null)
        {
            var main = heartParticles.main;
            main.startColor = color;
        }
    }
    
    /// <summary>
    /// Enable/disable ambient particles
    /// </summary>
    public void SetAmbientParticlesActive(bool active)
    {
        if (ambientGlowParticles != null)
        {
            var emission = ambientGlowParticles.emission;
            emission.enabled = active;
        }
    }
}
