using UnityEngine;

public class VersusModeManager : MonoBehaviour
{
    public void EnableVersusMode()
    {
        // This method will be called by GameManager to set up split-screen
        // We'll find the cameras and set their rects
        Camera[] cameras = Camera.allCameras;
        Camera player1Camera = null;
        Camera player2Camera = null;

        foreach (Camera cam in cameras)
        {
            // Assuming we tag the cameras or they are named in a specific way
            if (cam.CompareTag("Player1Camera"))
            {
                player1Camera = cam;
            }
            else if (cam.CompareTag("Player2Camera"))
            {
                player2Camera = cam;
            }
        }

        // If we didn't find tagged cameras, we can try to find by name or just use the first two cameras
        if (player1Camera == null || player2Camera == null)
        {
            Camera[] allCameras = Camera.allCameras;
            if (allCameras.Length >= 2)
            {
                player1Camera = allCameras[0];
                player2Camera = allCameras[1];
            }
            else
            {
                Debug.LogError("Not enough cameras for split-screen. Need at least two.");
                return;
            }
        }

        // Set up split-screen: vertical split
        // Player 1 (left half)
        Rect p1Rect = player1Camera.rect;
        p1Rect.x = 0f;
        p1Rect.width = 0.5f;
        p1Rect.y = 0f;
        p1Rect.height = 1f;
        player1Camera.rect = p1Rect;

        // Player 2 (right half)
        Rect p2Rect = player2Camera.rect;
        p2Rect.x = 0.5f;
        p2Rect.width = 0.5f;
        p2Rect.y = 0f;
        p2Rect.height = 1f;
        player2Camera.rect = p2Rect;

        // Set clear flags to solid color (black for space) if not already
        player1Camera.clearFlags = CameraClearFlags.SolidColor;
        player2Camera.clearFlags = CameraClearFlags.SolidColor;
        player1Camera.backgroundColor = Color.black;
        player2Camera.backgroundColor = Color.black;

        // Optional: set depths so they render correctly (if one is overlaying the other)
        player1Camera.depth = 0;
        player2Camera.depth = 1;
    }

    public void DisableVersusMode()
    {
        // Revert to single camera (full screen)
        // We'll enable the first camera and disable the second, or set the first to full rect.
        Camera[] cameras = Camera.allCameras;
        if (cameras.Length > 0)
        {
            Camera mainCamera = cameras[0];
            mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
            mainCamera.depth = 0;
        }

        // Disable other cameras if we don't need them
        for (int i = 1; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
        }
    }
}
