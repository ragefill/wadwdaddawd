using UnityEngine;
using UnityEngine.XR;

namespace GoreSandbox
{
    /// <summary>
    /// Gorilla Tag locomotion reworked: no "hold grip then release" toggle.
    ///
    /// Your hands drive movement directly:
    ///   * Swing your arms -> your body accelerates in that direction (arm-swim).
    ///   * Collide your hand with a wall/floor/object -> you push off it (launch away
    ///     from the surface normal with a little upward bounce).
    ///   * Momentum carries you; friction slows you on the ground and in the air.
    ///
    /// Falls back to WASD + Space when no XR headset is present.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class GorillaLocomotion : MonoBehaviour
    {
        [Header("References")]
        public Transform leftHand;
        public Transform rightHand;
        public Transform head;
        public Transform playerRig;

        [Header("Movement")]
        public float swingAcceleration = 45f;   // how strongly hand motion moves your body
        public float maxSpeed = 24f;
        public float groundFriction = 10f;
        public float airFriction = 3f;
        public float airControl = 0.5f;
        public float jumpVelocity = 10f;
        public float gravity = 38f;

        [Header("Hand push-off (collide with hands to move)")]
        public float pushOffForce = 18f;
        public float pushOffRadius = 0.14f;
        public float pushOffCooldown = 0.12f;
        public LayerMask pushableLayers = Physics.DefaultRaycastLayers;

        [Header("Keyboard fallback (no VR)")]
        public KeyCode jumpKey = KeyCode.Space;

        CharacterController cc;
        Vector3 velocity;
        Vector3 leftLastPos, rightLastPos;
        float leftPushCD, rightPushCD;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (!playerRig) playerRig = transform;
            if (!head) head = Camera.main ? Camera.main.transform : transform;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            Vector3 lPos = GetHandPosition(XRNode.LeftHand);
            Vector3 rPos = GetHandPosition(XRNode.RightHand);

            // ---- Arm-swing propulsion: moving your hands moves your body ----
            Vector3 lDelta = lPos - leftLastPos;
            Vector3 rDelta = rPos - rightLastPos;

            Vector3 wish = Flatten(lDelta + rDelta) * swingAcceleration;

            float friction = cc.isGrounded ? groundFriction : airFriction * airControl;
            velocity = Vector3.MoveTowards(velocity, wish, friction * dt);

            // Cap horizontal speed
            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            if (flat.magnitude > maxSpeed)
                velocity = new Vector3(flat.normalized * maxSpeed, velocity.y);

            // ---- Push-off: collide your hand with the world to launch ----
            TryPushOff(leftHand, ref leftPushCD);
            TryPushOff(rightHand, ref rightPushCD);

            // ---- Gravity / jump ----
            if (cc.isGrounded)
            {
                velocity.y = -2f;
                if (Input.GetKeyDown(jumpKey)) velocity.y = jumpVelocity;

                // XR primary button (A / X) also jumps
                foreach (var node in new[] { XRNode.LeftHand, XRNode.RightHand })
                {
                    var dev = InputDevices.GetDeviceAtXRNode(node);
                    if (dev.isValid && dev.TryGetFeatureValue(CommonUsages.primaryButton, out bool b) && b)
                        velocity.y = jumpVelocity;
                }
            }
            else
            {
                velocity.y -= gravity * dt;
            }

            cc.Move(velocity * dt);

            // ---- Keyboard WASD fallback ----
            if (!HasXRDevice())
            {
                Vector3 kb = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
                if (kb.sqrMagnitude > 0.0001f)
                {
                    kb = Flatten(kb).normalized * swingAcceleration * dt;
                    cc.Move(kb);
                }
            }

            leftLastPos = lPos;
            rightLastPos = rPos;
        }

        Vector3 Flatten(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        Vector3 GetHandPosition(XRNode node)
        {
            var dev = InputDevices.GetDeviceAtXRNode(node);
            if (dev.isValid && dev.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 p))
                return p;

            var t = node == XRNode.LeftHand ? leftHand : rightHand;
            return t != null ? t.position : transform.position;
        }

        void TryPushOff(Transform hand, ref float cd)
        {
            if (hand == null) return;
            cd -= Time.deltaTime;
            if (cd > 0f) return;

            // Did our hand just collide with something?
            Collider[] hits = Physics.OverlapSphere(hand.position, pushOffRadius, pushableLayers, QueryTriggerInteraction.Ignore);
            if (hits.Length == 0) return;

            Vector3 push = Vector3.zero;
            foreach (var c in hits)
            {
                Vector3 closest = c.ClosestPoint(hand.position);
                Vector3 dir = (hand.position - closest);
                if (dir.sqrMagnitude > 0.0001f)
                    push += dir.normalized;
            }

            if (push.sqrMagnitude > 0.0001f)
            {
                push.Normalize();
                push.y = Mathf.Max(push.y, 0.3f); // always a little upward hop
                velocity += push * pushOffForce;
                cd = pushOffCooldown;
            }
        }

        bool HasXRDevice()
        {
            return InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).isValid
                || InputDevices.GetDeviceAtXRNode(XRNode.RightHand).isValid;
        }

        public Vector3 GetVelocity() => velocity;
    }
}
