# DragonSnake VR - Apple Spawning System

## Apple System Components

### Apple.cs
- Represents individual apples that can be eaten by the snake
- Handles collision detection with snake segments
- Awards points when eaten
- Auto-destroys after a configurable lifetime

### AppleSpawner.cs
- Manages apple spawning at regular intervals (default: 5 seconds)
- Ensures apples spawn in free space (no collisions)
- Limits maximum number of apples on field
- Integrates with game state system (only spawns during Playing state)

## Setup Instructions

1. **Create Apple Prefab:**
   - Create a sphere GameObject
   - Scale it to match snake segment radius
   - Add a Collider component (set as Trigger)
   - Add the `Apple` script
   - Tag it appropriately for collision detection
   - Save as prefab

2. **Setup AppleSpawner:**
   - Add `AppleSpawner` script to a GameObject in your scene
   - Assign the apple prefab to the `applePrefab` field
   - Configure spawn settings (interval, max apples, etc.)
   - Set up layer masks for obstacle detection

3. **Configure Snake Segments:**
   - Ensure snake segments have appropriate tags ("SnakeHead", "Snake")
   - Add colliders to snake segments if not already present

4. **Layer Setup:**
   - Create layers for different object types (Snake, Apple, Obstacles)
   - Configure the `obstacleLayerMask` in AppleSpawner to detect walls/obstacles

## Configuration Options

- **Spawn Interval**: How often apples spawn (default: 5 seconds)
- **Max Apples**: Maximum apples on field simultaneously
- **Apple Lifetime**: How long apples persist if not eaten
- **Score Value**: Points awarded when apple is eaten
- **Collision Detection**: Configurable radius and layer masks

## Integration Notes

- Apples only spawn during `GameState.Playing`
- All apples are cleared when game state changes (pause, game over, etc.)
- Score is automatically added to GameManager when apples are eaten
- System uses singleton pattern for easy access from other components


# DragonSnake VR - UI System

## UI Architecture

The UI system consists of:

- **GameUI**: Manages UI windows and button interactions based on game state
- **HUD**: Displays game information (score, lives) and state-specific overlays
- **UI Windows**: Separate UI panels for different game states

## UI Windows

### Start Game Window
- Shown in `NotStarted` and `GameOver` states
- Contains "START GAME" button
- Should be a child of your main Canvas

### Pause Menu Window
- Shown in `Paused` state
- Contains "Resume", "Finish Session", and "Quit" buttons
- Should be a child of your main Canvas

### In-Game Controls
- Pause button visible only during `Playing` state
- Can be positioned as a floating UI element in VR space

## Setup Instructions

1. **Create UI Canvas** (World Space recommended for VR)
2. **Create Start Game Window**:
   - Add Panel as child of Canvas
   - Add "START GAME" button inside the panel
3. **Create Pause Menu Window**:
   - Add Panel as child of Canvas
   - Add "Resume", "Finish Session", and "Quit" buttons inside
4. **Create In-Game Pause Button**:
   - Add Button as child of Canvas or as floating UI
5. **Attach GameUI Script**:
   - Assign all window GameObjects and buttons in Inspector
6. **The system will automatically show/hide windows based on game state**

## Notes

- All UI state management is automatic based on GameManager state changes
- Windows are hidden by default and shown only when appropriate
- The pause button is only visible during gameplay
- UI follows VR best practices with World Space Canvas

# DragonSnake VR - State Machine Architecture

## Game States

The game uses a state machine for robust, modular logic. The main states are:

1. **NotStarted**: Game session not started. Shows "START GAME" button.
2. **Countdown**: Countdown before level/game session starts. Shows level number and countdown.
3. **Playing**: Player rides the snake.
4. **Paused**: In-game menu with options to resume, finish session, or quit.
5. **LevelCompleted**: Player completed the level.
6. **LevelFailed**: Player lost a life but has lives left.
7. **GameOver**: Player lost all lives.

## How It Works

- `GameManager` manages the state machine and exposes events for state changes.
- `SnakeController` only moves the snake in the `Playing` state.
- `HUD` updates UI elements and overlays based on the current state.
- `GameUI` handles button presses and calls `GameManager` methods.

## Integration Steps

1. Add `GameManager`, `SnakeController`, `HUD`, and `GameUI` to your scene.
2. Wire up UI buttons to the `GameUI` script.
3. Assign TextMeshProUGUI fields in `HUD` for score, lives, and overlay.
4. The system will handle state transitions and UI automatically.

## Notes

- You can add more states or transitions as needed.
- All state transitions are event-driven and modular.
- Extend the state machine for more complex game logic as your project grows.


# DragonSnake VR - Modular Game Architecture

## Structure

- **GameManager**: Handles game state, score, lives, level restart, and game over logic.
- **GameArea**: Defines and checks the rectangular game area boundaries.
- **SnakeController**: Handles snake movement and notifies GameManager if the snake leaves the game area.
- **SnakeSegmentPool**: Object pool for snake segments.
- **SnakeGameTick**: Provides a fixed tick event for game logic.

# DragonSnake VR - Snake Movement Overview

## Snake Movement

- The snake's head is controlled by the player's head (XR camera) direction, moving smoothly on the horizontal plane.
- The snake's body consists of spherical segments (radius=1), each following the previous segment at a fixed distance.
- The movement is processed at a fixed 60Hz tick using an event-driven system (`SnakeGameTick`), not via `Update`.
- The snake starts with 5 segments, positioned as described in the game design.
- **The XR Origin (XR Rig) is moved every tick so that the Main Camera's X and Z coordinates match the snake's head, giving the player the sensation of riding the snake.**
- **The XR Origin alignment now uses `playerHead.position` (world position) for robust handling of any XR rig hierarchy or scaling.**

## How to Use

1. **Add `SnakeGameTick` to your scene** (one instance only).
2. **Add `SnakeSegmentPool` to your scene** (one instance only):
    - Assign your segment prefab (sphere, radius=1) to the `segmentPrefab` field.
    - Set the initial pool size as needed for your expected maximum snake length.
3. **Add `GameManager` to your scene** (one instance only).
4. **Add `GameArea` to your scene** (one instance only).
    - Set `areaMin` and `areaMax` in the inspector to define the game rectangle.
5. **Add `SnakeController` to an empty GameObject.**
    - Assign references for `playerHead`, `xrOrigin`, and `segmentPrefab`.
6. Play the scene. The snake will move in the direction the player is looking (on the horizontal plane), and the player will feel as if they are riding on the snake's head. The modular system will handle movement, boundaries, lives, and level restarts.

## Notes

- The system is modular and event-driven, suitable for VR and high-performance requirements.
- Each class has a single responsibility, making the codebase easier to maintain and extend.
- All boundary checks and game logic are event-driven and modular.
- No direct use of `Update` in gameplay logic.
- **Snake segments are managed using an object pool (`SnakeSegmentPool`) to avoid performance issues from frequent instantiation and destruction.**
- **XR Origin is moved every tick to keep the Main Camera on the snake's head, using world positions for robust alignment.**
- Designed for Meta Quest 3 and portable to other VR platforms.
# DragonSnake VR - HUD Integration

## HUD (Heads-Up Display)

- The HUD shows the player's current score and remaining lives.
- It automatically updates when lives change, the level restarts, or the game is over.

### How to Use

1. Add a Canvas to your scene (set to World Space or Screen Space as appropriate for VR).
2. Add two TextMeshProUGUI elements for score and lives.
3. Create an empty GameObject and attach the `HUD` script.
4. Assign the TextMeshProUGUI references (`txtScore`, `txtLives`) in the Inspector.
5. Ensure the `GameManager` is present in the scene.

## Example Hierarchy

Canvas (World Space)
├── ScoreText (TextMeshProUGUI)
└── LivesText (TextMeshProUGUI)
HUD (attach to Canvas or another GameObject)


## Notes

- The score logic is a placeholder; update it when you implement scoring.
- The HUD listens to `GameManager` events for automatic updates.
- Make sure to assign the references in the Inspector.
