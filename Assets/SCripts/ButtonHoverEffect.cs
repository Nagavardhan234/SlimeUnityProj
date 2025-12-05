using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Add hover effects to UI buttons (desktop only)
/// Attach this component to any button that needs hover animations
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ChatUIManager chatUIManager;
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
        
        // Find ChatUIManager if not assigned
        if (chatUIManager == null)
        {
            chatUIManager = FindObjectOfType<ChatUIManager>();
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (chatUIManager != null && button != null)
        {
            chatUIManager.OnButtonHoverEnter(button);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (chatUIManager != null && button != null)
        {
            chatUIManager.OnButtonHoverExit(button);
        }
    }
}
