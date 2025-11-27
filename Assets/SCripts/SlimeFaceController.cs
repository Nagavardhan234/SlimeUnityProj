using System.Collections;
using UnityEngine;

public class SlimeFaceController : MonoBehaviour
{
    [Header("Eye References")]
    [SerializeField] private Transform leftEye;
    [SerializeField] private Transform rightEye;
    [SerializeField] private float eyeSpacing = 0.3f;
    [SerializeField] private float eyeForwardOffset = 0.8f;
    
    [Header("Eye Appearance")]
    [SerializeField] private Vector3 eyeScale = new Vector3(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color eyeColor = Color.white;
    [SerializeField] private float eyeEmissionIntensity = 3f;
    
    [Header("Pupil Settings")]
    [SerializeField] private Transform leftPupil;
    [SerializeField] private Transform rightPupil;
    [SerializeField] private float pupilScale = 0.6f;
    [SerializeField] private Color pupilColor = new Color(0.1f, 0.1f, 0.1f);
    
    [Header("Blinking")]
    [SerializeField] private float blinkIntervalMin = 2f;
    [SerializeField] private float blinkIntervalMax = 5f;
    [SerializeField] private float blinkSpeed = 0.08f;
    [SerializeField] private float blinkScaleY = 0.1f;
    
    [Header("Look Behavior")]
    [SerializeField] private bool lookAtCamera = true;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private float lookSpeed = 3f;
    [SerializeField] private float maxLookAngle = 30f;
    [SerializeField] private float pupilMovementRange = 0.05f;
    
    [Header("Emotion States")]
    [SerializeField] private SlimeEmotion currentEmotion = SlimeEmotion.Happy;
    
    private Vector3 leftEyeOriginalScale;
    private Vector3 rightEyeOriginalScale;
    private float nextBlinkTime;
    private bool isBlinking = false;
    private Camera mainCamera;
    private Coroutine blinkCoroutine;
    
    public enum SlimeEmotion
    {
        Happy,
        Excited,
        Sleepy,
        Curious,
        Playful,
        Sad
    }
    
    void Start()
    {
        mainCamera = Camera.main;
        
        // Create eyes if they don't exist
        if (leftEye == null || rightEye == null)
        {
            CreateEyes();
        }
        
        leftEyeOriginalScale = eyeScale;
        rightEyeOriginalScale = eyeScale;
        
        // Set initial eye scales
        if (leftEye != null) leftEye.localScale = eyeScale;
        if (rightEye != null) rightEye.localScale = eyeScale;
        
        // Schedule first blink
        ScheduleNextBlink();
    }
    
    void Update()
    {
        UpdateLookAt();
        CheckForBlink();
        UpdateEmotionVisuals();
    }
    
    void CreateEyes()
    {
        // Create left eye
        GameObject leftEyeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftEyeObj.name = "LeftEye";
        leftEye = leftEyeObj.transform;
        leftEye.SetParent(transform);
        leftEye.localPosition = new Vector3(-eyeSpacing / 2f, 0.2f, eyeForwardOffset);
        leftEye.localScale = eyeScale;
        
        // Create right eye
        GameObject rightEyeObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightEyeObj.name = "RightEye";
        rightEye = rightEyeObj.transform;
        rightEye.SetParent(transform);
        rightEye.localPosition = new Vector3(eyeSpacing / 2f, 0.2f, eyeForwardOffset);
        rightEye.localScale = eyeScale;
        
        // Setup eye materials
        SetupEyeMaterial(leftEyeObj);
        SetupEyeMaterial(rightEyeObj);
        
        // Create pupils
        CreatePupils();
        
        Debug.Log("Eyes created for slime");
    }
    
    void CreatePupils()
    {
        if (leftEye != null)
        {
            GameObject leftPupilObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            leftPupilObj.name = "LeftPupil";
            leftPupil = leftPupilObj.transform;
            leftPupil.SetParent(leftEye);
            leftPupil.localPosition = new Vector3(0, 0, 0.45f);
            leftPupil.localScale = Vector3.one * pupilScale;
            SetupPupilMaterial(leftPupilObj);
        }
        
        if (rightEye != null)
        {
            GameObject rightPupilObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rightPupilObj.name = "RightPupil";
            rightPupil = rightPupilObj.transform;
            rightPupil.SetParent(rightEye);
            rightPupil.localPosition = new Vector3(0, 0, 0.45f);
            rightPupil.localScale = Vector3.one * pupilScale;
            SetupPupilMaterial(rightPupilObj);
        }
    }
    
    void SetupEyeMaterial(GameObject eyeObj)
    {
        Renderer renderer = eyeObj.GetComponent<Renderer>();
        Material eyeMat = new Material(Shader.Find("Standard"));
        eyeMat.color = eyeColor;
        eyeMat.EnableKeyword("_EMISSION");
        eyeMat.SetColor("_EmissionColor", eyeColor * eyeEmissionIntensity);
        eyeMat.SetFloat("_Metallic", 0f);
        eyeMat.SetFloat("_Glossiness", 0.9f);
        renderer.material = eyeMat;
        
        // Remove collider from eyes
        Collider col = eyeObj.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
    
    void SetupPupilMaterial(GameObject pupilObj)
    {
        Renderer renderer = pupilObj.GetComponent<Renderer>();
        Material pupilMat = new Material(Shader.Find("Standard"));
        pupilMat.color = pupilColor;
        pupilMat.SetFloat("_Metallic", 0f);
        pupilMat.SetFloat("_Glossiness", 0.8f);
        renderer.material = pupilMat;
        
        // Remove collider
        Collider col = pupilObj.GetComponent<Collider>();
        if (col != null) Destroy(col);
    }
    
    void UpdateLookAt()
    {
        if (leftEye == null || rightEye == null) return;
        
        // Determine look target
        Transform targetTransform = lookTarget;
        if (lookAtCamera && mainCamera != null)
        {
            targetTransform = mainCamera.transform;
        }
        
        if (targetTransform == null) return;
        
        // Calculate look direction
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        Vector3 localDirection = transform.InverseTransformDirection(directionToTarget);
        
        // Clamp angle
        float angle = Vector3.Angle(Vector3.forward, localDirection);
        if (angle > maxLookAngle)
        {
            localDirection = Vector3.Slerp(Vector3.forward, localDirection, maxLookAngle / angle);
        }
        
        // Rotate eyes smoothly
        Quaternion targetRotation = Quaternion.LookRotation(localDirection);
        leftEye.localRotation = Quaternion.Slerp(leftEye.localRotation, targetRotation, Time.deltaTime * lookSpeed);
        rightEye.localRotation = Quaternion.Slerp(rightEye.localRotation, targetRotation, Time.deltaTime * lookSpeed);
        
        // Move pupils slightly for extra life
        if (leftPupil != null && rightPupil != null)
        {
            Vector3 pupilOffset = new Vector3(localDirection.x, localDirection.y, 0) * pupilMovementRange;
            leftPupil.localPosition = Vector3.Lerp(leftPupil.localPosition, 
                new Vector3(0, 0, 0.45f) + pupilOffset, Time.deltaTime * lookSpeed);
            rightPupil.localPosition = Vector3.Lerp(rightPupil.localPosition, 
                new Vector3(0, 0, 0.45f) + pupilOffset, Time.deltaTime * lookSpeed);
        }
    }
    
    void CheckForBlink()
    {
        if (!isBlinking && Time.time >= nextBlinkTime)
        {
            if (blinkCoroutine != null) StopCoroutine(blinkCoroutine);
            blinkCoroutine = StartCoroutine(Blink());
        }
    }
    
    IEnumerator Blink()
    {
        isBlinking = true;
        
        // Close eyes
        float elapsed = 0f;
        while (elapsed < blinkSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blinkSpeed;
            
            if (leftEye != null)
            {
                Vector3 scale = leftEyeOriginalScale;
                scale.y = Mathf.Lerp(leftEyeOriginalScale.y, leftEyeOriginalScale.y * blinkScaleY, t);
                leftEye.localScale = scale;
            }
            
            if (rightEye != null)
            {
                Vector3 scale = rightEyeOriginalScale;
                scale.y = Mathf.Lerp(rightEyeOriginalScale.y, rightEyeOriginalScale.y * blinkScaleY, t);
                rightEye.localScale = scale;
            }
            
            yield return null;
        }
        
        // Open eyes
        elapsed = 0f;
        while (elapsed < blinkSpeed)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / blinkSpeed;
            
            if (leftEye != null)
            {
                Vector3 scale = leftEyeOriginalScale;
                scale.y = Mathf.Lerp(leftEyeOriginalScale.y * blinkScaleY, leftEyeOriginalScale.y, t);
                leftEye.localScale = scale;
            }
            
            if (rightEye != null)
            {
                Vector3 scale = rightEyeOriginalScale;
                scale.y = Mathf.Lerp(rightEyeOriginalScale.y * blinkScaleY, rightEyeOriginalScale.y, t);
                rightEye.localScale = scale;
            }
            
            yield return null;
        }
        
        isBlinking = false;
        ScheduleNextBlink();
    }
    
    void ScheduleNextBlink()
    {
        nextBlinkTime = Time.time + Random.Range(blinkIntervalMin, blinkIntervalMax);
    }
    
    void UpdateEmotionVisuals()
    {
        switch (currentEmotion)
        {
            case SlimeEmotion.Happy:
                leftEyeOriginalScale = eyeScale;
                rightEyeOriginalScale = eyeScale;
                break;
                
            case SlimeEmotion.Excited:
                leftEyeOriginalScale = eyeScale * 1.2f;
                rightEyeOriginalScale = eyeScale * 1.2f;
                break;
                
            case SlimeEmotion.Sleepy:
                leftEyeOriginalScale = new Vector3(eyeScale.x, eyeScale.y * 0.5f, eyeScale.z);
                rightEyeOriginalScale = new Vector3(eyeScale.x, eyeScale.y * 0.5f, eyeScale.z);
                break;
                
            case SlimeEmotion.Curious:
                leftEyeOriginalScale = eyeScale * 1.1f;
                rightEyeOriginalScale = eyeScale * 1.1f;
                break;
                
            case SlimeEmotion.Sad:
                leftEyeOriginalScale = eyeScale * 0.9f;
                rightEyeOriginalScale = eyeScale * 0.9f;
                break;
        }
        
        // Apply emotion scale if not blinking
        if (!isBlinking)
        {
            if (leftEye != null) 
                leftEye.localScale = Vector3.Lerp(leftEye.localScale, leftEyeOriginalScale, Time.deltaTime * 5f);
            if (rightEye != null) 
                rightEye.localScale = Vector3.Lerp(rightEye.localScale, rightEyeOriginalScale, Time.deltaTime * 5f);
        }
    }
    
    /// <summary>
    /// Change the slime's emotional state
    /// </summary>
    public void SetEmotion(SlimeEmotion emotion)
    {
        currentEmotion = emotion;
    }
    
    /// <summary>
    /// React to interaction at a specific point
    /// </summary>
    public void ReactToInteraction(Vector3 worldPoint)
    {
        // Look at the interaction point briefly
        StartCoroutine(LookAtPointTemporarily(worldPoint, 0.5f));
        
        // Widen eyes slightly
        if (currentEmotion != SlimeEmotion.Excited)
        {
            StartCoroutine(WidenEyesBriefly());
        }
    }
    
    IEnumerator LookAtPointTemporarily(Vector3 point, float duration)
    {
        Transform originalTarget = lookTarget;
        bool wasLookingAtCamera = lookAtCamera;
        
        // Create temporary target
        GameObject tempTarget = new GameObject("TempLookTarget");
        tempTarget.transform.position = point;
        lookTarget = tempTarget.transform;
        lookAtCamera = false;
        
        yield return new WaitForSeconds(duration);
        
        // Restore
        lookTarget = originalTarget;
        lookAtCamera = wasLookingAtCamera;
        Destroy(tempTarget);
    }
    
    IEnumerator WidenEyesBriefly()
    {
        Vector3 widenScale = eyeScale * 1.15f;
        float duration = 0.2f;
        
        // Widen
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            leftEyeOriginalScale = Vector3.Lerp(eyeScale, widenScale, t);
            rightEyeOriginalScale = Vector3.Lerp(eyeScale, widenScale, t);
            yield return null;
        }
        
        // Return to normal
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            leftEyeOriginalScale = Vector3.Lerp(widenScale, eyeScale, t);
            rightEyeOriginalScale = Vector3.Lerp(widenScale, eyeScale, t);
            yield return null;
        }
    }
    
    /// <summary>
    /// Get current emotion state
    /// </summary>
    public SlimeEmotion GetCurrentEmotion()
    {
        return currentEmotion;
    }
}
