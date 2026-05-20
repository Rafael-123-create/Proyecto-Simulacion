#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class SetupGameTools
{
    [MenuItem("Tools/Setup Background Image")]
    public static void SetupBackgroundImage()
    {
        string spritePath = "Assets/Sprites/Backgrounds/fondoespacio.png";
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        
        if (bgSprite == null)
        {
            EditorUtility.DisplayDialog("Error", "Background sprite not found at: " + spritePath, "OK");
            return;
        }

        GameObject existingBg = GameObject.Find("Background");
        if (existingBg != null)
        {
            SpriteRenderer existingSr = existingBg.GetComponent<SpriteRenderer>();
            if (existingSr != null)
            {
                existingSr.sprite = bgSprite;
                existingSr.sortingLayerName = "Default";
                existingSr.sortingOrder = -10;
                Debug.Log("Updated existing background");
            }
            return;
        }

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.position = new Vector3(0, 0, 10);
        bgObj.layer = 0;

        SpriteRenderer sr = bgObj.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingLayerName = "Default";
        sr.sortingOrder = -10;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(50, 50);

        Undo.RegisterCreatedObjectUndo(bgObj, "Create Background");
        Debug.Log("Background created");
    }

    [MenuItem("Tools/Setup Basic Scene")]
    public static void Execute()
    {
        Debug.Log("Setup tool: Please create the scene manually.");
    }
}
#endif
