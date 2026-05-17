using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CreateBulletPrefab : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Create Bullet Prefab")]
    public static void CreateBulletPrefab()
    {
        // Create a new GameObject for the bullet
        GameObject bulletGO = new GameObject("Bullet");
        bulletGO.AddComponent<SpriteRenderer>();
        bulletGO.AddComponent<Bullet>();

        // Set the bullet sprite (we'll use the auto cannon bullet sprite from the assets)
        string bulletSpritePath = "Assets/Player Sprites/Main ship weapons/PNGs/Main ship weapon - Projectile - Auto cannon bullet.png";
        Sprite bulletSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bulletSpritePath);
        if (bulletSprite != null)
        {
            bulletGO.GetComponent<SpriteRenderer>().sprite = bulletSprite;
        }
        else
        {
            Debug.LogWarning("Bullet sprite not found at: " + bulletSpritePath);
            // Create a simple white square as fallback
            Texture2D tex = new Texture2D(16, 16);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();
            bulletSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f));
            bulletGO.GetComponent<SpriteRenderer>().sprite = bulletSprite;
        }

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Bullets/Bullet.prefab";
        PrefabUtility.SaveAsPrefabAsset(bulletGO, prefabPath);

        // Clean up the temporary GameObject
        DestroyImmediate(bulletGO);

        Debug.Log("Bullet prefab created at: " + prefabPath);
    }
#endif
}
