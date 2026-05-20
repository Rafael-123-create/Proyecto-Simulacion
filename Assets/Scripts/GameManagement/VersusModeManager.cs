using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    public Camera player1Camera;
    public Camera player2Camera;
    public Camera mainCamera;
    
    public const int Player1Layer = 8;
    public const int Player2Layer = 9;

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

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        Debug.Log("VersusModeManager: P1 Camera = " + (player1Camera != null ? player1Camera.gameObject.name : "NULL"));
        Debug.Log("VersusModeManager: P2 Camera = " + (player2Camera != null ? player2Camera.gameObject.name : "NULL"));
    }
    
    Camera FindCameraByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            Camera cam = obj.GetComponent<Camera>();
            if (cam != null) return cam;
            
            foreach (Transform child in obj.transform)
            {
                cam = child.GetComponent<Camera>();
                if (cam != null) return cam;
            }
        }
        
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

        if (mainCamera != null)
        {
            mainCamera.enabled = false;
        }

        player1Camera.enabled = true;
        player2Camera.enabled = true;

        player1Camera.rect = new Rect(0f, 0f, 0.48f, 1f);
        player2Camera.rect = new Rect(0.52f, 0f, 0.48f, 1f);

        ConfigureCamera(player1Camera, 0);
        ConfigureCamera(player2Camera, 1);
        
        float camHeight = 2f * player1Camera.orthographicSize;
        float camWidth = camHeight * player1Camera.aspect;
        float halfWidth = camWidth * 0.5f;
        
        player1Camera.transform.position = new Vector3(-halfWidth * 0.5f, 0f, -10f);
        player2Camera.transform.position = new Vector3(halfWidth * 0.5f, 0f, -10f);
        
        int p1Mask = (1 << Player1Layer) | (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Background"));
        int p2Mask = (1 << Player2Layer) | (1 << LayerMask.NameToLayer("UI")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Background"));
        
        player1Camera.cullingMask = p1Mask;
        player2Camera.cullingMask = p2Mask;
        
        Debug.Log("VersusModeManager: Modo versus activado 50/50 - P1 mask=" + p1Mask + ", P2 mask=" + p2Mask);
    }

    public void DisableVersusMode()
    {
        if (mainCamera != null)
        {
            mainCamera.enabled = true;
            mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
            mainCamera.transform.position = new Vector3(0, 0, -10);
            mainCamera.cullingMask = ~0;
        }

        if (player1Camera != null)
        {
            player1Camera.enabled = false;
            player1Camera.cullingMask = ~0;
        }
        
        if (player2Camera != null)
        {
            player2Camera.enabled = false;
            player2Camera.cullingMask = ~0;
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
