using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Velocity reporter
// My moving waypoint is moved by a Mecanim animation, not by a Rigidbody, so it doesn't have
// a velocity I can just read. This script works it out myself every frame from how far it moved
// I put this on the MovingWaypoint sphere itself (the child that actually moves), not on its
// root, because the root never moves and would always report (0, 0, 0)
// MinionAI grabs this with GetComponent and uses .velocity for the intercept prediction
public class VelocityReporter : MonoBehaviour {

    // Where the waypoint was last frame, so I can compare it to where it is now
    private Vector3 prevPos;

    // Unfiltered velocity straight from (change in position / time)
    // I don't use this one for prediction because it can jump around from frame to frame
    public Vector3 rawVelocity
    {
        get;
        private set;
    }

    // Smoothed velocity, this is the one MinionAI reads (the TA FAQ says to use this and not
    // rawVelocity, otherwise the prediction can look jittery)
    public Vector3 velocity
    {
        get;
        private set;
    }

    // How much smoothing SmoothDamp does, bigger = smoother but slower to react
    public float smoothingTimeFactor = 0.5f;

    // SmoothDamp needs this to remember its own rate of change between frames
    private Vector3 smoothingParamVel;

    // Use this for initialization
    void Start () {

        // Start prevPos at the current spot so the first frame doesn't see a huge fake jump
        prevPos = this.transform.position;

    }


    // Update is called once per frame
    void Update () {

        // If time is paused (deltaTime is 0) dividing by it would blow up, so only calculate
        // the velocity when time is actually moving
        if(!Mathf.Approximately(Time.deltaTime,0f)){
            rawVelocity = (this.transform.position - prevPos) / Time.deltaTime;
            velocity = Vector3.SmoothDamp(velocity, rawVelocity, ref smoothingParamVel, smoothingTimeFactor);
        }
        else {
            rawVelocity = Vector3.zero;
            velocity = Vector3.zero;
        }

        // Remember this frame's position for next frame's calculation
        prevPos = this.transform.position;

    }
}
