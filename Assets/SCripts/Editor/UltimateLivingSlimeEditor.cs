using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for UltimateLivingSlime with professional emotion testing UI
/// Includes duration locking, visual feedback, and comprehensive debug display
/// </summary>
[CustomEditor(typeof(UltimateLivingSlime))]
public class UltimateLivingSlimeEditor : Editor
{
    private UltimateLivingSlime slime;
    private GUIStyle headerStyle;
    private GUIStyle lockStyle;
    private GUIStyle activeEmotionStyle;
    private bool showDebugInfo = true;
    private bool showPersonalityFoldout = true;
    private bool showBreathingFoldout = false;
    private bool showEnergyFoldout = false;
    
    private void OnEnable()
    {
        slime = (UltimateLivingSlime)target;
    }
    
    public override void OnInspectorGUI()
    {
        // Initialize styles
        InitializeStyles();
        
        serializedObject.Update();
        
        // Header
        EditorGUILayout.Space(10);
        GUILayout.Label("🌟 ULTIMATE LIVING SLIME 🌟", headerStyle);
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Biologically-Inspired Emotional Intelligence System", EditorStyles.centeredGreyMiniLabel);
        EditorGUILayout.Space(10);
        
        // Warning if missing references
        DrawReferencesSection();
        
        EditorGUILayout.Space(10);
        
        // Main Emotion Control
        DrawEmotionControlSection();
        
        EditorGUILayout.Space(10);
        
        // Personality Traits
        DrawPersonalitySection();
        
        EditorGUILayout.Space(10);
        
        // Debug Information
        DrawDebugSection();
        
        EditorGUILayout.Space(10);
        
        // Advanced Settings
        DrawAdvancedSettingsSection();
        
        serializedObject.ApplyModifiedProperties();
        
        // Repaint continuously when playing to update timers
        if (Application.isPlaying && slime.emotionLockTimer > 0)
        {
            Repaint();
        }
    }
    
    void InitializeStyles()
    {
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.alignment = TextAnchor.MiddleCenter;
            headerStyle.fontSize = 16;
            headerStyle.normal.textColor = new Color(0.3f, 0.8f, 1f);
        }
        
        if (lockStyle == null)
        {
            lockStyle = new GUIStyle(EditorStyles.helpBox);
            lockStyle.normal.textColor = new Color(1f, 0.5f, 0.3f);
            lockStyle.fontSize = 12;
            lockStyle.fontStyle = FontStyle.Bold;
            lockStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        if (activeEmotionStyle == null)
        {
            activeEmotionStyle = new GUIStyle(EditorStyles.boldLabel);
            activeEmotionStyle.fontSize = 14;
            activeEmotionStyle.normal.textColor = new Color(0.3f, 1f, 0.5f);
        }
    }
    
    void DrawReferencesSection()
    {
        if (slime.slimeController == null || slime.spriteAnimationManager == null)
        {
            EditorGUILayout.HelpBox("⚠️ Missing References! Auto-initialization will attempt to find them at runtime.", MessageType.Warning);
        }
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("slimeController"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spriteAnimationManager"));
    }
    
    void DrawEmotionControlSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("🎭 EMOTION CONTROL CENTER", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(5);
        
        // Current emotion display with visual feedback
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Current Emotion:", GUILayout.Width(120));
        
        if (Application.isPlaying)
        {
            // Animated label during play mode
            Color emotionColor = GetEmotionColor(slime.currentEmotionName);
            GUI.color = emotionColor;
            GUILayout.Label(slime.currentEmotionName, activeEmotionStyle);
            GUI.color = Color.white;
        }
        else
        {
            GUILayout.Label("(Not Playing)", EditorStyles.centeredGreyMiniLabel);
        }
        EditorGUILayout.EndHorizontal();
        
        // Duration lock indicator
        if (Application.isPlaying && slime.emotionLockTimer > 0)
        {
            EditorGUILayout.Space(5);
            
            // Progress bar
            float progress = 1f - (slime.emotionLockTimer / slime.minimumEmotionDuration);
            Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(progressRect, progress, 
                $"🔒 LOCKED: {slime.emotionLockTimer:F1}s remaining");
            
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Emotion is locked during duration. Cannot change until timer expires.", 
                EditorStyles.centeredGreyMiniLabel);
        }
        else if (Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("✅ Ready to change emotion", EditorStyles.centeredGreyMiniLabel);
        }
        
        EditorGUILayout.Space(10);
        
        // Duration settings
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("minimumEmotionDuration"), 
            new GUIContent("Duration (seconds)"));
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("lockEmotionDuringDuration"), 
            new GUIContent("Lock Dropdown During Duration"));
        
        EditorGUILayout.Space(5);
        
        // Emotion preset dropdown
        GUI.enabled = Application.isPlaying ? slime.canChangeEmotion : true;
        
        EditorGUI.BeginChangeCheck();
        var emotionProperty = serializedObject.FindProperty("testEmotionPreset");
        EditorGUILayout.PropertyField(emotionProperty, new GUIContent("Select Emotion"));
        
        if (EditorGUI.EndChangeCheck() && Application.isPlaying)
        {
            // Emotion changed in inspector during play - will be handled by Update()
            serializedObject.ApplyModifiedProperties();
        }
        
        GUI.enabled = true;
        
        // Quick emotion buttons (only when unlocked)
        if (Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("Quick Select:", EditorStyles.miniBoldLabel);
            
            GUI.enabled = slime.canChangeEmotion;
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("😊 Happy")) SetEmotion(UltimateLivingSlime.EmotionPreset.Happy);
            if (GUILayout.Button("😢 Sad")) SetEmotion(UltimateLivingSlime.EmotionPreset.Sad);
            if (GUILayout.Button("😡 Angry")) SetEmotion(UltimateLivingSlime.EmotionPreset.Angry);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("😨 Scared")) SetEmotion(UltimateLivingSlime.EmotionPreset.Scared);
            if (GUILayout.Button("🎉 Excited")) SetEmotion(UltimateLivingSlime.EmotionPreset.Excited);
            if (GUILayout.Button("😴 Tired")) SetEmotion(UltimateLivingSlime.EmotionPreset.Tired);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("🤔 Curious")) SetEmotion(UltimateLivingSlime.EmotionPreset.Curious);
            if (GUILayout.Button("😳 Shy")) SetEmotion(UltimateLivingSlime.EmotionPreset.Shy);
            if (GUILayout.Button("😌 Content")) SetEmotion(UltimateLivingSlime.EmotionPreset.Content);
            EditorGUILayout.EndHorizontal();
            
            GUI.enabled = true;
            
            // Emergency stop button
            EditorGUILayout.Space(5);
            GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
            if (GUILayout.Button("⏹ FORCE STOP (Reset to Neutral)"))
            {
                slime.SetEmotionPreset(UltimateLivingSlime.EmotionPreset.Neutral);
                slime.emotionLockTimer = 0f;
                slime.canChangeEmotion = true;
            }
            GUI.backgroundColor = Color.white;
        }
        else
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox("▶️ Enter Play Mode to test emotion system with duration locking", MessageType.Info);
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("emotionTransitionSpeed"), 
            new GUIContent("Transition Speed"));
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawPersonalitySection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showPersonalityFoldout = EditorGUILayout.Foldout(showPersonalityFoldout, 
            "🧬 PERSONALITY TRAITS (Individuality System)", true, EditorStyles.foldoutHeader);
        
        if (showPersonalityFoldout)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Adjust traits to create unique personality", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.Space(5);
            
            var personalityProp = serializedObject.FindProperty("personality");
            
            DrawPersonalitySlider(personalityProp.FindPropertyRelative("extroversion"), 
                "Extroversion", "Shy", "Social");
            DrawPersonalitySlider(personalityProp.FindPropertyRelative("sensitivity"), 
                "Sensitivity", "Calm", "Reactive");
            DrawPersonalitySlider(personalityProp.FindPropertyRelative("curiosity"), 
                "Curiosity", "Passive", "Inquisitive");
            DrawPersonalitySlider(personalityProp.FindPropertyRelative("affection"), 
                "Affection", "Distant", "Loving");
            DrawPersonalitySlider(personalityProp.FindPropertyRelative("energyLevel"), 
                "Energy Level", "Lazy", "Hyperactive");
            
            EditorGUILayout.Space(5);
            
            // Preset personality buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Cheerful")) SetPersonalityPreset(0.7f, 0.4f, 0.6f, 0.8f, 0.8f);
            if (GUILayout.Button("Shy")) SetPersonalityPreset(0.2f, 0.7f, 0.5f, 0.6f, 0.4f);
            if (GUILayout.Button("Energetic")) SetPersonalityPreset(0.8f, 0.5f, 0.8f, 0.7f, 0.9f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Calm")) SetPersonalityPreset(0.5f, 0.3f, 0.4f, 0.6f, 0.3f);
            if (GUILayout.Button("Curious")) SetPersonalityPreset(0.6f, 0.5f, 0.9f, 0.5f, 0.6f);
            if (GUILayout.Button("Balanced")) SetPersonalityPreset(0.5f, 0.5f, 0.5f, 0.5f, 0.5f);
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawDebugSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, 
            "🔍 DEBUG INFORMATION", true, EditorStyles.foldoutHeader);
        
        if (showDebugInfo && Application.isPlaying)
        {
            EditorGUILayout.Space(5);
            
            // Emotional state vector display
            EditorGUILayout.LabelField("Emotional State Vector4:", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            DrawColoredValue("Valence", slime.currentEmotion.valence, -1f, 1f);
            DrawColoredValue("Arousal", slime.currentEmotion.arousal, 0f, 1f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            DrawColoredValue("Dominance", slime.currentEmotion.dominance, 0f, 1f);
            DrawColoredValue("Engagement", slime.currentEmotion.engagement, 0f, 1f);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Intensity display
            Rect intensityRect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(intensityRect, slime.currentEmotion.intensity, 
                $"💪 Intensity: {slime.currentEmotion.intensity:F2}");
            
            EditorGUILayout.Space(5);
            
            // Energy display
            Color energyColor = slime.currentEnergy > 0.5f ? Color.green : 
                               slime.currentEnergy > 0.3f ? Color.yellow : Color.red;
            GUI.color = energyColor;
            Rect energyRect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(energyRect, slime.currentEnergy, 
                $"⚡ Energy: {slime.currentEnergy:F2}");
            GUI.color = Color.white;
            
            EditorGUILayout.Space(5);
            
            // Breathing phase
            float breathProgress = slime.breathPhase;
            string breathPhaseLabel = breathProgress < 0.5f ? "Inhale" : "Exhale";
            Rect breathRect = EditorGUILayout.GetControlRect(false, 20);
            EditorGUI.ProgressBar(breathRect, breathProgress, 
                $"🫁 Breath: {breathPhaseLabel} ({breathProgress:F2})");
            
            EditorGUILayout.Space(5);
            
            // Timers
            EditorGUILayout.LabelField($"⏱️ Emotion Timer: {slime.emotionTimer:F1}s", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"🔒 Lock Timer: {slime.emotionLockTimer:F1}s", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"✅ Can Change: {slime.canChangeEmotion}", EditorStyles.miniLabel);
        }
        else if (!Application.isPlaying)
        {
            EditorGUILayout.LabelField("Debug info available during Play Mode", EditorStyles.centeredGreyMiniLabel);
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawAdvancedSettingsSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        
        GUILayout.Label("⚙️ ADVANCED SETTINGS", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(5);
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enableIdleMicroAnimations"), 
            new GUIContent("Enable Idle Micro-Animations"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("breathRate"), 
            new GUIContent("Manual Breath Rate Override"));
        
        if (Application.isPlaying)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("currentEnergy"), 
                new GUIContent("Current Energy (Read-Only)"));
        }
        
        EditorGUILayout.EndVertical();
    }
    
    void DrawPersonalitySlider(SerializedProperty prop, string label, string minLabel, string maxLabel)
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(100));
        GUILayout.Label(minLabel, EditorStyles.miniLabel, GUILayout.Width(60));
        EditorGUILayout.PropertyField(prop, GUIContent.none);
        GUILayout.Label(maxLabel, EditorStyles.miniLabel, GUILayout.Width(80));
        EditorGUILayout.EndHorizontal();
    }
    
    void DrawColoredValue(string label, float value, float min, float max)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        
        float normalized = (value - min) / (max - min);
        Color valueColor = Color.Lerp(new Color(0.5f, 0.5f, 1f), new Color(1f, 0.5f, 0.5f), normalized);
        
        GUI.color = valueColor;
        EditorGUILayout.LabelField($"{value:F2}", EditorStyles.boldLabel);
        GUI.color = Color.white;
        
        EditorGUILayout.EndVertical();
    }
    
    Color GetEmotionColor(string emotionName)
    {
        switch (emotionName)
        {
            case "Happy": case "Excited": case "Playful": return new Color(1f, 0.8f, 0.2f);
            case "Sad": case "Lonely": case "Tired": return new Color(0.4f, 0.6f, 1f);
            case "Angry": return new Color(1f, 0.3f, 0.3f);
            case "Scared": case "Worried": return new Color(0.8f, 0.5f, 1f);
            case "Curious": case "Content": return new Color(0.3f, 1f, 0.6f);
            case "Shy": case "Embarrassed": return new Color(1f, 0.6f, 0.8f);
            default: return Color.white;
        }
    }
    
    void SetEmotion(UltimateLivingSlime.EmotionPreset preset)
    {
        if (Application.isPlaying && slime.canChangeEmotion)
        {
            slime.SetEmotionPreset(preset);
            serializedObject.FindProperty("testEmotionPreset").enumValueIndex = (int)preset;
            serializedObject.ApplyModifiedProperties();
        }
    }
    
    void SetPersonalityPreset(float ext, float sens, float cur, float aff, float ener)
    {
        var personalityProp = serializedObject.FindProperty("personality");
        personalityProp.FindPropertyRelative("extroversion").floatValue = ext;
        personalityProp.FindPropertyRelative("sensitivity").floatValue = sens;
        personalityProp.FindPropertyRelative("curiosity").floatValue = cur;
        personalityProp.FindPropertyRelative("affection").floatValue = aff;
        personalityProp.FindPropertyRelative("energyLevel").floatValue = ener;
        serializedObject.ApplyModifiedProperties();
    }
}
