using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public class SetupGame : MonoBehaviour
{
    [MenuItem("Game Setup/Create Bullet Prefab")]
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

        // Make sure the bullet sprite is set to not be flipped and has appropriate pivot
        // We'll set the sprite renderer's flip and other properties if needed

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Bullets/Bullet.prefab";
        PrefabUtility.SaveAsPrefabAsset(bulletGO, prefabPath);

        // Clean up the temporary GameObject
        DestroyImmediate(bulletGO);

        Debug.Log("Bullet prefab created at: " + prefabPath);
    }

    [MenuItem("Game Setup/Create Player Prefab")]
    public static void CreatePlayerPrefab()
    {
        // We'll create a player prefab that can be instantiated for both players
        // The PlayerController script will handle the player number via inspector
        GameObject playerGO = new GameObject("Player");
        playerGO.AddComponent<SpriteRenderer>();
        playerGO.AddComponent<PlayerController>();

        // Set the player sprite (we'll use the main ship base full health)
        string playerSpritePath = "Assets/Player Sprites/Main Ship/Main Ship - Bases/PNGs/Main Ship - Base - Full health.png";
        Sprite playerSprite = AssetDatabase.LoadAssetAtPath<Sprite>(playerSpritePath);
        if (playerSprite != null)
        {
            playerGO.GetComponent<SpriteRenderer>().sprite = playerSprite;
        }
        else
        {
            Debug.LogWarning("Player sprite not found at: " + playerSpritePath);
        }

        // Create an empty child for the fire point (where bullets will spawn)
        GameObject firePointGO = new GameObject("FirePoint");
        firePointGO.transform.SetParent(playerGO.transform);
        firePointGO.transform.localPosition = new Vector3(0f, 1f, 0f); // Adjust as needed

        // Assign the fire point to the PlayerController
        PlayerController playerController = playerGO.GetComponent<PlayerController>();
        playerController.firePoint = firePointGO.transform;

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Players/Player.prefab";
        PrefabUtility.SaveAsPrefabAsset(playerGO, prefabPath);

        // Clean up
        DestroyImmediate(playerGO);

        Debug.Log("Player prefab created at: " + prefabPath);
    }

    [MenuItem("Game Setup/Create Enemy Prefabs")]
    public static void CreateEnemyPrefabs()
    {
        // Create a shooter enemy prefab
        CreateShooterEnemyPrefab();
        // Create a kamikaze enemy prefab
        CreateKamikazeEnemyPrefab();
    }

    [MenuItem("Game Setup/Create Shooter Enemy Prefab")]
    public static void CreateShooterEnemyPrefab()
    {
        GameObject enemyGO = new GameObject("ShooterEnemy");
        enemyGO.AddComponent<SpriteRenderer>();
        enemyGO.AddComponent<ShooterEnemy>();

        // Set the enemy sprite (we'll use the Nairan Fighter Base)
        string enemySpritePath = "Assets/Enemy Sprites/Foozle_2DS0013_Void_EnemyFleet_2/Nairan/Designs - Base/PNGs/Nairan - Fighter - Base.png";
        Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(enemySpritePath);
        if (enemySprite != null)
        {
            enemyGO.GetComponent<SpriteRenderer>().sprite = enemySprite;
        }
        else
        {
            Debug.LogWarning("Enemy sprite not found at: " + enemySpritePath);
        }

        // Set the bullet prefab for the shooter (we'll use the same bullet as the player for now, but ideally an enemy bullet)
        // We'll create an enemy bullet prefab later, but for now let's use the player bullet and then change its direction in the script.
        // Actually, in the ShooterEnemy script, we instantiate the bulletPrefab and set its speed and isPlayerBullet.
        // So we can assign the bullet prefab we created earlier.
        ShooterEnemy shooter = enemyGO.GetComponent<ShooterEnemy>();
        shooter.bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Bullets/Bullet.prefab");
        if (shooter.bulletPrefab == null)
        {
            Debug.LogWarning("Bullet prefab not found for ShooterEnemy.");
        }
        // Set the fire point (we'll create an empty child)
        GameObject firePointGO = new GameObject("FirePoint");
        firePointGO.transform.SetParent(enemyGO.transform);
        firePointGO.transform.localPosition = new Vector3(0f, -1f, 0f); // Adjust as needed (shooting downward)
        shooter.firePoint = firePointGO.transform;

        // Set explosion prefab (we'll create a simple one later, but for now leave it null and add a warning)
        // We'll create a simple explosion prefab in another step.

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Enemies/ShooterEnemy.prefab";
        PrefabUtility.SaveAsPrefabAsset(enemyGO, prefabPath);

        // Clean up
        DestroyImmediate(enemyGO);

        Debug.Log("ShooterEnemy prefab created at: " + prefabPath);
    }

    [MenuItem("Game Setup/Create Kamikaze Enemy Prefab")]
    public static void CreateKamikazeEnemyPrefab()
    {
        GameObject enemyGO = new GameObject("KamikazeEnemy");
        enemyGO.AddComponent<SpriteRenderer>();
        enemyGO.AddComponent<KamikazeEnemy>();

        // Set the enemy sprite (we'll use the Nairan Scout Base for example)
        string enemySpritePath = "Assets/Enemy Sprites/Foozle_2DS0013_Void_EnemyFleet_2/Nairan/Designs - Base/PNGs/Nairan - Scout - Base.png";
        Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(enemySpritePath);
        if (enemySprite != null)
        {
            enemyGO.GetComponent<SpriteRenderer>().sprite = enemySprite;
        }
        else
        {
            Debug.LogWarning("Enemy sprite not found at: " + enemySpritePath);
        }

        // KamikazeEnemy doesn't shoot, so no bullet prefab needed.

        // Set explosion prefab (same as above)

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Enemies/KamikazeEnemy.prefab";
        PrefabUtility.SaveAsPrefabAsset(enemyGO, prefabPath);

        // Clean up
        DestroyImmediate(enemyGO);

        Debug.Log("KamikazeEnemy prefab created at: " + prefabPath);
    }

    [MenuItem("Game Setup/Create Explosion Prefab")]
    public static void CreateExplosionPrefab()
    {
        // For simplicity, we'll create a prefab that just plays an animation or shows a sprite for a short time.
        // We don't have an explosion sprite, so we'll create a placeholder that uses the player ship's damaged sprite or just a circle.
        // Alternatively, we can use one of the destruction sprites from the enemy assets.

        // Let's use the first destruction sprite of the Nairan Fighter as an example.
        string explosionSpritePath = "Assets/Enemy Sprites/Foozle_2DS0013_Void_EnemyFleet_2/Nairan/Destruction/PNGs/Nairan - Fighter - Destruction.png";
        Sprite explosionSprite = AssetDatabase.LoadAssetAtPath<Sprite>(explosionSpritePath);
        if (explosionSprite == null)
        {
            Debug.LogWarning("Explosion sprite not found at: " + explosionSpritePath);
            // Create a simple white circle as fallback? We'll just leave it null and the enemy will not show explosion.
        }

        GameObject explosionGO = new GameObject("Explosion");
        explosionGO.AddComponent<SpriteRenderer>();
        if (explosionSprite != null)
        {
            explosionGO.GetComponent<SpriteRenderer>().sprite = explosionSprite;
        }
        // We'll also add a script to destroy the explosion after a short time, but for now we'll just let it be destroyed by the enemy script's timing.
        // Actually, in the EnemyController.Die() we instantiate the explosion and then destroy the enemy. We want the explosion to last a bit.
        // Let's add a simple script that destroys the game object after 0.5 seconds.
        explosionGO.AddComponent<DestroyAfterTime>();

        // Create the prefab
        string prefabPath = "Assets/Prefabs/Effects/Explosion.prefab";
        PrefabUtility.SaveAsPrefabAsset(explosionGO, prefabPath);

        // Clean up
        DestroyImmediate(explosionGO);

        Debug.Log("Explosion prefab created at: " + prefabPath);
    }

    [MenuItem("Game Setup/Setup Scene")]
    public static void SetupScene()
    {
        // Create a new scene
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Create GameManager
        GameObject gmGO = new GameObject("GameManager");
        gmGO.AddComponent<GameManager>();
        // Assign references (we'll do this by finding the prefabs and setting them)
        // We'll do this after creating the prefabs, but for now we'll leave them null and the user can assign in the inspector.

        // Create UIManager
        GameObject uiGO = new GameObject("UIManager");
        uiGO.AddComponent<UIManager>();

        // Create WaveSpawner
        GameObject wsGO = new GameObject("WaveSpawner");
        wsGO.AddComponent<WaveSpawner>();

        // Create VersusModeManager
        GameObject vmGO = new GameObject("VersusModeManager");
        vmGO.AddComponent<VersusModeManager>();

        // Create two cameras for versus mode (we'll set them up later)
        GameObject p1CamGO = new GameObject("Player1Camera");
        p1CamGO.AddComponent<Camera>();
        p1CamGO.tag = "Player1Camera";

        GameObject p2CamGO = new GameObject("Player2Camera");
        p2CamGO.AddComponent<Camera>();
        p2CamGO.tag = "Player2Camera";

        // Create a simple background (optional)
        GameObject bgGO = new GameObject("Background");
        bgGO.AddComponent<SpriteRenderer>();
        // Set a background sprite (we'll use one of the nebula sprites)
        string bgSpritePath = "Assets/DinV/Dynamic Space Background/Sprites/Nebula Aqua-Pink.png";
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(bgSpritePath);
        if (bgSprite != null)
        {
            bgGO.GetComponent<SpriteRenderer>().sprite = bgSprite;
            bgGO.transform.position = new Vector3(0f, 0f, 1f); // Behind everything
            bgGO.transform.localScale = new Vector3(20f, 20f, 1f); // Make it big
        }

        // Save the scene
        string scenePath = "Assets/Scenes/GameScene.unity";
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);

        Debug.Log("Scene setup complete. Please assign the prefab references in the GameManager and WaveSpawner components.");
    }
}
