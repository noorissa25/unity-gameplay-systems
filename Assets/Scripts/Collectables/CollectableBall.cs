using UnityEngine;

// Trigger-based collectable ball
// The pink sphere's SphereCollider has IsTrigger checked, so it doesn't physically
// collide with anything but still gets OnTriggerEnter() callbacks
// Only characters with a BallCollector component can pick it up
public class CollectableBall : MonoBehaviour
{
    void OnTriggerEnter(Collider c)
    {
        // Making sure whatever entered the trigger has a rigidbody
        if (c.attachedRigidbody != null)
        {
            // Then checking if the rigidbody's GameObject is a BallCollector (SomeDude_RootMotion)
            BallCollector bc = c.attachedRigidbody.gameObject.GetComponent<BallCollector>();

            // Only a BallCollector can collect the ball, everything else is ignored
            if (bc != null)
            {
                // Telling the collector it now has the ball
                bc.ReceiveBall();

                // Play a sound through the event system (BombBounceEvent is just a placeholder sound)
                EventManager.TriggerEvent<BombBounceEvent, Vector3>(c.transform.position);

                // Remove the collected ball from the scene
                Destroy(this.gameObject);
            }
        }
    }
}
