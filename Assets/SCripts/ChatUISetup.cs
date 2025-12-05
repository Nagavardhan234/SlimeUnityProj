using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Automatically creates the complete Chat UI system with all visual elements.
/// Just attach this to an empty GameObject and run - it creates everything!
/// </summary>
public class ChatUISetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [SerializeField] private bool createUIOnStart = true;
    [SerializeField] private Canvas targetCanvas;
    
    private ChatUIManager chatUIManager;
    
    void Start()
    {
        if (createUIOnStart)
        {
            // Wait one frame to ensure SlimeController's Awake() has created the canvas
            StartCoroutine(CreateUIAfterFrame());
        }
    }
    
    System.Collections.IEnumerator CreateUIAfterFrame()
    {
        yield return null; // Wait one frame
        CreateCompleteUI();
    }
    
    [ContextMenu("Create Chat UI")]
    public void CreateCompleteUI()
    {
        Debug.Log("============ ChatUISetup: Starting RESPONSIVE UI Creation ============");
        Debug.Log($"ChatUISetup: Screen size: {Screen.width}x{Screen.height}");
        
        // Check for SlimeController FIRST
        SlimeController slimeCheck = FindObjectOfType<SlimeController>();
        if (slimeCheck == null)
        {
            Debug.LogError("ChatUISetup: ⚠️ CRITICAL - No SlimeController found in scene!");
            Debug.LogError("ChatUISetup: Please add a GameObject with SlimeController component to the scene first.");
            Debug.LogError("ChatUISetup: This is required for the canvas setup.");
            return;
        }
        else
        {
            Debug.Log($"ChatUISetup: ✓ Found SlimeController on GameObject: {slimeCheck.gameObject.name}");
        }
        
        // CRITICAL FIX: Destroy old UI containers before creating new one
        GameObject existingContainer = GameObject.Find("ChatUI_Container");
        if (existingContainer != null)
        {
            DestroyImmediate(existingContainer);
            Debug.Log("ChatUISetup: ✓ Destroyed existing ChatUI_Container");
        }
        
        // Also search for any containers in the canvas
        Canvas[] allCanvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canv in allCanvases)
        {
            Transform oldUI = canv.transform.Find("ChatUI_Container");
            if (oldUI != null)
            {
                DestroyImmediate(oldUI.gameObject);
                Debug.Log("ChatUISetup: Destroyed old UI in canvas");
            }
        }
        
        // CRITICAL FIX: Ensure EventSystem exists for UI interactions
        EnsureEventSystem();
        
        // Find or create canvas
        if (targetCanvas == null)
        {
            targetCanvas = FindOrCreateCanvas();
        }
        
        // Create main container
        GameObject container = CreateMainContainer(targetCanvas);
        
        // Create top buttons (Feed & Share)
        GameObject topButtonsGroup = CreateTopButtonsGroup(container);
        GameObject feedButton = CreateFeedButton(topButtonsGroup);
        GameObject shareButton = CreateShareButton(topButtonsGroup);
        
        // Create chat bar (Input & Send)
        GameObject chatBarGroup = CreateChatBarGroup(container);
        GameObject inputField = CreateInputField(chatBarGroup);
        GameObject sendButton = CreateSendButton(chatBarGroup);
        
        // Create and setup ChatUIManager
        chatUIManager = container.AddComponent<ChatUIManager>();
        SetupChatUIManager(chatUIManager, inputField, sendButton, feedButton, shareButton);
        
        // Add hover effects to buttons
        feedButton.AddComponent<ButtonHoverEffect>();
        shareButton.AddComponent<ButtonHoverEffect>();
        sendButton.AddComponent<ButtonHoverEffect>();
        
        // CRITICAL FIX: Set as last sibling to render ON TOP of slime
        container.transform.SetAsLastSibling();
        
        #if UNITY_EDITOR
        // Force Unity to refresh and recalculate layout in edit mode
        UnityEditor.EditorUtility.SetDirty(container);
        UnityEditor.EditorUtility.SetDirty(targetCanvas.gameObject);
        Canvas.ForceUpdateCanvases(); // Force immediate canvas recalculation
        Debug.Log($"ChatUISetup: ✓ UI created with RESPONSIVE canvas: {targetCanvas.GetComponent<CanvasScaler>().referenceResolution}");
        Debug.Log($"ChatUISetup: ✓ Container size: {container.GetComponent<RectTransform>().sizeDelta}");
        #endif
        
        Debug.Log("ChatUISetup: ✓ Complete! Chat UI created successfully with RESPONSIVE ADAPTIVE DESIGN.");
    }
    
    void EnsureEventSystem()
    {
        // CRITICAL FIX: Create EventSystem if it doesn't exist (required for all UI interactions)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("ChatUISetup: ✓ Created EventSystem for UI interactions");
        }
        else
        {
            Debug.Log("ChatUISetup: ✓ EventSystem already exists");
        }
    }
    
    Canvas FindOrCreateCanvas()
    {
        // First try to find SlimeController and get its canvas
        SlimeController slimeController = FindObjectOfType<SlimeController>();
        Canvas canvas = null;
        
        if (slimeController != null)
        {
            // Get canvas from SlimeController's GameObject or children
            canvas = slimeController.GetComponentInChildren<Canvas>();
            if (canvas == null)
            {
                canvas = slimeController.GetComponent<Canvas>();
            }
            if (canvas != null)
            {
                Debug.Log($"ChatUISetup: ✓ Found canvas from SlimeController: {canvas.gameObject.name}");
            }
        }
        
        // Fallback: search all canvases
        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
        }
        
        if (canvas == null)
        {
            // This should not happen if SlimeController is in scene (it creates canvas in Awake)
            Debug.LogError("ChatUISetup: CRITICAL - No Canvas found! " +
                          "SlimeController must be in the scene. Check your Hierarchy for SlimeController component.");
            
            // Emergency fallback: create canvas with portrait resolution to match SlimeController
            Debug.LogWarning("ChatUISetup: Creating emergency canvas as fallback...");
            GameObject canvasObj = new GameObject("ChatUI_EmergencyCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // Setup canvas scaler matching SlimeController's portrait resolution (1080x1920)
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // Landscape reference
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            Debug.LogWarning("ChatUISetup: Emergency canvas created. Slime visuals may not work correctly!");
        }
        else
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                // Ensure consistent responsive scaler settings
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0.5f;
                
                #if UNITY_EDITOR
                // Force canvas scaler to recalculate in edit mode
                UnityEditor.EditorUtility.SetDirty(canvas);
                UnityEditor.EditorUtility.SetDirty(scaler);
                #endif
                
                Debug.Log($"ChatUISetup: ✓ Found existing canvas with resolution {scaler.referenceResolution}");
            }
            else
            {
                Debug.Log("ChatUISetup: ✓ Found existing canvas (no scaler)");
            }
        }
        return canvas;
    }
    
    GameObject CreateMainContainer(Canvas canvas)
    {
        GameObject container = new GameObject("ChatUI_Container");
        container.transform.SetParent(canvas.transform, false);
        
        RectTransform rect = container.AddComponent<RectTransform>();
        
        // Anchor to bottom, stretch full width with pixel margins
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        
        rect.anchoredPosition = new Vector2(0, 40); // 40px from bottom
        // Calculate responsive height: 22% of reference height
        float containerHeight = 1080 * 0.22f; // 237.6px at reference resolution, scales automatically
        rect.sizeDelta = new Vector2(-80, containerHeight); // -80 = 40px margin each side
        
        // WHATSAPP: Semi-transparent light background
        Image containerBg = container.AddComponent<Image>();
        containerBg.color = new Color(0.95f, 0.95f, 0.97f, 0.85f); // Dark gray with transparency
        containerBg.sprite = CreateRoundedSprite();
        containerBg.type = Image.Type.Sliced;
        containerBg.raycastTarget = false;
        
        // Strong depth shadow
        Shadow containerShadow = container.AddComponent<Shadow>();
        containerShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        containerShadow.effectDistance = new Vector2(0, -8);
        
        // Bright subtle outline for definition
        Outline containerOutline = container.AddComponent<Outline>();
        containerOutline.effectColor = new Color(0.4f, 0.4f, 0.5f, 0.4f);
        containerOutline.effectDistance = new Vector2(3, -3);
        
        // Shaders disabled - using solid colors for reliability
        
        // EXPERT FIX: Strong shadow for depth
        Shadow shadow = container.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f); // 50% black for strong depth
        shadow.effectDistance = new Vector2(0, -6); // 6px offset
        shadow.useGraphicAlpha = false;
        
        // RESPONSIVE: Spacing and padding scale with container
        VerticalLayoutGroup vlg = container.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = Mathf.RoundToInt(containerHeight * 0.08f); // 8% of container height
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        int paddingH = Mathf.RoundToInt(containerHeight * 0.13f); // 13% horizontal
        int paddingV = Mathf.RoundToInt(containerHeight * 0.11f); // 11% vertical
        vlg.padding = new RectOffset(paddingH, paddingH, paddingV, paddingV);
        
        return container;
    }
    
    GameObject CreateTopButtonsGroup(GameObject parent)
    {
        GameObject group = new GameObject("TopButtons_Group");
        group.transform.SetParent(parent.transform, false);
        
        RectTransform rect = group.AddComponent<RectTransform>();
        // Responsive height: 50% of container height for input area
        float inputRowHeight = (1080 * 0.22f) * 0.50f;
        rect.sizeDelta = new Vector2(0, inputRowHeight);
        
        // Horizontal layout - CENTERED with responsive spacing
        HorizontalLayoutGroup hlg = group.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        float buttonSpacing = 1920 * 0.015f; // 1.5% of reference width
        hlg.spacing = Mathf.RoundToInt(buttonSpacing);
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        
        return group;
    }
    
    GameObject CreateFeedButton(GameObject parent)
    {
        GameObject button = new GameObject("FeedButton");
        button.transform.SetParent(parent.transform, false);
        
        // WORLD-CLASS: Huge button 240x95
        LayoutElement le = button.AddComponent<LayoutElement>();
        le.preferredWidth = 240;
        le.preferredHeight = 95;
        
        // Background with rounded corners
        Image img = button.AddComponent<Image>();
        img.sprite = CreateRoundedSprite();
        img.type = Image.Type.Sliced;
        
        // CHAMPIONSHIP: Rich vibrant orange
        img.color = new Color(1f, 0.6f, 0f, 1f); // #FF9900 Rich orange
        
        // Deep shadow for elevation
        Shadow btnShadow = button.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        btnShadow.effectDistance = new Vector2(0, -5);
        
        // Second shadow for more depth
        Shadow btnShadow2 = button.AddComponent<Shadow>();
        btnShadow2.effectColor = new Color(0f, 0f, 0f, 0.3f);
        btnShadow2.effectDistance = new Vector2(0, -10);
        
        // Bright outline for pop
        Outline btnOutline = button.AddComponent<Outline>();
        btnOutline.effectColor = new Color(1f, 0.8f, 0.4f, 0.4f); // Golden outline
        btnOutline.effectDistance = new Vector2(2, -2);
        
        // Button component
        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = img;
        
        // Hover effect only
        ButtonHoverEffect hoverEffect = button.AddComponent<ButtonHoverEffect>();
        
        // Text with icon
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Feed Slime";
        // Responsive font: 40% of button height
        text.fontSize = Mathf.RoundToInt(((1080 * 0.22f) * 0.38f) * 0.40f);
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        // Add strong shadow to text
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        textShadow.effectDistance = new Vector2(2, -2);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return button;
    }
    
    GameObject CreateShareButton(GameObject parent)
    {
        GameObject button = new GameObject("ShareButton");
        button.transform.SetParent(parent.transform, false);
        
        // WORLD-CLASS: Huge button 240x95
        LayoutElement le = button.AddComponent<LayoutElement>();
        le.preferredWidth = 240;
        le.preferredHeight = 95;
        
        // Background with rounded corners
        Image img = button.AddComponent<Image>();
        img.sprite = CreateRoundedSprite();
        img.type = Image.Type.Sliced;
        
        // CHAMPIONSHIP: Vibrant cyan
        img.color = new Color(0f, 0.75f, 0.85f, 1f); // #00BFD9 Rich cyan
        
        // Deep shadow for elevation
        Shadow btnShadow = button.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        btnShadow.effectDistance = new Vector2(0, -5);
        
        // Second shadow for more depth
        Shadow btnShadow2 = button.AddComponent<Shadow>();
        btnShadow2.effectColor = new Color(0f, 0f, 0f, 0.3f);
        btnShadow2.effectDistance = new Vector2(0, -10);
        
        // Bright outline for pop
        Outline btnOutline = button.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0.4f, 0.9f, 1f, 0.4f); // Cyan outline
        btnOutline.effectDistance = new Vector2(2, -2);
        
        // Button component
        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = img;
        
        // Hover effect only
        ButtonHoverEffect hoverEffect = button.AddComponent<ButtonHoverEffect>();
        
        // Text with icon
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Share";
        // Responsive font: 40% of button height
        text.fontSize = Mathf.RoundToInt(((1080 * 0.22f) * 0.38f) * 0.40f);
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        
        // Add strong shadow to text
        Shadow textShadow = textObj.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        textShadow.effectDistance = new Vector2(2, -2);
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        return button;
    }
    
    GameObject CreateChatBarGroup(GameObject parent)
    {
        GameObject group = new GameObject("ChatBar_Group");
        group.transform.SetParent(parent.transform, false);
        
        RectTransform rect = group.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0, 90); // PRO: 90px height
        
        // Horizontal layout - LEFT for input stretch
        HorizontalLayoutGroup hlg = group.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        float inputSpacing = 1920 * 0.01f; // 1% of reference width
        hlg.spacing = Mathf.RoundToInt(inputSpacing);
        hlg.childForceExpandHeight = true;
        hlg.childForceExpandWidth = false; // Don't force expand, let flexible width work
        hlg.childControlHeight = true;
        hlg.childControlWidth = true; // CRITICAL: Must be true for flexibleWidth to work
        
        return group;
    }
    
    GameObject CreateInputField(GameObject parent)
    {
        GameObject inputObj = new GameObject("MessageInput");
        inputObj.transform.SetParent(parent.transform, false);
        
        // RESPONSIVE: Input field scales with container - WIDE LIKE WHATSAPP
        LayoutElement le = inputObj.AddComponent<LayoutElement>();
        le.flexibleWidth = 10; // Much wider - takes up most space
        float inputHeightCalc = (1080 * 0.22f) * 0.48f; // 48% of container height
        le.preferredHeight = Mathf.RoundToInt(inputHeightCalc);
        
        // WHATSAPP: Bright white input with strong border
        Image img = inputObj.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 1f); // Pure white
        img.type = Image.Type.Sliced;
        img.sprite = CreateRoundedSprite();
        
        // Subtle inset shadow for depth
        Shadow inputShadow = inputObj.AddComponent<Shadow>();
        inputShadow.effectColor = new Color(0f, 0f, 0f, 0.12f);
        inputShadow.effectDistance = new Vector2(0, 2);
        
        // STRONG visible border like WhatsApp
        Outline inputOutline = inputObj.AddComponent<Outline>();
        inputOutline.effectColor = new Color(0.3f, 0.3f, 0.35f, 0.85f); // Dark visible border
        inputOutline.effectDistance = new Vector2(4, -4); // Thick border
        
        // Second outline for more depth
        Outline inputOutline2 = inputObj.AddComponent<Outline>();
        inputOutline2.effectColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);
        inputOutline2.effectDistance = new Vector2(2, -2);
        
        // Third outline for WhatsApp-style depth
        Outline inputOutline3 = inputObj.AddComponent<Outline>();
        inputOutline3.effectColor = new Color(0.6f, 0.6f, 0.65f, 0.5f);
        inputOutline3.effectDistance = new Vector2(1, -1);
        
        // Input field
        TMP_InputField input = inputObj.AddComponent<TMP_InputField>();
        input.characterLimit = 200;
        
        // Text area with better padding
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputObj.transform, false);
        RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
        textAreaRect.anchorMin = Vector2.zero;
        textAreaRect.anchorMax = Vector2.one;
        float paddingH = inputHeightCalc * 0.13f; // 13% horizontal padding
        float paddingV = inputHeightCalc * 0.13f; // 13% vertical padding
        textAreaRect.offsetMin = new Vector2(paddingH, paddingV);
        textAreaRect.offsetMax = new Vector2(-inputHeightCalc * 0.48f, -paddingV); // Leave space for emoji icon
        
        // EXPERT: Dark gray placeholder for contrast
        GameObject placeholder = new GameObject("Placeholder");
        placeholder.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
        placeholderText.text = "Type a message...";
        // Responsive font: 35% of input height
        placeholderText.fontSize = Mathf.RoundToInt(inputHeightCalc * 0.35f);
        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.7f); // WhatsApp gray
        placeholderText.fontStyle = FontStyles.Italic;
        
        RectTransform placeholderRect = placeholder.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.sizeDelta = Vector2.zero;
        
        // EXPERT: Pure black text for maximum readability
        GameObject text = new GameObject("Text");
        text.transform.SetParent(textArea.transform, false);
        TextMeshProUGUI inputText = text.AddComponent<TextMeshProUGUI>();
        // Responsive font: 38% of input height
        inputText.fontSize = Mathf.RoundToInt(inputHeightCalc * 0.38f);
        inputText.color = new Color(0.05f, 0.05f, 0.05f, 1f); // Deep black
        
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        
        // Emoji icon
        GameObject emoji = new GameObject("SlimeIcon");
        emoji.transform.SetParent(inputObj.transform, false);
        TextMeshProUGUI emojiText = emoji.AddComponent<TextMeshProUGUI>();
        emojiText.text = ":)";
        // Responsive font: 25% of input height
        emojiText.fontSize = Mathf.RoundToInt(inputHeightCalc * 0.25f);
        emojiText.alignment = TextAlignmentOptions.Center;
        
        RectTransform emojiRect = emoji.GetComponent<RectTransform>();
        emojiRect.anchorMin = new Vector2(1, 0.5f);
        emojiRect.anchorMax = new Vector2(1, 0.5f);
        emojiRect.pivot = new Vector2(1, 0.5f);
        float emojiOffset = inputHeightCalc * 0.09f; // 9% of input height
        float emojiSize = inputHeightCalc * 0.26f; // 26% of input height
        emojiRect.anchoredPosition = new Vector2(-emojiOffset, 0);
        emojiRect.sizeDelta = new Vector2(emojiSize, emojiSize);
        
        // Setup input field references
        input.textViewport = textAreaRect;
        input.textComponent = inputText;
        input.placeholder = placeholderText;
        
        return inputObj;
    }
    
    GameObject CreateSendButton(GameObject parent)
    {
        GameObject button = new GameObject("SendButton");
        button.transform.SetParent(parent.transform, false);
        
        // RESPONSIVE: Send button matches input height - FIXED SIZE (no flexible)
        LayoutElement le = button.AddComponent<LayoutElement>();
        float sendSize = (1080 * 0.22f) * 0.48f; // Match input height (48% of container)
        le.preferredWidth = Mathf.RoundToInt(sendSize);
        le.preferredHeight = Mathf.RoundToInt(sendSize);
        le.flexibleWidth = 0; // No flexible width - stay fixed size
        
        // Perfect circle background
        Image img = button.AddComponent<Image>();
        img.sprite = CreateCircleSprite();
        img.type = Image.Type.Simple; // Use Simple to prevent black rendering
        
        // CHAMPIONSHIP: Rich vibrant GREEN
        img.color = new Color(0.15f, 0.75f, 0.25f, 1f); // #26BF40 Rich green
        
        // Deep shadow for elevation
        Shadow btnShadow = button.AddComponent<Shadow>();
        btnShadow.effectColor = new Color(0f, 0f, 0f, 0.5f);
        btnShadow.effectDistance = new Vector2(0, -5);
        
        // Second shadow for more depth
        Shadow btnShadow2 = button.AddComponent<Shadow>();
        btnShadow2.effectColor = new Color(0f, 0f, 0f, 0.3f);
        btnShadow2.effectDistance = new Vector2(0, -10);
        
        // Bright green outline
        Outline btnOutline = button.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0.4f, 1f, 0.5f, 0.4f); // Bright green outline
        btnOutline.effectDistance = new Vector2(2, -2);
        
        // Button component
        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.interactable = false; // Starts disabled
        
        // Hover effect only
        ButtonHoverEffect hoverEffect = button.AddComponent<ButtonHoverEffect>();
        
        // Send icon (arrow)
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(button.transform, false);
        
        TextMeshProUGUI icon = iconObj.AddComponent<TextMeshProUGUI>();
        icon.text = ">"; // Simple greater-than arrow (always renders)
        // Responsive font: 50% of button size
        float sendIconSize = (1080 * 0.22f) * 0.48f;
        icon.fontSize = Mathf.RoundToInt(sendIconSize * 0.50f);
        icon.color = Color.white;
        icon.alignment = TextAlignmentOptions.Center;
        icon.fontStyle = FontStyles.Bold;
        
        // Add strong shadow to icon
        Shadow iconShadow = iconObj.AddComponent<Shadow>();
        iconShadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
        iconShadow.effectDistance = new Vector2(2, -2);
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        iconRect.sizeDelta = Vector2.zero;
        
        return button;
    }
    
    void SetupChatUIManager(ChatUIManager manager, GameObject input, GameObject send, GameObject feed, GameObject share)
    {
        // Use reflection to set private fields
        var type = typeof(ChatUIManager);
        
        type.GetField("messageInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, input.GetComponent<TMP_InputField>());
        
        type.GetField("sendButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, send.GetComponent<Button>());
        
        type.GetField("feedButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, feed.GetComponent<Button>());
        
        type.GetField("shareButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, share.GetComponent<Button>());
        
        type.GetField("sendButtonImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(manager, send.GetComponent<Image>());
        
        Debug.Log("ChatUIManager references set via reflection!");
    }
    
    // WORLD-CLASS: Create rounded rectangle with 40px radius
    Sprite CreateRoundedSprite()
    {
        int size = 100;
        int radius = 40; // Ultra smooth premium corners
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Check if pixel is within rounded rectangle
                bool inRounded = IsInRoundedRect(x, y, size, size, radius);
                pixels[y * size + x] = inRounded ? Color.white : Color.clear;
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }
    
    // Create a circle sprite
    Sprite CreateCircleSprite()
    {
        int size = 100;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100);
    }
    
    bool IsInRoundedRect(int x, int y, int width, int height, int radius)
    {
        // Check if point is in the rounded rectangle
        int left = radius;
        int right = width - radius;
        int top = height - radius;
        int bottom = radius;
        
        // Inside main rectangle
        if (x >= left && x < right && y >= bottom && y < top)
            return true;
        
        // Check corners
        if (x < left && y < bottom)
            return Vector2.Distance(new Vector2(x, y), new Vector2(left, bottom)) <= radius;
        if (x >= right && y < bottom)
            return Vector2.Distance(new Vector2(x, y), new Vector2(right, bottom)) <= radius;
        if (x < left && y >= top)
            return Vector2.Distance(new Vector2(x, y), new Vector2(left, top)) <= radius;
        if (x >= right && y >= top)
            return Vector2.Distance(new Vector2(x, y), new Vector2(right, top)) <= radius;
        
        // Extended rectangles
        if (x >= left && x < right)
            return true;
        if (y >= bottom && y < top)
            return true;
        
        return false;
    }
    
    Color HexToColor(string hex)
    {
        hex = hex.Replace("#", "");
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255);
    }
}
