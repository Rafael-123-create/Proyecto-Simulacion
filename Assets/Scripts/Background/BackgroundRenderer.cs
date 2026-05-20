using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundRenderer : MonoBehaviour
{
    void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -1000;
        
        // Also set the layer to Default to ensure proper rendering
        gameObject.layer = 0;
    }
}
