using UnityEngine;
using UnityEditor;

public class CreateBulletPrefab
{
    [MenuItem("Tools/Create Bullet Prefab")]
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
        }

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Bullets/Bullet.prefab";
        PrefabUtility.SaveAsPrefabAsset(bulletGO, prefabPath);

        // Clean up the temporary GameObject
        DestroyImmediate(bulletGO);

        Debug.Log("Bullet prefab created at: " + prefabPath);
    }
}
