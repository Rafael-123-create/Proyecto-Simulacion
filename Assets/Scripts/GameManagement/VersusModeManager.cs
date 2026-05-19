using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    public Camera player1Camera;
    public Camera player2Camera;

    void Awake()
    {
        if (player1Camera == null)
        {
            player1Camera = FindCameraByName("Player1Camera");
        }
        
        if (player2Camera == null)
        {
            player2Camera = FindCameraByName("Player2Camera");
        }
        
        Debug.Log("VersusModeManager: P1 Camera = " + (player1Camera != null ? player1Camera.gameObject.name : "NULL"));
        Debug.Log("VersusModeManager: P2 Camera = " + (player2Camera != null ? player2Camera.gameObject.name : "NULL"));
    }
    
    Camera FindCameraByName(string name)
    {
        // Try exact match first
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Camera cam = obj.GetComponent<Camera>();
            if (cam != null) return cam;
            
            // Check children
            foreach (Transform child in obj.transform)
            {
                cam = child.GetComponent<Camera>();
                if (cam != null) return cam;
            }
        }
        
        // Search all cameras
        Camera[] allCameras = FindObjectsByType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.gameObject.name == name || cam.gameObject.transform.parent?.name == name)
            {
                return cam;
            }
        }
        
        return null;
    }

    public void EnableVersusMode()
    {
        if (player1Camera == null || player2Camera == null)
        {
            Debug.LogError("VersusModeManager: No se encontraron las camaras. Asigna Player1Camera y Player2Camera en el Inspector.");
            return;
        }

        player1Camera.enabled = true;
        player2Camera.enabled = true;

        player1Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
        player2Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);

        ConfigureCamera(player1Camera, 0);
        ConfigureCamera(player2Camera, 1);
        
        // Offset Player2Camera to show the right side of the play area
        float camHeight = 2f * player2Camera.orthographicSize;
        float camWidth = camHeight * player2Camera.aspect;
        float halfPlayableWidth = camWidth / 2f;
        
        Vector3 p1Pos = player1Camera.transform.position;
        player2Camera.transform.position = new Vector3(p1Pos.x + halfPlayableWidth, p1Pos.y, p1Pos.z);
        
        Debug.Log("VersusModeManager: Modo versus activado - P2 camera offset: " + halfPlayableWidth);
    }

    public void DisableVersusMode()
    {
        if (player1Camera == null) return;

        player1Camera.rect = new Rect(0f, 0f, 1f, 1f);
        player1Camera.enabled = true;
        
        // Reset Player1Camera position
        player1Camera.transform.position = new Vector3(0, 0, -10);
        
        if (player2Camera != null)
        {
            player2Camera.enabled = false;
            // Reset Player2Camera position
            player2Camera.transform.position = new Vector3(0, 0, -10);
        }
    }

    void ConfigureCamera(Camera cam, int renderOrder)
    {
        if (cam == null) return;
        
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.depth = renderOrder;
        cam.orthographic = true;
    }
}
