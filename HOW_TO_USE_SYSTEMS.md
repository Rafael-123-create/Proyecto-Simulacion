# How to Use the Game Systems

This guide explains how to set up and use the UI, Game Management, and Utility systems in your Unity project.

## 1. UI System (UIManager.cs)

### Purpose
Handles all on-screen text, panels, and visual feedback for the game.

### Setup Instructions

1. **Create a Canvas**:
   - In Unity Hierarchy: Right-click → UI → Canvas
   - Set Canvas Render Mode to "Screen Space - Overlay"
   - Set UI Scale Mode to "Scale With Screen Size"
   - Set Reference Resolution to your desired base resolution (e.g., 1920x1080)

2. **Add UIManager Component**:
   - Select the Canvas GameObject
   - Click "Add Component" → Search for "UIManager" → Add it

3. **Configure UI Elements**:
   In the UIManager component in the Inspector, you'll need to assign:
   
   **Text Elements** (Create these as UI → Text - TextMeshPro):
   - Player 1 Score Text → Assign to `player1ScoreText`
   - Player 2 Score Text → Assign to `player2ScoreText`
   - Player 1 Lives Text → Assign to `player1LivesText`
   - Player 2 Lives Text → Assign to `player2LivesText`
   - Level Text → Assign to `levelText`
   - Get Ready Text → Assign to `getReadyText`
   - Level Complete Text → Assign to `levelCompleteText`
   - Game Over Text → Assign to `gameOverText`
   - Winner Text → Assign to `winnerText`

   **Panel Elements** (Create these as UI → Panel):
   - Get Ready Panel → Assign to `getReadyPanel`
   - Level Complete Panel → Assign to `levelCompletePanel`
   - Game Over Panel → Assign to `gameOverPanel`

4. **UI Hierarchy Suggestion**:
   ```
   Canvas
   ├── Player1Score (Text)
   ├── Player2Score (Text)
   ├── Player1Lives (Text)
   ├── Player2Lives (Text)
   ├── LevelText (Text)
   ├── GetReadyPanel (Panel)
   │   └── GetReadyText (Text)
   ├── LevelCompletePanel (Panel)
   │   └── LevelCompleteText (Text)
   ├── GameOverPanel (Panel)
   │   ├── GameOverText (Text)
   │   └── WinnerText (Text)
   ```

### How to Use in Code

The UIManager is a singleton, so you can access it from anywhere:

```csharp
// Update player 1 score
UIManager.Instance.UpdateScore(1, 150);

// Update player 2 lives
UIManager.Instance.UpdateLives(2, 2);

// Set level text
UIManager.Instance.SetLevelText(3);

// Show UI panels
UIManager.Instance.ShowGetReadyUI();
UIManager.Instance.ShowLevelCompleteUI(1200, 950);
UIManager.Instance.ShowGameOverUI(1200, 950, true); // versusMode = true

// Hide panels
UIManager.Instance.HideGetReadyUI();
UIManager.Instance.HideLevelCompleteUI();
```

## 2. Game Management System (GameManager.cs)

### Purpose
Central manager for game state, scoring, lives, level progression, and game flow.

### Setup Instructions

1. **Create GameManager GameObject**:
   - In Unity Hierarchy: Right-click → Create Empty
   - Name it "GameManager"
   - Add Component → Search for "GameManager" → Add it

2. **Assign References**:
   In the GameManager component in the Inspector, you need to assign:
   
   - **Wave Spawner**: Drag the WaveSpawner GameObject here
   - **UI Manager**: Drag the UIManager GameObject here
   - **Versus Mode Manager**: Drag the VersusModeManager GameObject here
   - **Player 1 Prefab**: Drag your Player prefab here
   - **Player 2 Prefab**: Drag your Player prefab here (same prefab works for both)
   - **Spawn Points**: Create empty GameObjects at the top of your screen area and assign them to the array

3. **Configure Game Settings**:
   - Total Levels: How many levels your game has (default 3)
   - Level Wait Time: Seconds to wait between levels (default 3)
   - Game Over Wait Time: Seconds to wait before showing game over (default 3)
   - Starting Lives: How many lives each player starts with (default 3)

### How It Works

The GameManager handles:
- Game initialization and state tracking
- Score management for both players
- Life management for both players
- Level progression and wave completion
- Versus mode activation/deactivation
- Game over conditions

### Key Methods You Can Call

From other scripts, you can call:

```csharp
// Add score to a player
GameManager.Instance.AddScore(playerNumber, pointsToAdd);

// Take a life from a player
GameManager.Instance.TakeLife(playerNumber);

// Check game state
if (GameManager.Instance.IsVersusMode()) {
    // Versus mode specific logic
}

if (GameManager.Instance.IsGameOver()) {
    // Game over handling
}

// Get current scores/lives for display or logic
int p1Score = GameManager.Instance.GetPlayerScore(1);
int p2Lives = GameManager.Instance.GetPlayerLives(2);
```

### Events

The GameManager exposes events you can subscribe to:

```csharp
void OnEnable() {
    GameManager.Instance.OnScoreChanged += OnScoreChanged;
    GameManager.Instance.OnLivesChanged += OnLivesChanged;
}

void OnDisable() {
    GameManager.Instance.OnScoreChanged -= OnScoreChanged;
    GameManager.Instance.OnLivesChanged -= OnLivesChanged;
}

void OnScoreChanged(int playerNumber, int newScore) {
    // Update your custom score display or trigger effects
}

void OnLivesChanged(int playerNumber, int newLives) {
    // Update life display or trigger effects when lives change
}
```

## 3. Utility System (Utils/)

### Purpose
Contains reusable helper components and scripts.

### Current Utilities

#### Bullet.cs (in Utils/)
Handles projectile behavior for both players and enemies.

**Setup:**
1. Create a Bullet prefab:
   - Create empty GameObject
   - Add SpriteRenderer component
   - Add Bullet component (from Utils/Bullet.cs)
   - Assign your bullet sprite to the SpriteRenderer
   - Drag the GameObject to Assets/Prefabs/Bullets/ to create a prefab

**Configuration in Inspector:**
- Speed: How fast the bullet moves (default 10)
- Damage: How much damage it does (default 1)
- Is Player Bullet: Check if this is a player bullet (uncheck for enemy bullets)

**How It Works:**
- Automatically moves upward each frame
- Destroys itself when going off-screen (top)
- Can detect collisions with players/enemies via tags

#### DestroyAfterTime.cs (in Utils/)
Simple component that destroys a GameObject after a set time.

**Usage:**
1. Add to any GameObject (like explosion effects)
2. Set the lifetime in seconds in the Inspector
3. The object will automatically destroy itself after that time

**Perfect for:** Explosion effects, temporary power-ups, etc.

## 4. Complete Setup Workflow

### Step-by-Step Setup:

1. **Create Essential Managers**:
   - Create GameObject named "GameManager" → Add GameManager component
   - Create GameObject named "UIManager" → Add UIManager component
   - Create GameObject named "WaveSpawner" → Add WaveSpawner component
   - Create GameObject named "VersusModeManager" → Add VersusModeManager component

2. **Set Up Cameras for Versus Mode**:
   - Create two Camera GameObjects:
     - Name one "Player1Camera", tag it "Player1Camera"
     - Name one "Player2Camera", tag it "Player2Camera"
   - Position both at (0,0,-10) for 2D game
   - Set Clear Flags to "Solid Color" with black background
   - Set Depth: Player1Camera = 0, Player2Camera = 1

3. **Create Player Prefab**:
   - Create empty GameObject named "Player"
   - Add SpriteRenderer component → Assign your ship sprite
   - Add PlayerController component
   - Create empty child named "FirePoint" → Position it where bullets should spawn
   - In PlayerController, assign the FirePoint transform to the firePoint field
   - Drag the Player GameObject to Assets/Prefabs/Players/ to create prefab

4. **Create Enemy Prefabs**:
   - Repeat similar process for ShooterEnemy and KamikazeEnemy
   - Assign appropriate sprites and configure bullet references

5. **Create Bullet Prefab**:
   - Use the Tools → Create Bullet Prefab menu item
   - Or create manually as described above

6. **Set Up Spawn Points**:
   - Create empty GameObjects at the top of your screen area
   - Position them where enemies should spawn from
   - Assign them to the WaveSpawner's spawnPoints array

7. **Wire Up References**:
   - In GameManager:
     - Assign WaveSpawner reference
     - Assign UIManager reference
     - Assign VersusModeManager reference
     - Assign Player 1 and 2 prefabs
     - Assign Spawn Points array
   - In WaveSpawner:
     - Add enemy types to the list with their prefabs and weights
   - In UIManager:
     - Assign all the Text and Panel references as described above

8. **Set Up Tags**:
   - Make sure you have these tags defined (Edit → Project Settings → Tags and Layers):
     - "Player"
     - "PlayerBullet"
     - "Enemy"
     - "EnemyBullet"

9. **Test the Setup**:
   - Press Play
   - You should see:
     - UI elements showing scores, lives, level
     - Players spawning at bottom left/right
     - Enemies spawning from top
     - Players able to move and shoot
     - Enemies shooting back (Shooter type) or charging (Kamikaze type)
     - Proper collision detection and destruction
     - Score updates when enemies are destroyed
     - Level progression after waves complete

## 5. Customization Tips

### Adjusting Difficulty:
- In WaveSpawner.cs: Adjust spawn rates, enemy types, and difficulty increase intervals
- In EnemyController.cs: Modify speed, health, and score values for each enemy type
- In PlayerController.cs: Adjust moveSpeed, fireRate, and laneChangeSpeed

### Customizing UI:
- Modify the text formats in UIManager.cs to change how scores/lives are displayed
- Change colors, fonts, and sizes of UI elements in the Canvas
- Add animations to panels for better feedback

### Adding New Features:
- Add new enemy types by extending EnemyController
- Add power-ups by creating new scripts that modify player stats temporarily
- Add different weapon types by creating different bullet prefabs
- Add background music and sound effects using AudioSource components

## 6. Troubleshooting

### Common Issues:

1. **Null Reference Exceptions**:
   - Check that all references in the Inspector are properly assigned
   - Make sure prefabs exist before assigning them

2. **Bullets Not Moving**:
   - Verify the Bullet script is assigned to the bullet prefab
   - Check that the sprite renderer is visible and not blocked by other layers

3. **Enemies Not Spawning**:
   - Check that WaveSpawner is enabled
   - Verify spawn points are positioned correctly (above the screen)
   - Confirm enemy prefabs are assigned in the WaveSpawner list

4. **UI Not Updating**:
   - Confirm all Text references are assigned in UIManager
   - Check that the Canvas is set to Screen Space - Overlay
   - Verify the GameObject with UIManager is active in the scene

5. **Versus Mode Not Splitting Screen**:
   - Ensure you have two cameras tagged correctly
   - Check that VersusModeManager is finding and configuring the cameras
   - Verify GameManager.IsVersusMode() returns true when it should

By following this guide, you should be able to successfully set up and use all the systems we've created for your 2-player versus Galaga-style game. Don't hesitate to experiment and adjust values to get the gameplay feel you want!