#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateBulletPrefab
{
    [MenuItem("Tools/Create Bullet Prefab")]
    public static void Execute()
    {
        // Create bullet GameObject
        GameObject bulletGO = new GameObject("Bullet");
        
        // Add SpriteRenderer
        SpriteRenderer sr = bulletGO.AddComponent<SpriteRenderer>();
        
        // Try to find a bullet sprite
        string[] bulletPaths = new string[]
        {
            "Assets/Player Sprites/Main ship weapons/PNGs/Main ship weapon - Projectile - Auto cannon bullet.png",
            "Assets/Player Sprites/Main ship weapons/PNGs/Main ship weapon - Projectile - Rocket.png",
            "Assets/Player Sprites/Main ship weapons/PNGs/Main ship weapon - Projectile - Zapper.png"
        };
        
        Sprite sprite = null;
        foreach (string path in bulletPaths)
        {
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) break;
        }
        
        if (sprite != null)
        {
            sr.sprite = sprite;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 1;
        }
        else
        {
            // Create a simple white rectangle as fallback
            Texture2D tex = new Texture2D(8, 16);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 8; x++)
                    tex.SetPixel(x, y, Color.cyan);
            tex.Apply();
            sprite = Sprite.Create(tex, new Rect(0, 0, 8, 16), new Vector2(0.5f, 0.5f));
            sr.sprite = sprite;
        }
        
        // Add Bullet script
        Bullet bulletScript = bulletGO.AddComponent<Bullet>();
        bulletScript.speed = 10f;
        bulletScript.damage = 1;
        bulletScript.isPlayerBullet = true;
        
        // Add collider
        CircleCollider2D col = bulletGO.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        
        // Create prefab
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Bullets");
        string prefabPath = "Assets/Prefabs/Bullets/Bullet.prefab";
        PrefabUtility.SaveAsPrefabAsset(bulletGO, prefabPath);
        Object.DestroyImmediate(bulletGO);
        
        Debug.Log("Bullet prefab created at: " + prefabPath);
    }
}
#endif
