#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateBulletPrefab
{
    [MenuItem("Tools/Create Bullet Prefab")]
    public static void Execute()
    {
        GameObject bulletGO = new GameObject("Bullet");
        bulletGO.AddComponent<SpriteRenderer>();
        bulletGO.AddComponent<Bullet>();

        string spritePath = "Assets/Player Sprites/Main ship weapons/PNGs/Main ship weapon - Projectile - Auto cannon bullet.png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
        {
            bulletGO.GetComponent<SpriteRenderer>().sprite = sprite;
        }

        System.IO.Directory.CreateDirectory("Assets/Prefabs/Bullets");
        PrefabUtility.SaveAsPrefabAsset(bulletGO, "Assets/Prefabs/Bullets/Bullet.prefab");
        Object.DestroyImmediate(bulletGO);
        Debug.Log("Bullet prefab created.");
    }
}
#endif
