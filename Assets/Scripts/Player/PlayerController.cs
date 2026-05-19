using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Transform))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1; // 1 or 2, set in inspector for each player
    
    [Header("Lane Settings")]
    public int laneCount = 5; // Number of vertical lanes (columns)
    
    [Header("Movement Settings")]
    public float laneChangeSpeed = 12f; // How fast to snap to lane center
    public float moveSpeed = 8f; // Speed for movement within lane
    public float maxVertical = 4f; // Top boundary
    public float minVertical = -4f; // Bottom boundary
    public float maxLateralOffset = 0.8f; // Max left/right movement within lane
    
    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float baseFireRate = 2f; // Base fire rate (shots per second)
    public float maxFireRate = 5f;  // Max fire rate at full difficulty
    private float nextFireTime = 0f;
    
    [Header("Input")]
    public bool useGamepad = true;
    
    // Internal state
    private int currentLaneIndex = 2; // Start in middle lane (0-indexed)
    private float lateralOffset = 0f; // Left/right offset within lane
    private float verticalPosition = 0f; // Y position
    private float targetLaneX = 0f; // Target X for lane center
    private float laneWidth = 0f; // Width of each lane
    private float screenLeft = 0f; // Left boundary
    private float screenRight = 0f; // Right boundary
    
    // Track previous input to detect key presses (not holds)
    private bool prevLaneLeft = false;
    private bool prevLaneRight = false;
    private bool shootPressed = false;
    
    void Start()
    {
        InitializeBounds();
        InitializePosition();
    }
    
    void InitializeBounds()
    {
        Camera cam = GetPlayerCamera();
        if (cam == null)
        {
            Debug.LogError("PlayerController: Could not find camera for Player " + playerNumber);
            return;
        }
        
        Debug.Log("PlayerController: Player " + playerNumber + " using camera: " + cam.gameObject.name);
        
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        
        // Check versus mode
        bool isVersus = false;
        if (GameManager.Instance != null)
        {
            isVersus = GameManager.Instance.IsVersusMode();
        }
        
        float playableWidth = camWidth;
        
        // Calculate lane width
        laneWidth = playableWidth / laneCount;
        
        // Calculate screen bounds for this player
        float halfPlayableWidth = playableWidth / 2f;
        screenLeft = -halfPlayableWidth;
        screenRight = halfPlayableWidth;
        
        // For versus mode, adjust bounds based on player number
        if (isVersus)
        {
            if (playerNumber == 2)
            {
                screenLeft += playableWidth;
                screenRight += playableWidth;
            }
        }
        
        // Initialize target lane X
        targetLaneX = GetLaneCenterX();
        
        Debug.Log("PlayerController: Player " + playerNumber + " bounds: left=" + screenLeft + ", right=" + screenRight);
    }
    
    Camera GetPlayerCamera()
    {
        // Find camera by GameObject name
        string camName = playerNumber == 1 ? "Player1Camera" : "Player2Camera";
        
        // Use GameObject.Find to search the hierarchy
        GameObject camObj = GameObject.Find(camName);
        if (camObj != null)
        {
            Camera cam = camObj.GetComponent<Camera>();
            if (cam != null) return cam;
            
            // Check children
            foreach (Transform child in camObj.transform)
            {
                cam = child.GetComponent<Camera>();
                if (cam != null) return cam;
            }
        }
        
        // Fallback to Camera.main
        Debug.LogWarning("PlayerController: Falling back to Camera.main for Player " + playerNumber);
        return Camera.main;
    }
    
    void InitializePosition()
    {
        currentLaneIndex = laneCount / 2; // Start in middle
        targetLaneX = GetLaneCenterX();
        lateralOffset = 0f;
        verticalPosition = minVertical;
        transform.position = new Vector3(targetLaneX, verticalPosition, 0f);
    }
    
    void Update()
    {
        HandleInput();
        UpdatePosition();
        HandleShooting();
    }
    
    void HandleInput()
    {
        float lateralInput = 0f; // Left/Right within lane
        float verticalInput = 0f; // Forward/Back
        bool laneLeftPressed = false;
        bool laneRightPressed = false;
        shootPressed = false;
        
        if (playerNumber == 1)
        {
            // Player 1 Controls:
            // Lane Change: Q (left lane) / E (right lane)
            // Lateral: A (left) / D (right)
            // Vertical: W (up) / S (down)
            // Shoot: Left Ctrl
            
            if (Keyboard.current != null)
            {
                // Lane change (detect press, not hold)
                bool qPressed = Keyboard.current.qKey.isPressed;
                bool ePressed = Keyboard.current.eKey.isPressed;
                
                if (qPressed && !prevLaneLeft) laneLeftPressed = true;
                if (ePressed && !prevLaneRight) laneRightPressed = true;
                
                prevLaneLeft = qPressed;
                prevLaneRight = ePressed;
                
                // Lateral movement within lane
                if (Keyboard.current.aKey.isPressed) lateralInput -= 1f;
                if (Keyboard.current.dKey.isPressed) lateralInput += 1f;
                
                // Vertical movement
                if (Keyboard.current.wKey.isPressed) verticalInput += 1f;
                if (Keyboard.current.sKey.isPressed) verticalInput -= 1f;
                
                // Shoot
                if (Keyboard.current.leftCtrlKey.isPressed) shootPressed = true;
            }
            
            // Gamepad 0
            if (useGamepad && Gamepad.all.Count > 0)
            {
                var pad = Gamepad.all[0];
                lateralInput += pad.leftStick.x.ReadValue();
                verticalInput += pad.leftStick.y.ReadValue();
                
                // D-Pad for lane change
                if (pad.dpad.left.isPressed && !prevLaneLeft) laneLeftPressed = true;
                if (pad.dpad.right.isPressed && !prevLaneRight) laneRightPressed = true;
                
                prevLaneLeft = pad.dpad.left.isPressed;
                prevLaneRight = pad.dpad.right.isPressed;
                
                // Shoot
                if (pad.rightTrigger.isPressed) shootPressed = true;
            }
        }
        else if (playerNumber == 2)
        {
            // Player 2 Controls:
            // Lane Change: U (left lane) / O (right lane)
            // Lateral: Left Arrow / Right Arrow
            // Vertical: Up Arrow / Down Arrow
            // Shoot: Right Ctrl
            
            if (Keyboard.current != null)
            {
                // Lane change (detect press, not hold)
                bool uPressed = Keyboard.current.uKey.isPressed;
                bool oPressed = Keyboard.current.oKey.isPressed;
                
                if (uPressed && !prevLaneLeft) laneLeftPressed = true;
                if (oPressed && !prevLaneRight) laneRightPressed = true;
                
                prevLaneLeft = uPressed;
                prevLaneRight = oPressed;
                
                // Lateral movement within lane
                if (Keyboard.current.leftArrowKey.isPressed) lateralInput -= 1f;
                if (Keyboard.current.rightArrowKey.isPressed) lateralInput += 1f;
                
                // Vertical movement
                if (Keyboard.current.upArrowKey.isPressed) verticalInput += 1f;
                if (Keyboard.current.downArrowKey.isPressed) verticalInput -= 1f;
                
                // Shoot
                if (Keyboard.current.rightCtrlKey.isPressed) shootPressed = true;
            }
            
            // Gamepad 1
            if (useGamepad && Gamepad.all.Count > 1)
            {
                var pad = Gamepad.all[1];
                lateralInput += pad.leftStick.x.ReadValue();
                verticalInput += pad.leftStick.y.ReadValue();
                
                // D-Pad for lane change
                if (pad.dpad.left.isPressed && !prevLaneLeft) laneLeftPressed = true;
                if (pad.dpad.right.isPressed && !prevLaneRight) laneRightPressed = true;
                
                prevLaneLeft = pad.dpad.left.isPressed;
                prevLaneRight = pad.dpad.right.isPressed;
                
                // Shoot
                if (pad.leftTrigger.isPressed) shootPressed = true;
            }
        }
        
        // Process lane change (only on press, not hold)
        if (laneLeftPressed && currentLaneIndex > 0)
        {
            currentLaneIndex--;
            targetLaneX = GetLaneCenterX();
            lateralOffset = 0f;
        }
        if (laneRightPressed && currentLaneIndex < laneCount - 1)
        {
            currentLaneIndex++;
            targetLaneX = GetLaneCenterX();
            lateralOffset = 0f;
        }
        
        // Process lateral movement within lane
        if (Mathf.Abs(lateralInput) > 0.1f)
        {
            lateralOffset = Mathf.Clamp(
                lateralOffset + lateralInput * Time.deltaTime * moveSpeed,
                -maxLateralOffset, 
                maxLateralOffset
            );
        }
        
        // Process vertical movement
        if (Mathf.Abs(verticalInput) > 0.1f)
        {
            verticalPosition = Mathf.Clamp(
                verticalPosition + verticalInput * Time.deltaTime * moveSpeed,
                minVertical,
                maxVertical
            );
        }
    }
    
    void UpdatePosition()
    {
        // Smoothly move toward target lane center + lateral offset
        float targetX = targetLaneX + lateralOffset;
        float currentX = transform.position.x;
        float newX = Mathf.Lerp(currentX, targetX, Time.deltaTime * laneChangeSpeed);
        
        transform.position = new Vector3(newX, verticalPosition, 0f);
    }
    
    float GetLaneCenterX()
    {
        // Lane centers are distributed from screenLeft to screenRight
        return screenLeft + (currentLaneIndex + 0.5f) * laneWidth;
    }
    
    void HandleShooting()
    {
        float currentFireRate = GetCurrentFireRate();
        if (shootPressed && Time.time >= nextFireTime && bulletPrefab != null && firePoint != null)
        {
            Shoot();
            nextFireTime = Time.time + 1f / currentFireRate;
        }
    }
    
    float GetCurrentFireRate()
    {
        float difficultyScale = 0f;
        if (WaveSpawner.Instance != null)
        {
            difficultyScale = WaveSpawner.Instance.DifficultyScale;
        }
        return Mathf.Lerp(baseFireRate, maxFireRate, difficultyScale);
    }
    
    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.ownerPlayerNumber = playerNumber;
            }
        }
    }
    
    // Visualize lanes in editor
    void OnDrawGizmosSelected()
    {
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
        float currentLaneX = screenLeftGizmo + (currentLaneIndex + 0.5f) * laneWidthGizmo;
        Gizmos.DrawLine(new Vector3(currentLaneX, minVertical - 1f, 0), new Vector3(currentLaneX, maxVertical + 1f, 0));
        
        // Draw player position
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.2f);
        
        // Draw lateral offset range
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        float minOffsetX = currentLaneX - maxLateralOffset;
        float maxOffsetX = currentLaneX + maxLateralOffset;
        Gizmos.DrawLine(new Vector3(minOffsetX, minVertical, 0), new Vector3(minOffsetX, maxVertical, 0));
        Gizmos.DrawLine(new Vector3(maxOffsetX, minVertical, 0), new Vector3(maxOffsetX, maxVertical, 0));
    }
}
