# Unity Gameplay Systems: Physics Simulation

A Unity physics sandbox: rigidbody stacking, layer-based collision filtering, compound colliders, joint chains, a kinematic animated platform, custom center of mass, physics materials, an autonomous jumping object and a ragdoll.

This branch builds on [`character-animation`](../../tree/character-animation). See [Branches](#branches) for the rest of the project.

## Features

### Rigidbody stacks and collision events
- Three blue spheres stacked with a slight offset so they topple and roll off one another under gravity.
- `BallCollisionReporter` fires a bounce event on each collision, reusing the project's audio event system to play a sound.

### Layer collision filtering
- Three red spheres on a dedicated physics layer, with `Physics.IgnoreLayerCollision` set for that layer against itself. They fall into and through one another instead of pushing apart.

### Compound collider
- An imported mask model (scaled 10×) uses a compound collider instead of a `MeshCollider`: a box for the head and ears, a box for the cheeks and a capsule for the snout, sized from the mesh bounds. There's a single `Rigidbody` on the root.
- It starts tilted and slightly raised, so it falls and lands nose-first on the ground.

### Jointed chain
- Six links connected with `ConfigurableJoint`s. Linear motion is locked at each pivot, angular swing is free, and twist is locked along each link's long axis so the chain swings without spinning.
- Anchored to a fixed point and started about 18° off vertical, so it swings when play starts.

### Animated kinematic elevator
- A looping `localPosition.y` animation (0 → 3 → 0 over 3 s) drives a kinematic `Rigidbody`. The Animator uses **Update Mode: Fixed** with **Animate Physics** on, so the platform pushes and carries other rigidbodies. A sphere rides on top of it.

### Self-righting "weeble"
- A capsule whose `RigidbodyCenterOfMass` is set well below the center of its rounded bottom, so gravity always produces a righting torque. It starts tilted 20° and stands back up after being knocked.

### Physics materials
- On a ramp, a box with the default material stays put while one with a custom low-friction material (0.1 / 0.1, Minimum combine) slides down.
- A sphere with a custom bouncy material (bounciness 0.9, Maximum combine) bounces on landing.

### Autonomous jumping bean
- `JumpingBean` checks for ground with a raycast. While grounded, it applies a random upward and sideways impulse plus a small random torque at random intervals. Nobody controls it.

### Ragdoll
- A 13-body ragdoll on a rigged humanoid, matching the bone set of Unity's Ragdoll wizard: pelvis, middle spine, head, and both leg and arm chains.
- Each bone has a collider sized from its bone-to-child distance, a `Rigidbody` and a `CharacterJoint` to its parent. All bones share a ragdoll layer with self-collision disabled.
- It's placed leaning over a hurdle so it topples across it when play starts.

### Start paused
- `PauseController` sets `Time.timeScale = 0` on start so the initial setup can be inspected. **P** toggles pause.
- Everything is laid out to fit within the starting camera's 16:9 view.

## Controls

| Input | Action |
|---|---|
| P | Pause / unpause (the scene starts paused) |
| W / A / S / D | Move and turn the character |

## Running it
1. Clone the repo, check out this branch (`git checkout physics-simulation`) and add the folder in Unity Hub.
2. Open `Assets/Scenes/demo.unity`, press Play, then press **P** to unpause.

Built with Unity 6 (6000.0.82f1). Uses a free third-party "Japanese Mask" model (`Assets/Japanese Mask`).

## Branches

Each branch builds on the previous one.

| Branch | Focus |
|---|---|
| [`character-animation`](../../tree/character-animation) | Root motion, blend trees, target matching, IK, animation events |
| [`physics-simulation`](../../tree/physics-simulation) | Rigidbodies, joints, compound colliders, physics materials, ragdoll |
| [`menus-and-collectables`](../../tree/menus-and-collectables) | Start and pause menus, post-processing, trigger pickups, trigger-driven animation |
| [`ai-navigation`](../../tree/ai-navigation) (also `main`) | NavMesh agent, waypoint patrol, moving-target interception, AI state machine |
