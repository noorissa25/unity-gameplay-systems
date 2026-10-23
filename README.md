# Unity Gameplay Systems

A third-person character animation and control sandbox built in Unity. It covers root-motion and code-driven locomotion, blend trees, animation events, target matching and inverse kinematics.

The project started from a provided character-control starter scene. The systems below are the ones I implemented or extended on top of it.

## Features

### Root-motion locomotion with walk/run blending
- Extended the forward-movement blend tree with run, run-left and run-right motions, replacing a placeholder sped-up walk clip.
- Tuned blend-tree positions so the character walks at low input, runs at full input in any direction (including while turning), and blends smoothly in between.

### Tunable root-motion speed
- Added inspector-exposed `animationSpeed`, `rootMovementSpeed` and `rootTurnSpeed` scalars to the root-motion controller.
- Root-motion position and rotation deltas are scaled in `OnAnimatorMove()` with `Vector3.LerpUnclamped` / `Quaternion.LerpUnclamped`, so movement can be made faster and more responsive without breaking the animation.

### Code-driven turning (non-root-motion character)
- The non-root-motion character turns with `Rigidbody.MoveRotation` while the walk-forward animation keeps playing, instead of switching to a separate turn animation.

### Target matching and IK for interactions
- When the character is close to (but not exactly at) a button, it transitions through a `MatchToButtonPress` state and uses `Animator.MatchTarget()` to land precisely on the standing spot before pressing.
- Added `OnAnimatorIK()` so that during the press the character looks at and reaches toward the button with right-hand IK. The IK and look-at weights are driven by an animation curve, so the pose blends in and out smoothly.

### Procedural hop animation and footstep events
- Authored hop-and-twist variants of a creature's forward and turning walk cycles by keyframing the model's child transform, leaving the root-motion curves on the parent untouched.
- Added animation events on the hop landings that trigger a footstep sound emitter.

## Tech
- Unity (see `ProjectSettings/ProjectVersion.txt` for the exact version)
- C#
- Mecanim: blend trees, root motion, animation events, `MatchTarget`, IK

## Running it
1. Clone the repo and add the folder in Unity Hub.
2. Open `Assets/Scenes/demo.unity` and press Play.
