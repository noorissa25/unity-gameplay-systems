# Unity Gameplay Systems

A third-person character animation and control sandbox built in Unity. It covers root-motion and code-driven locomotion, blend trees, animation events, target matching and inverse kinematics.

The project started from a provided character-control starter scene. The systems below are the ones I implemented or extended on top of it.

## Features

### Root-motion locomotion with walk/run blending
- Extended the forward-movement blend tree in `SimpleAnimatorController` with run, run-left and run-right motions, replacing a placeholder sped-up walk clip.
- Moved the walk-turn clips closer to the blend origin and placed the run clips further out. The character walks at about a quarter of full input, runs at full input in any direction (including mid-turn), and blends smoothly in between.

### Tunable root-motion speed
- Added inspector-exposed `animationSpeed`, `rootMovementSpeed` and `rootTurnSpeed` scalars to `RootMotionControlScript` (default `1`).
- `animationSpeed` drives `Animator.speed`. The other two scale the root-motion position and rotation deltas in `OnAnimatorMove()` using `Vector3.LerpUnclamped` / `Quaternion.LerpUnclamped`, so movement can be made faster and more responsive without the feet visibly sliding.

### Code-driven turning (non-root-motion character)
- The non-root-motion character turns with `Rigidbody.MoveRotation` while the walk-forward animation keeps playing, instead of switching to a separate turn animation.

### Target matching and IK for interactions
- When the character is close to (but not exactly at) a button, it transitions through a `MatchToButtonPress` state and uses `Animator.MatchTarget()` to land precisely on the standing spot before pressing.
- Added `OnAnimatorIK()` so that during the `ButtonPress` state the character looks at and reaches toward the button handle with right-hand IK.
- IK and look-at weights are driven by the animation's own `buttonClose` curve, so the reach blends in and out with the motion instead of snapping.

### Procedural hop animation and footstep events
- Created hop-and-twist variants of a creature's forward, forward-left and forward-right walk cycles and wired them into its blend tree.
- Keyframed only the child model transform (hop height `0 → 0.5 → 0 → 0.5 → 0`, twist `0° → 10° → 0° → -10° → 0°` across the clip), leaving the parent's root-motion curves untouched so locomotion is unaffected.
- Added animation events at both hop landings that call `MinionFootstepEmitter.ExecuteFootstep()` to play a footstep sound.

## Controls

| Input | Action |
|---|---|
| W / S or ↑ / ↓ | Move forward / back |
| A / D or ← / → | Turn |
| Q / E | Half-speed turn |
| 1–9, 0 | Limit forward speed from 10% up to 100% |
| Left Ctrl | Press the button when nearby |

## Tech
- Unity 6 (6000.0.82f1)
- C#
- Mecanim: blend trees, root motion, animation events, `MatchTarget`, IK

## Running it
1. Clone the repo and add the folder in Unity Hub.
2. Open `Assets/Scenes/demo.unity` and press Play.

The project also builds for macOS as a universal binary (Intel + Apple Silicon).

## Key scripts
- `Assets/Scripts/CharacterControl/RootMotionControlScript.cs`: root-motion controller, speed scalars, target matching and IK
- `Assets/Scripts/CharacterControl/BasicControlScript.cs`: non-root-motion controller
- `Assets/Scripts/CharacterControl/CharacterInputController.cs`: input filtering and speed limiting
- `Assets/Scripts/MinionSupport/MinionFootstepEmitter.cs`: footstep sound trigger
