using UnityEngine;

// Trigger-based animated alien plant
// This goes on the root empty GameObject of my AlienPlant prefab, along with the Animator
// and a SphereCollider set to Is Trigger (the "too close" radius)
// When a player walks into the trigger the plant ducks into the ground, and when they
// leave it pops back out
//
// To keep the animation smooth (no discontinuities) I never jump between animation states
// Instead my Animator Controller uses a 1D Blend Tree between two poses:
//   Retract = 0 -> PlantOut clip (fully out)
//   Retract = 1 -> PlantHidden clip (squashed into the ground)
// This script slowly moves the "Retract" float toward 0 or 1, so if the player leaves
// halfway through, the plant just reverses from wherever it currently is
[RequireComponent(typeof(Animator))]
public class PlantRetractTrigger : MonoBehaviour
{
    // How fast the plant hides / comes back out (1 = takes one second to go all the way)
    public float retractSpeed = 1.5f;

    private Animator anim;

    // Current blend value I'm sending to the Animator (0 = out, 1 = hidden)
    private float retractAmount = 0f;

    // Counting enters/exits so a character with more than one collider doesn't
    // accidentally make the plant pop back out while they're still inside
    private int playersInside = 0;

    void Awake()
    {
        anim = GetComponent<Animator>();

        if (anim == null)
            Debug.LogError("PlantRetractTrigger: No Animator found on " + gameObject.name);
    }

    void OnTriggerEnter(Collider c)
    {
        // Only react to player characters (they all have a CharacterInputController)
        if (IsPlayer(c))
            playersInside++;
    }

    void OnTriggerExit(Collider c)
    {
        if (IsPlayer(c))
            playersInside = Mathf.Max(0, playersInside - 1);
    }

    void Update()
    {
        // Hide if a player is close, otherwise come back out
        float target = playersInside > 0 ? 1f : 0f;

        // Move a little bit toward the target every frame instead of snapping to it
        retractAmount = Mathf.MoveTowards(retractAmount, target, retractSpeed * Time.deltaTime);

        // Drive the Blend Tree parameter in my Animator Controller
        anim.SetFloat("Retract", retractAmount);
    }

    // Same idea as my CollectableBall check: needs to have a rigidbody first,
    // then need to look for the component that marks it as a player
    private bool IsPlayer(Collider c)
    {
        if (c.attachedRigidbody == null)
            return false;

        return c.attachedRigidbody.GetComponent<CharacterInputController>() != null;
    }
}
