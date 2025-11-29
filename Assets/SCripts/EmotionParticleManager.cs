using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages particle effects for slime emotions (tears, sweat, sparkles, icons).
/// Uses object pooling for performance.
/// </summary>
public class EmotionParticleManager : MonoBehaviour
{
    [Header("References")]
    public Transform slimeTransform;
    public Canvas canvas;
    
    [Header("Particle Prefabs")]
    public GameObject tearPrefab;
    public GameObject sweatPrefab;
    public GameObject zParticlePrefab;
    public GameObject musicalNotePrefab;
    public GameObject iconPrefab;  // For ?, !, etc
    
    [Header("Pool Settings")]
    public int poolSize = 20;
    
    // Object pools
    private List<GameObject> tearPool = new List<GameObject>();
    private List<GameObject> sweatPool = new List<GameObject>();
    private List<GameObject> zPool = new List<GameObject>();
    private List<GameObject> notePool = new List<GameObject>();
    private List<GameObject> iconPool = new List<GameObject>();
    
    // Active particles
    private List<ParticleInstance> activeParticles = new List<ParticleInstance>();
    
    private class ParticleInstance
    {
        public GameObject obj;
        public float lifetime;
        public float age;
        public Vector3 velocity;
        public ParticleType type;
    }
    
    public enum ParticleType
    {
        Tear, Sweat, Z, MusicalNote, Question, Exclamation, Heart
    }
    
    void Start()
    {
        if (canvas == null) canvas = FindObjectOfType<Canvas>();
        InitializePools();
    }
    
    void InitializePools()
    {
        // Create simple fallback prefabs if none assigned
        if (tearPrefab == null) tearPrefab = CreateSimpleTearPrefab();
        if (sweatPrefab == null) sweatPrefab = CreateSimpleSweatPrefab();
        if (zParticlePrefab == null) zParticlePrefab = CreateSimpleZPrefab();
        if (musicalNotePrefab == null) musicalNotePrefab = CreateSimpleNotePrefab();
        if (iconPrefab == null) iconPrefab = CreateSimpleIconPrefab();
        
        // Initialize pools
        for (int i = 0; i < poolSize; i++)
        {
            tearPool.Add(Instantiate(tearPrefab, canvas.transform));
            sweatPool.Add(Instantiate(sweatPrefab, canvas.transform));
            zPool.Add(Instantiate(zParticlePrefab, canvas.transform));
            notePool.Add(Instantiate(musicalNotePrefab, canvas.transform));
            iconPool.Add(Instantiate(iconPrefab, canvas.transform));
            
            tearPool[i].SetActive(false);
            sweatPool[i].SetActive(false);
            zPool[i].SetActive(false);
            notePool[i].SetActive(false);
            iconPool[i].SetActive(false);
        }
    }
    
    GameObject CreateSimpleTearPrefab()
    {
        GameObject tear = new GameObject("Tear");
        UnityEngine.UI.Image img = tear.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.6f, 0.8f, 1f, 0.8f);  // Light blue
        RectTransform rt = tear.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(15, 20);  // Tear drop shape
        return tear;
    }
    
    GameObject CreateSimpleSweatPrefab()
    {
        GameObject sweat = new GameObject("Sweat");
        UnityEngine.UI.Image img = sweat.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.7f, 0.9f, 1f, 0.7f);  // Very light blue
        RectTransform rt = sweat.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(25, 30);
        return sweat;
    }
    
    GameObject CreateSimpleZPrefab()
    {
        GameObject z = new GameObject("Z");
        UnityEngine.UI.Text txt = z.AddComponent<UnityEngine.UI.Text>();
        txt.text = "Z";
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 40;
        txt.color = new Color(0.4f, 0.4f, 0.8f, 0.8f);
        txt.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = z.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
        return z;
    }
    
    GameObject CreateSimpleNotePrefab()
    {
        GameObject note = new GameObject("Note");
        UnityEngine.UI.Text txt = note.AddComponent<UnityEngine.UI.Text>();
        txt.text = "♪";
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 35;
        txt.color = new Color(1f, 0.8f, 0.4f, 0.9f);
        txt.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = note.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(40, 40);
        return note;
    }
    
    GameObject CreateSimpleIconPrefab()
    {
        GameObject icon = new GameObject("Icon");
        UnityEngine.UI.Text txt = icon.AddComponent<UnityEngine.UI.Text>();
        txt.text = "?";
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 50;
        txt.color = new Color(1f, 1f, 1f, 0.95f);
        txt.alignment = TextAnchor.MiddleCenter;
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(60, 60);
        return icon;
    }
    
    void Update()
    {
        UpdateActiveParticles();
    }
    
    void UpdateActiveParticles()
    {
        for (int i = activeParticles.Count - 1; i >= 0; i--)
        {
            ParticleInstance particle = activeParticles[i];
            particle.age += Time.deltaTime;
            
            if (particle.age >= particle.lifetime)
            {
                particle.obj.SetActive(false);
                activeParticles.RemoveAt(i);
                continue;
            }
            
            // Update position
            RectTransform rt = particle.obj.GetComponent<RectTransform>();
            Vector3 newPos = rt.anchoredPosition;
            newPos += particle.velocity * Time.deltaTime;
            
            // Apply gravity for tears/sweat
            if (particle.type == ParticleType.Tear || particle.type == ParticleType.Sweat)
            {
                particle.velocity.y -= 500f * Time.deltaTime;  // Gravity
            }
            
            rt.anchoredPosition = newPos;
            
            // Fade out
            float alpha = 1f - (particle.age / particle.lifetime);
            CanvasGroup cg = particle.obj.GetComponent<CanvasGroup>();
            if (cg == null) cg = particle.obj.AddComponent<CanvasGroup>();
            cg.alpha = alpha;
        }
    }
    
    public void SpawnTear(Vector2 position, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject tear = GetFromPool(tearPool);
            if (tear == null) return;
            
            RectTransform rt = tear.GetComponent<RectTransform>();
            rt.anchoredPosition = position + new Vector2(Random.Range(-20f, 20f), Random.Range(-10f, 10f));
            tear.SetActive(true);
            
            ParticleInstance instance = new ParticleInstance
            {
                obj = tear,
                lifetime = Random.Range(1.5f, 2.5f),
                age = 0f,
                velocity = new Vector3(Random.Range(-30f, 30f), -100f, 0),
                type = ParticleType.Tear
            };
            activeParticles.Add(instance);
        }
    }
    
    public void SpawnSweat(Vector2 position, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject sweat = GetFromPool(sweatPool);
            if (sweat == null) return;
            
            RectTransform rt = sweat.GetComponent<RectTransform>();
            rt.anchoredPosition = position + new Vector2(Random.Range(80f, 120f), Random.Range(50f, 100f));
            sweat.SetActive(true);
            
            ParticleInstance instance = new ParticleInstance
            {
                obj = sweat,
                lifetime = Random.Range(1f, 2f),
                age = 0f,
                velocity = new Vector3(0, -150f, 0),
                type = ParticleType.Sweat
            };
            activeParticles.Add(instance);
        }
    }
    
    public void SpawnZ(Vector2 position)
    {
        GameObject z = GetFromPool(zPool);
        if (z == null) return;
        
        RectTransform rt = z.GetComponent<RectTransform>();
        rt.anchoredPosition = position + new Vector2(Random.Range(-30f, 30f), 150f);
        z.SetActive(true);
        
        ParticleInstance instance = new ParticleInstance
        {
            obj = z,
            lifetime = 2f,
            age = 0f,
            velocity = new Vector3(Random.Range(-20f, 20f), 80f, 0),
            type = ParticleType.Z
        };
        activeParticles.Add(instance);
    }
    
    public void SpawnMusicalNote(Vector2 position)
    {
        GameObject note = GetFromPool(notePool);
        if (note == null) return;
        
        RectTransform rt = note.GetComponent<RectTransform>();
        rt.anchoredPosition = position + new Vector2(Random.Range(-60f, 60f), Random.Range(-40f, 40f));
        note.SetActive(true);
        
        ParticleInstance instance = new ParticleInstance
        {
            obj = note,
            lifetime = 1.5f,
            age = 0f,
            velocity = new Vector3(Random.Range(-50f, 50f), Random.Range(50f, 150f), 0),
            type = ParticleType.MusicalNote
        };
        activeParticles.Add(instance);
    }
    
    public void SpawnIcon(ParticleType iconType, Vector2 position)
    {
        GameObject icon = GetFromPool(iconPool);
        if (icon == null) return;
        
        UnityEngine.UI.Text txt = icon.GetComponent<UnityEngine.UI.Text>();
        switch (iconType)
        {
            case ParticleType.Question:
                txt.text = "?";
                txt.color = new Color(1f, 1f, 0.4f, 0.95f);
                break;
            case ParticleType.Exclamation:
                txt.text = "!";
                txt.color = new Color(1f, 0.3f, 0.3f, 0.95f);
                break;
            case ParticleType.Heart:
                txt.text = "♥";
                txt.color = new Color(1f, 0.4f, 0.6f, 0.95f);
                break;
        }
        
        RectTransform rt = icon.GetComponent<RectTransform>();
        rt.anchoredPosition = position + new Vector2(0, 200f);
        icon.SetActive(true);
        
        ParticleInstance instance = new ParticleInstance
        {
            obj = icon,
            lifetime = 2f,
            age = 0f,
            velocity = new Vector3(0, 50f, 0),
            type = iconType
        };
        activeParticles.Add(instance);
    }
    
    GameObject GetFromPool(List<GameObject> pool)
    {
        foreach (GameObject obj in pool)
        {
            if (!obj.activeInHierarchy)
                return obj;
        }
        return null;  // Pool exhausted
    }
    
    public void ClearAllParticles()
    {
        foreach (var particle in activeParticles)
        {
            particle.obj.SetActive(false);
        }
        activeParticles.Clear();
    }
}
