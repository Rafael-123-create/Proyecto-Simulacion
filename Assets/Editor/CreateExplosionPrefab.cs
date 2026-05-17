#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class CreateExplosionPrefab
{
    [MenuItem("Tools/Create Explosion Prefab")]
    public static void Execute()
    {
        string spritePath = "Assets/Enemy Sprites/Foozle_2DS0013_Void_EnemyFleet_2/Nairan/Destruction/PNGs/Nairan - Fighter - Destruction.png";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

        GameObject go = new GameObject("Explosion");
        go.AddComponent<SpriteRenderer>();
        if (sprite != null) go.GetComponent<SpriteRenderer>().sprite = sprite;
        go.AddComponent<DestroyAfterTime>().lifetime = 0.5f;

        System.IO.Directory.CreateDirectory("Assets/Prefabs/Effects");
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/Effects/Explosion.prefab");
        Object.DestroyImmediate(go);
        Debug.Log("Explosion prefab created.");
    }
}
#endif
