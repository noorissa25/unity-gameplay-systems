using UnityEngine;

namespace GameplaySystems.PhysicsSim
{
    // Autonomous "click beetle" / jumping bean. Not player controlled. While grounded,
    // applies a random impulse (and a little random torque) at random intervals to
    // produce an erratic hopping motion.
    [RequireComponent(typeof(Rigidbody))]
    public class JumpingBean : MonoBehaviour
    {
        [Header("Timing")]
        [SerializeField] private float minSecondsBetweenJumps = 0.5f;
        [SerializeField] private float maxSecondsBetweenJumps = 2.5f;

        [Header("Jump Impulse")]
        [SerializeField] private float minJumpForce = 2f;
        [SerializeField] private float maxJumpForce = 5f;
        [SerializeField] private float maxHorizontalAngleFromUp = 35f;
        [SerializeField] private float maxTorque = 3f;

        [Header("Ground Check")]
        [SerializeField] private float groundCheckDistance = 0.6f;
        [SerializeField] private float groundCheckOriginOffset = 0.1f;

        private Rigidbody rb;
        private float nextJumpTime;

        void Awake()
        {
            rb = GetComponent<Rigidbody>();
            ScheduleNextJump();
        }

        void FixedUpdate()
        {
            if (Time.time < nextJumpTime)
                return;

            if (!IsGrounded())
                return;

            DoJump();
            ScheduleNextJump();
        }

        private bool IsGrounded()
        {
            Vector3 origin = transform.position + Vector3.up * groundCheckOriginOffset;

            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckOriginOffset + groundCheckDistance))
            {
                return hit.collider.CompareTag("ground");
            }

            return false;
        }

        private void DoJump()
        {
            // Random direction: mostly upward, with some random lean away from vertical.
            float angle = Random.Range(0f, maxHorizontalAngleFromUp) * Mathf.Deg2Rad;
            float heading = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(angle) * Mathf.Cos(heading), Mathf.Cos(angle), Mathf.Sin(angle) * Mathf.Sin(heading));

            float force = Random.Range(minJumpForce, maxJumpForce);

            rb.AddForce(dir * force, ForceMode.Impulse);

            Vector3 randomTorque = Random.insideUnitSphere * maxTorque;
            rb.AddTorque(randomTorque, ForceMode.Impulse);
        }

        private void ScheduleNextJump()
        {
            nextJumpTime = Time.time + Random.Range(minSecondsBetweenJumps, maxSecondsBetweenJumps);
        }
    }
}
