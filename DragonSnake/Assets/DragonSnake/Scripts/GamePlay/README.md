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