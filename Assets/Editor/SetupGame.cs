#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class SetupGameTools
{
    [MenuItem("Tools/Setup Background Image")]
    public static void SetupBackgroundImage()
    {
        // Find the background sprite
        string spritePath = "Assets/Sprites/Backgrounds/fondoespacio.png";
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        
        if (bgSprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Background sprite not found at: " + spritePath, "OK");
            return;
        }

        // Check if background already exists
        GameObject existingBg = GameObject.Find("Background");
        if (existingBg != null)
        {
            Debug.Log("Background already exists: " + existingBg.name);
            SpriteRenderer existingSr = existingBg.GetComponent<SpriteRenderer>();
            if (existingSr != null)
            {
                existingSr.sprite = bgSprite;
                existingSr.sortingLayerName = "Default";
                existingSr.sortingOrder = -1000;
                existingBg.layer = 0;
                
                if (existingBg.GetComponent<BackgroundRenderer>() == null)
                {
                    existingBg.AddComponent<BackgroundRenderer>();
                }
                
                Debug.Log("Updated existing background sprite and sorting order");
            }
            return;
        }

        // Create background GameObject
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.position = Vector3.zero;
        
        // Set to Background layer
        int bgLayer = LayerMask.NameToLayer("Background");
        if (bgLayer != -1)
        {
            bgObj.layer = bgLayer;
        }

        // Add SpriteRenderer
        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -1000;
        
        // Add BackgroundRenderer to ensure it stays behind everything
        bgObj.AddComponent<BackgroundRenderer>();
        
        // Scale to cover the view
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(50, 50);

        Undo.RegisterCreatedObjectUndo(bgObj, "Create Background");
        Debug.Log("Background created with sprite: " + bgSprite.name);
    }

    [MenuItem("Tools/Setup Basic Scene")]
    public static void Execute()
    {
        Debug.Log("Setup tool: Please create the scene manually and assign references in the Inspector.");
    }
}
#endif
