# DragonSnake VR - Snake Movement Overview

## Snake Movement

- The snake's head is controlled by the player's head (XR camera) direction, moving smoothly on the horizontal plane.
- The snake's body consists of spherical segments (radius=1), each following the previous segment at a fixed distance.
- The movement is processed at a fixed 60Hz tick using an event-driven system (`SnakeGameTick`), not via `Update`.
- The snake starts with 5 segments, positioned as described in the game design.
- **The XR Origin (XR Rig) is moved every tick so that the Main Camera's X and Z coordinates match the snake's head, giving the player the sensation of riding the snake.**

## How to Use

1. **Add `SnakeGameTick` to your scene** (one instance only).
2. **Add `SnakeSegmentPool` to your scene** (one instance only):
    - Assign your segment prefab (sphere, radius=1) to the `segmentPrefab` field.
    - Set the initial pool size as needed for your expected maximum snake length.
3. **Add `SnakeController` to an empty GameObject.**
4. Assign the XR camera (player head, e.g., "Main Camera") to the `playerHead` field in `SnakeController`.
5. Assign the XR Origin (XR Rig) GameObject to the `xrOrigin` field in `SnakeController`.
6. Assign the same segment prefab to the `segmentPrefab` field in `SnakeController`.
7. Play the scene. The snake will move in the direction the player is looking (on the horizontal plane), and the player will feel as if they are riding on the snake's head.

## Notes

- The system is modular and event-driven, suitable for VR and high-performance requirements.
- No direct use of `Update` in gameplay logic.
- **Snake segments are managed using an object pool (`SnakeSegmentPool`) to avoid performance issues from frequent instantiation and destruction.**
- **XR Origin is moved every tick to keep the Main Camera on the snake's head.**
- Designed for Meta Quest 3 and portable to other VR platforms.