using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Modern chat UI system for Slime virtual pet interaction.
/// Provides attractive, user-friendly interface with Feed, Share, and messaging.
/// </summary>
public class ChatUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField messageInput;
    [SerializeField] private Button sendButton;
    [SerializeField] private Button feedButton;
    [SerializeField] private Button shareButton;
    
    [Header("Visual Feedback")]
    [SerializeField] private Image sendButtonImage;
    [SerializeField] private Color sendActiveColor = new Color(0.29f, 0.56f, 0.89f, 1f); // #4A90E2
    [SerializeField] private Color sendDisabledColor = new Color(0.8f, 0.8f, 0.8f, 1f); // Gray
    
    [Header("Animation Settings")]
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float pressScale = 0.97f;
    [SerializeField] private float animationSpeed = 0.15f;
    
    [Header("Character Limit")]
    [SerializeField] private int maxCharacters = 200;
    [SerializeField] private TextMeshProUGUI characterCountText;
    
    [Header("Integration")]
    [SerializeField] private UltimateLivingSlime livingSlime;
    
    // State tracking
    private bool isInputFocused = false;
    private Coroutine buttonAnimationCoroutine;
    
    void Start()
    {
        InitializeUI();
        SetupEventListeners();
        UpdateSendButtonState();
        
        // Find living slime if not assigned
        if (livingSlime == null)
        {
            livingSlime = FindObjectOfType<UltimateLivingSlime>();
        }
    }
    
    void InitializeUI()
    {
        // Set input field character limit
        if (messageInput != null)
        {
            messageInput.characterLimit = maxCharacters;
            messageInput.placeholder.GetComponent<TextMeshProUGUI>().text = "Say something to Slime...";
        }
        
        // Initialize send button state
        if (sendButton != null)
        {
            sendButton.interactable = false;
        }
        
        Debug.Log("ChatUIManager: Initialized successfully!");
    }
    
    void SetupEventListeners()
    {
        // Input field listeners
        if (messageInput != null)
        {
            messageInput.onValueChanged.AddListener(OnInputChanged);
            messageInput.onSelect.AddListener(OnInputFocused);
            messageInput.onDeselect.AddListener(OnInputUnfocused);
            messageInput.onSubmit.AddListener(OnInputSubmit);
        }
        
        // Button listeners
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }
        
        if (feedButton != null)
        {
            feedButton.onClick.AddListener(OnFeedClicked);
        }
        
        if (shareButton != null)
        {
            shareButton.onClick.AddListener(OnShareClicked);
        }
    }
    
    void Update()
    {
        // Handle Enter key for sending
        if (isInputFocused && Input.GetKeyDown(KeyCode.Return) && sendButton.interactable)
        {
            OnSendClicked();
        }
        
        // Update character count if displayed
        if (characterCountText != null && messageInput != null)
        {
            int remaining = maxCharacters - messageInput.text.Length;
            characterCountText.text = $"{remaining}";
            
            // Color feedback for character limit
            if (remaining < 20)
            {
                characterCountText.color = new Color(0.9f, 0.3f, 0.3f); // Red warning
            }
            else
            {
                characterCountText.color = new Color(0.63f, 0.68f, 0.75f); // Light gray
            }
        }
    }
    
    #region Input Field Events
    
    void OnInputChanged(string text)
    {
        UpdateSendButtonState();
    }
    
    void OnInputFocused(string text)
    {
        isInputFocused = true;
        Debug.Log("ChatUIManager: Input focused");
    }
    
    void OnInputUnfocused(string text)
    {
        isInputFocused = false;
        Debug.Log("ChatUIManager: Input unfocused");
    }
    
    void OnInputSubmit(string text)
    {
        if (sendButton.interactable)
        {
            OnSendClicked();
        }
    }
    
    void UpdateSendButtonState()
    {
        if (messageInput == null || sendButton == null) return;
        
        bool hasText = !string.IsNullOrWhiteSpace(messageInput.text);
        sendButton.interactable = hasText;
        
        // Update visual state
        if (sendButtonImage != null)
        {
            sendButtonImage.color = hasText ? sendActiveColor : sendDisabledColor;
        }
    }
    
    #endregion
    
    #region Button Click Events
    
    public void OnSendClicked()
    {
        if (messageInput == null || string.IsNullOrWhiteSpace(messageInput.text))
        {
            return;
        }
        
        string message = messageInput.text.Trim();
        Debug.Log($"ChatUIManager: Sending message: '{message}'");
        
        // Animate button press
        StartCoroutine(AnimateButtonPress(sendButton.transform));
        
        // Process message
        ProcessUserMessage(message);
        
        // Clear input
        messageInput.text = "";
        UpdateSendButtonState();
        
        // Optional: Play send sound effect
        PlaySendSound();
    }
    
    public void OnFeedClicked()
    {
        Debug.Log("ChatUIManager: Feed button clicked");
        
        // Animate button press
        if (feedButton != null)
        {
            StartCoroutine(AnimateButtonPress(feedButton.transform));
        }
        
        // Call living slime feed method
        if (livingSlime != null)
        {
            livingSlime.Feed();
            ShowFeedbackMessage("🍪 Yummy! Slime is happy!");
        }
        else
        {
            Debug.LogWarning("ChatUIManager: LivingSlime reference not found! Make sure UltimateLivingSlime is in the scene.");
            ShowFeedbackMessage("⚠️ Slime not found!");
        }
        
        // Optional: Play feed sound effect
        PlayFeedSound();
    }
    
    public void OnShareClicked()
    {
        Debug.Log("ChatUIManager: Share button clicked");
        
        // Animate button press
        if (shareButton != null)
        {
            StartCoroutine(AnimateButtonPress(shareButton.transform));
        }
        
        // Implement share functionality (screenshot, social share, etc.)
        ShareSlimeStatus();
        
        // Optional: Play share sound effect
        PlayShareSound();
    }
    
    #endregion
    
    #region Message Processing
    
    void ProcessUserMessage(string message)
    {
        // Here you can add NLP, keyword detection, or AI responses
        // For now, simple keyword-based reactions
        
        string lowerMessage = message.ToLower();
        
        // Detect greetings
        if (lowerMessage.Contains("hello") || lowerMessage.Contains("hi") || lowerMessage.Contains("hey"))
        {
            if (livingSlime != null)
            {
                livingSlime.SetEmotionPreset(UltimateLivingSlime.EmotionPreset.Happy);
                livingSlime.GiveAttention();
            }
            ShowFeedbackMessage("Slime is happy to see you! 😊");
        }
        // Detect praise
        else if (lowerMessage.Contains("good") || lowerMessage.Contains("cute") || lowerMessage.Contains("love"))
        {
            if (livingSlime != null)
            {
                livingSlime.SetEmotionPreset(UltimateLivingSlime.EmotionPreset.Affectionate);
                livingSlime.GiveAttention();
            }
            ShowFeedbackMessage("Slime loves you too! 💙");
        }
        // Detect questions
        else if (lowerMessage.Contains("?") || lowerMessage.Contains("what") || lowerMessage.Contains("how"))
        {
            if (livingSlime != null)
            {
                livingSlime.SetEmotionPreset(UltimateLivingSlime.EmotionPreset.Curious);
            }
            ShowFeedbackMessage("Slime is curious! 🤔");
        }
        // Detect sadness
        else if (lowerMessage.Contains("sad") || lowerMessage.Contains("cry") || lowerMessage.Contains("sorry"))
        {
            if (livingSlime != null)
            {
                livingSlime.SetEmotionPreset(UltimateLivingSlime.EmotionPreset.Sad);
            }
            ShowFeedbackMessage("Slime feels your emotion... 😢");
        }
        // Detect excitement
        else if (lowerMessage.Contains("!") || lowerMessage.Contains("wow") || lowerMessage.Contains("yay"))
        {
            if (livingSlime != null)
            {
                livingSlime.SetEmotionPreset(UltimateLivingSlime.EmotionPreset.Excited);
            }
            ShowFeedbackMessage("Slime is excited! 🎉");
        }
        // Generic response
        else
        {
            if (livingSlime != null)
            {
                livingSlime.GiveAttention();
            }
            ShowFeedbackMessage("Slime heard you! 👂");
        }
    }
    
    void ShowFeedbackMessage(string message)
    {
        // Display feedback message (you can implement a toast/notification system)
        Debug.Log($"Feedback: {message}");
        StartCoroutine(ShowTemporaryFeedback(message));
    }
    
    IEnumerator ShowTemporaryFeedback(string message)
    {
        // TODO: Implement visual feedback (floating text, toast notification, etc.)
        // For now, just log
        yield return new WaitForSeconds(2f);
    }
    
    #endregion
    
    #region Share Functionality
    
    void ShareSlimeStatus()
    {
        if (livingSlime == null)
        {
            Debug.LogWarning("ChatUIManager: Cannot share - LivingSlime not found");
            return;
        }
        
        // Create shareable text
        string shareText = $"My Slime is feeling {livingSlime.currentEmotionName}! 💙\n" +
                          $"Happiness: {Mathf.RoundToInt(livingSlime.happinessMeter * 100)}%\n" +
                          $"Energy: {Mathf.RoundToInt(livingSlime.currentEnergy * 100)}%\n" +
                          $"Relationship Level: {Mathf.RoundToInt(livingSlime.relationshipLevel)}";
        
        Debug.Log($"Share Text:\n{shareText}");
        
        // Copy to clipboard
        GUIUtility.systemCopyBuffer = shareText;
        
        ShowFeedbackMessage("✓ Status copied to clipboard!");
        
        // TODO: Implement native share on mobile platforms
        // #if UNITY_ANDROID || UNITY_IOS
        //     NativeShare.Share(shareText, captureScreenshot: true);
        // #endif
    }
    
    #endregion
    
    #region Button Animations
    
    IEnumerator AnimateButtonPress(Transform buttonTransform)
    {
        if (buttonTransform == null) yield break;
        
        Vector3 originalScale = buttonTransform.localScale;
        Vector3 pressedScale = originalScale * pressScale;
        
        // Press down
        float elapsed = 0f;
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            buttonTransform.localScale = Vector3.Lerp(originalScale, pressedScale, t);
            yield return null;
        }
        
        // Release back
        elapsed = 0f;
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            buttonTransform.localScale = Vector3.Lerp(pressedScale, originalScale, t);
            yield return null;
        }
        
        buttonTransform.localScale = originalScale;
    }
    
    public void OnButtonHoverEnter(Button button)
    {
        if (button != null && button.interactable)
        {
            if (buttonAnimationCoroutine != null)
            {
                StopCoroutine(buttonAnimationCoroutine);
            }
            buttonAnimationCoroutine = StartCoroutine(AnimateButtonScale(button.transform, hoverScale));
        }
    }
    
    public void OnButtonHoverExit(Button button)
    {
        if (button != null)
        {
            if (buttonAnimationCoroutine != null)
            {
                StopCoroutine(buttonAnimationCoroutine);
            }
            buttonAnimationCoroutine = StartCoroutine(AnimateButtonScale(button.transform, 1f));
        }
    }
    
    IEnumerator AnimateButtonScale(Transform buttonTransform, float targetScale)
    {
        if (buttonTransform == null) yield break;
        
        Vector3 startScale = buttonTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        
        float elapsed = 0f;
        while (elapsed < animationSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationSpeed;
            buttonTransform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        
        buttonTransform.localScale = endScale;
    }
    
    #endregion
    
    #region Sound Effects
    
    void PlaySendSound()
    {
        // TODO: Implement sound effect playback
        // AudioManager.Instance?.PlaySound("SendMessage");
    }
    
    void PlayFeedSound()
    {
        // TODO: Implement sound effect playback
        // AudioManager.Instance?.PlaySound("Feed");
    }
    
    void PlayShareSound()
    {
        // TODO: Implement sound effect playback
        // AudioManager.Instance?.PlaySound("Share");
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Programmatically send a message (useful for testing or AI responses)
    /// </summary>
    public void SendChatMessage(string message)
    {
        if (messageInput != null)
        {
            messageInput.text = message;
            OnSendClicked();
        }
    }
    
    /// <summary>
    /// Enable or disable the entire chat UI
    /// </summary>
    public void SetChatUIEnabled(bool enabled)
    {
        gameObject.SetActive(enabled);
    }
    
    #endregion
}
