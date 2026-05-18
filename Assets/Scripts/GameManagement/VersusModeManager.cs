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
        
        Debug.Log("VersusModeManager: Modo versus activado");
    }

    public void DisableVersusMode()
    {
        if (player1Camera == null) return;

        player1Camera.rect = new Rect(0f, 0f, 1f, 1f);
        player1Camera.enabled = true;
        
        if (player2Camera != null)
        {
            player2Camera.enabled = false;
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
