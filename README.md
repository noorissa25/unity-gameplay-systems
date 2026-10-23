# Unity Gameplay Systems

A collection of gameplay systems built in Unity 6 and C#: character animation, physics simulation, menus and interaction, and NavMesh-based AI. Each system was built on top of the previous one in a single evolving project, and each stage has its own branch.

This branch (`main` / `ai-navigation`) is the latest stage and contains everything below.

## Branches

| Branch | Focus |
|---|---|
| [`character-animation`](../../tree/character-animation) | Root-motion locomotion, walk/run blend trees, `MatchTarget`, IK, animation events |
| [`physics-simulation`](../../tree/physics-simulation) | Rigidbodies, joints, compound colliders, physics materials, center-of-mass tuning, ragdoll |
| [`menus-and-collectables`](../../tree/menus-and-collectables) | Post-processed start menu, pause menu, trigger pickups, trigger-driven blend-tree animation |
| [`ai-navigation`](../../tree/ai-navigation) (= `main`) | NavMesh agent, waypoint patrol, moving-target interception, AI state machine |

Each branch's README describes that stage in detail.

## AI navigation

An AI minion runs a continuous loop with no player input:
1. It patrols five static waypoints in order.
2. It crosses the arena to **intercept** a waypoint sliding back and forth along a wall, aiming for where it will be rather than where it is.
3. It returns to the first waypoint and repeats. A full loop takes about 28–30 seconds.

### NavMesh setup
- A `NavMeshSurface` (AI Navigation package) is baked on the arena. Waypoints, markers, characters and decorative props use `NavMeshModifier` set to **Remove Object** so they don't cut holes in the mesh.
- The minion is a kinematic-rigidbody character with a `NavMeshAgent` (speed 7, angular speed 360, acceleration 16, base offset 0.75 so it stands on the floor).

### Waypoint patrol
- `MinionAI` holds the waypoint array. It advances only when `!agent.pathPending` and `remainingDistance` is within 0.5, wraps back to the start, and logs an error if the array is empty.

### Moving-target interception
- The moving waypoint is animated with Mecanim using linear tangents, so it travels at a constant 3 units/s. Because it isn't physics-driven, `VelocityReporter` derives a smoothed velocity from its frame-to-frame movement.
- Each frame the minion predicts where to intercept:
  - `lookAhead = clamp(distance / agent.speed, 0, 3 s)`
  - `predicted = target.position + target.velocity * lookAhead`
- `NavMesh.Raycast` pulls the prediction back if it would leave the mesh, and `NavMesh.SamplePosition` snaps it onto the mesh, so `SetDestination` always gets a valid point.
- An intercept only counts when the minion is within 1 unit of the **actual** target across the floor, not when it reaches the predicted point.
- A cyan marker shows the current predicted destination in real time.

### Procedural state machine
- An `AIState` enum (`PatrolStaticWaypoints`, `InterceptMovingWaypoint`) drives behavior through a `switch` in `Update`, separate from the Animator's state machine. Each completed loop logs its duration to the Console.

### Animation sync
- `vely = agent.velocity.magnitude / agent.speed` feeds the locomotion blend tree, so the hop animation matches actual movement speed.

## Highlights from earlier stages
- **Character animation:** walk/run blend trees, scaled root motion in `OnAnimatorMove`, `Animator.MatchTarget` alignment, curve-weighted hand IK and look-at, animation-event footsteps.
- **Physics:** `ConfigurableJoint` chain, compound colliders, kinematic animated elevator that carries rigidbodies, self-righting custom center of mass, friction and bounce materials, 13-bone ragdoll.
- **Menus and interaction:** start menu with Depth of Field and Grain post-processing, `CanvasGroup` pause menu with `Time.timeScale`, trigger collectable gated by a collector component, trigger-driven plant that blends in and out without snapping.

## Controls

The AI runs on its own. The rest of the scene stays playable:

| Input | Action |
|---|---|
| W / A / S / D | Move and turn the selected character |
| T | Switch character |
| C | Toggle between the overhead view and a follow camera behind the current character |
| Escape | Pause menu |
| X | Quit |

## Running it
1. Clone the repo and add the folder in Unity Hub (Unity 6000.0.82f1).
2. Open `Assets/Scenes/demo.unity` and press Play. A fixed overhead camera shows the whole arena.

Uses the Unity AI Navigation, Post Processing and TextMeshPro packages, plus a free third-party "Japanese Mask" model (`Assets/Japanese Mask`).
