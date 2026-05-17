using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Transform))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1; // 1 or 2, set in inspector for each player
    
    [Header("Lane Settings")]
    public int laneCount = 5; // Number of horizontal lanes
    
    [Header("Movement Settings")]
    public float laneChangeSpeed = 10f; // How fast to snap to lane center
    public float moveSpeed = 5f; // Speed for offset and vertical movement
    public float maxVertical = 4f; // Top boundary
    public float minVertical = -4f; // Bottom boundary
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;
    
    [Header("Input")]
    public bool useGamepad = true;
    
    // Internal state
    private int currentLaneIndex = 2; // Start in middle lane (0-indexed, 0=leftmost, 4=rightmost)
    private float laneOffset = 0f; // Current offset within lane (-maxOffset to maxOffset)
    private float verticalPosition = 0f; // Current Y position
    private float maxLaneOffset = 0f; // Maximum allowed offset within lane (calculated)
    private float laneWidth = 0f; // Width of each lane (calculated)
    private float screenLeft = 0f; // Left boundary of playable area
    private float screenRight = 0f; // Right boundary of playable area
    
    void Start()
    {
        InitializeBounds();
        InitializePosition();
    }
    
    void InitializeBounds()
    {
        // Calculate playable bounds based on camera and versus mode
        Camera cam = Camera.main;
        if (cam == null) return;
        
        // Get the camera's viewport in world space
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        
        // In versus mode, each player gets half the screen
        // We need to check if we're in versus mode from GameManager
        bool isVersus = false;
        if (GameManager.Instance != null)
        {
            isVersus = GameManager.Instance.IsVersusMode();
        }
        
        float playableWidth = isVersus ? camWidth / 2f : camWidth;
        float playableHeight = camHeight; // Height is typically not split in versus mode for this game
        
        // Calculate lane width and max offset
        laneWidth = playableWidth / laneCount;
        maxLaneOffset = laneWidth / 2f * 0.8f; // 80% of half-lane width for comfortable movement
        
        // Calculate screen bounds for this player
        float halfPlayableWidth = playableWidth / 2f;
        screenLeft = -halfPlayableWidth;
        screenRight = halfPlayableWidth;
        
        // For versus mode, adjust bounds based on player number
        if (isVersus)
        {
            if (playerNumber == 2)
            {
                // Player 2 is on the right half
                screenLeft += playableWidth;
                screenRight += playableWidth;
            }
            // Player 1 stays on left half (no adjustment needed)
        }
    }
    
    void InitializePosition()
    {
        // Initialize position based on lane
        UpdateLanePosition();
        // Initialize Y to minimum (bottom) or center - we'll let player start at bottom
        verticalPosition = minVertical;
        transform.position = new Vector3(GetLaneCenterX() + laneOffset, verticalPosition, 0f);
    }
    
    void Update()
    {
        HandleInput();
        UpdatePosition();
    }
    
    void HandleInput()
    {
        // Get input based on player number
        float horizontalInput = 0f;
        float verticalInput = 0f;
        bool laneChangeUp = false;
        bool laneChangeDown = false;
        bool shootPressed = false;
        
        if (playerNumber == 1)
        {
            // Player 1: WASD for movement, Left Shift + W/S for lane change
            if (Keyboard.current != null)
            {
                // Horizontal movement (A/D)
                if (Keyboard.current.aKey.isPressed) horizontalInput -= 1f;
                if (Keyboard.current.dKey.isPressed) horizontalInput += 1f;
                
                // Vertical movement (W/S) - this will be for forward/back
                if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
                if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
                
                // Lane change with Left Shift (W/S with Shift)
                if (Keyboard.current.leftShiftKey.isPressed)
                {
                    if (Keyboard.current.wKey.isPressed) laneChangeUp = true;
                    if (Keyboard.current.sKey.isPressed) laneChangeDown = true;
                }
                
                // Shoot with Left Ctrl
                if (Keyboard.current.leftCtrlKey.isPressed) shootPressed = true;
            }
            
            // Gamepad 0
            if (useGamepad && Gamepad.current != null && Gamepad.all.Count > 0)
            {
                var pad = Gamepad.all[0]; // First gamepad for player 1
                horizontalInput += pad.leftStick.x.ReadValue();
                verticalInput += pad.leftStick.y.ReadValue();
                
                // D-Pad for lane change
                if (pad.dpad.up.isPressed) laneChangeUp = true;
                if (pad.dpad.down.isPressed) laneChangeDown = true;
                
                // Shoot with Right Trigger
                if (pad.rightTrigger.isPressed) shootPressed = true;
            }
        }
        else if (playerNumber == 2)
        {
            // Player 2: Arrow Keys for movement, Right Shift + Up/Down for lane change
            if (Keyboard.current != null)
            {
                // Horizontal movement (Left/Right Arrows)
                if (Keyboard.current.leftArrowKey.isPressed) horizontalInput -= 1f;
                if (Keyboard.current.rightArrowKey.isPressed) horizontalInput += 1f;
                
                // Vertical movement (Up/Down Arrows) - forward/back
                if (Keyboard.current.upArrowKey.isPressed) verticalInput += 1f;
                if (Keyboard.current.downArrowKey.isPressed) verticalInput -= 1f;
                
                // Lane change with Right Shift (Up/Down with Shift)
                if (Keyboard.current.rightShiftKey.isPressed)
                {
                    if (Keyboard.current.upArrowKey.isPressed) laneChangeUp = true;
                    if (Keyboard.current.downArrowKey.isPressed) laneChangeDown = true;
                }
                
                // Shoot with Right Ctrl
                if (Keyboard.current.rightCtrlKey.isPressed) shootPressed = true;
            }
            
            // Gamepad 1
            if (useGamepad && Gamepad.current != null && Gamepad.all.Count > 1)
            {
                var pad = Gamepad.all[1]; // Second gamepad for player 2
                horizontalInput += pad.leftStick.x.ReadValue();
                verticalInput += pad.leftStick.y.ReadValue();
                
                // D-Pad for lane change
                if (pad.dpad.up.isPressed) laneChangeUp = true;
                if (pad.dpad.down.isPressed) laneChangeDown = true;
                
                // Shoot with Left Trigger (for variety)
                if (pad.leftTrigger.isPressed) shootPressed = true;
            }
        }
        
        // Process lane change
        if (laneChangeUp && currentLaneIndex < laneCount - 1)
        {
            currentLaneIndex++;
        }
        if (laneChangeDown && currentLaneIndex > 0)
        {
            currentLaneIndex--;
        }
        
        // Process movement within lane
        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            // Update horizontal offset within lane
            laneOffset = Mathf.Clamp(
                laneOffset + horizontalInput * Time.deltaTime * moveSpeed,
                -maxLaneOffset, 
                maxLaneOffset
            );
        }
        
        // Process vertical movement (forward/back)
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            // Update vertical position
            verticalPosition = Mathf.Clamp(
                verticalPosition + verticalInput * Time.deltaTime * moveSpeed,
                minVertical,
                maxVertical
            );
        }
        
        // Process shooting
        if (shootPressed && Time.time >= nextFireTime && bulletPrefab != null && firePoint != null)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }
    
    void UpdatePosition()
    {
        // Smoothly move toward target lane center
        float targetX = GetLaneCenterX() + laneOffset;
        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * laneChangeSpeed);
        
        // Keep vertical position as is (already clamped in input)
        float newY = verticalPosition;
        
        transform.position = new Vector3(newX, newY, 0f);
    }
    
    float GetLaneCenterX()
    {
        // Calculate the center X position of the current lane
        // Lanes are distributed from screenLeft to screenRight
        float lanePosition = screenLeft + (currentLaneIndex + 0.5f) * laneWidth;
        return lanePosition;
    }

    void UpdateLanePosition()
    {
        float laneX = GetLaneCenterX() + laneOffset;
        transform.position = new Vector3(laneX, transform.position.y, 0f);
    }
    
    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            // Optional: add muzzle flash effect here
        }
    }
    
    // Optional: Add visualization for lanes in editor
    void OnDrawGizmosSelected()
    {
        // Recalculate bounds for gizmo drawing (approximation)
        Camera cam = Camera.main;
        if (cam == null) return;
        
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        bool isVersus = false;
        if (GameManager.Instance != null)
        {
            isVersus = GameManager.Instance.IsVersusMode();
        }
        
        float playableWidth = isVersus ? camWidth / 2f : camWidth;
        float laneWidthGizmo = playableWidth / laneCount;
        float maxOffsetGizmo = laneWidthGizmo / 2f * 0.8f;
        
        float screenLeftGizmo = -playableWidth / 2f;
        if (isVersus && playerNumber == 2)
        {
            screenLeftGizmo += playableWidth;
        }
        
        // Draw lane centers
        Gizmos.color = Color.yellow;
        for (int i = 0; i < laneCount; i++)
        {
            float laneX = screenLeftGizmo + (i + 0.5f) * laneWidthGizmo;
            Gizmos.DrawLine(new Vector3(laneX, minVertical - 1f, 0), new Vector3(laneX, maxVertical + 1f, 0));
        }
        
        // Draw current lane highlight
        Gizmos.color = Color.green;
        float currentLaneX = GetLaneCenterX();
        Gizmos.DrawLine(new Vector3(currentLaneX, minVertical - 1f, 0), new Vector3(currentLaneX, maxVertical + 1f, 0));
        
        // Draw player position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
        
        // Draw lane boundaries
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        for (int i = 0; i <= laneCount; i++)
        {
            float boundaryX = screenLeftGizmo + i * laneWidthGizmo;
            Gizmos.DrawLine(new Vector3(boundaryX, minVertical - 1f, 0), new Vector3(boundaryX, maxVertical + 1f, 0));
        }
        
        // Draw movement range for current lane
        Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
        float minOffsetX = GetLaneCenterX() - maxLaneOffset;
        float maxOffsetX = GetLaneCenterX() + maxLaneOffset;
        Gizmos.DrawLine(new Vector3(minOffsetX, minVertical, 0), new Vector3(minOffsetX, maxVertical, 0));
        Gizmos.DrawLine(new Vector3(maxOffsetX, minVertical, 0), new Vector3(maxOffsetX, maxVertical, 0));
    }
}
