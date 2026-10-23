using UnityEngine;

// Goes on SomeDude_RootMotion so it can collect the pink CollectableBall
// CollectableBall looks for this component to decide who is allowed to pick it up
public class BallCollector : MonoBehaviour
{
    // Starts false and flips to true once the ball is collected (visible in the Inspector)
    public bool hasBall = false;

    // Called by CollectableBall when this character enters the ball's trigger
    public void ReceiveBall()
    {
        hasBall = true;
    }
}
