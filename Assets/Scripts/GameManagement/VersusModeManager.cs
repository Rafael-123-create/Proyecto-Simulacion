using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    public Camera player1Camera;
    public Camera player2Camera;

    void Awake()
    {
        if (player1Camera == null)
        {
            GameObject p1Cam = GameObject.FindGameObjectWithTag("Player1Camera");
            if (p1Cam != null) player1Camera = p1Cam.GetComponent<Camera>();
        }
        
        if (player2Camera == null)
        {
            GameObject p2Cam = GameObject.FindGameObjectWithTag("Player2Camera");
            if (p2Cam != null) player2Camera = p2Cam.GetComponent<Camera>();
        }
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
