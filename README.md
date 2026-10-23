# Unity Gameplay Systems: Menus and Collectables

Game flow and interaction systems in Unity: a post-processed start menu, an in-game pause menu, a trigger-based collectable and a trigger-driven animated prefab that reverses smoothly.

This branch builds on [`physics-simulation`](../../tree/physics-simulation). See [Branches](#branches) for the rest of the project.

## Features

### Start menu with post-processing
- `GameMenuScene` is the first scene in the build. Its camera frames the physics area, so the elevator and jumping bean move behind the menu.
- Uses the Post Processing package with a global volume: **Depth of Field** (focus distance 2, aperture 5, focal length 50, max blur Very Large) and **Grain** (intensity 1, size 2). The profile is in `Assets/Scenes/GameMenuScene_Profiles/`.
- **Start Game** uses `GameStarter`, which loads the level and resets `Time.timeScale` to 1. **Exit** quits the game.
- All canvases use **Scale With Screen Size** (1920×1080), so the UI stays readable on high-DPI displays.

### In-game pause menu
- **Escape** toggles a centered pause panel and freezes the game.
- It sits on its own canvas, so hiding it doesn't affect the HUD. `PauseMenuToggle` flips a `CanvasGroup`'s visibility and interactivity and sets `Time.timeScale` to 0 or 1.
- **Restart Level** reuses `GameStarter` to reload the level. **Quit Game** exits.

### Trigger-based collectable
- A pink sphere with a trigger collider. In `OnTriggerEnter`, `CollectableBall` checks whether the entering rigidbody has a `BallCollector`. If it does, the ball calls `ReceiveBall()`, plays a sound through the audio event system, and destroys itself.
- Only the root-motion character has a `BallCollector`, so the other characters walk through the ball without picking it up. You can watch **Has Ball** flip in the Inspector.

### Trigger-driven animated prefab (alien plants)
- `AlienPlant.prefab` has an Animator, a radius-3 trigger collider and `PlantRetractTrigger` on the root. The visible mesh sits under a child object, so the animation works wherever the prefab is placed.
- A custom Animator Controller with a 1D blend tree on a `Retract` float blends between `PlantOut` (full size) and `PlantHidden` (squashed into the ground).
- The script counts the characters inside the trigger and moves `Retract` toward 1 or 0 a little each frame. The plant can reverse mid-animation without snapping.

## Controls

| Input | Action |
|---|---|
| W / A / S / D | Move and turn |
| T | Switch character |
| Escape | Open / close pause menu |
| X | Quit |

To pick up the ball, press **T** once to switch to the root-motion character, then walk forward into the pink sphere. Walk near any plant to make it duck.

## Running it
1. Clone the repo, check out this branch (`git checkout menus-and-collectables`) and add the folder in Unity Hub.
2. Open `Assets/Scenes/GameMenuScene.unity` and press Play.

Built with Unity 6 (6000.0.82f1). Uses the Unity Post Processing package and TextMeshPro.

## Branches

Each branch builds on the previous one.

| Branch | Focus |
|---|---|
| [`character-animation`](../../tree/character-animation) | Root motion, blend trees, target matching, IK, animation events |
| [`physics-simulation`](../../tree/physics-simulation) | Rigidbodies, joints, compound colliders, physics materials, ragdoll |
| [`menus-and-collectables`](../../tree/menus-and-collectables) | Start and pause menus, post-processing, trigger pickups, trigger-driven animation |
| [`ai-navigation`](../../tree/ai-navigation) (also `main`) | NavMesh agent, waypoint patrol, moving-target interception, AI state machine |
