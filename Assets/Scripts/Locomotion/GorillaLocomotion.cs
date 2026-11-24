using UnityEngine;
using UnityEngine.XR;

namespace GoreSandbox
{
    /// <summary>
    /// Gorilla Tag style arm-swing locomotion.
    /// Hold grip on a hand and swing it to pull your body. Let go to keep momentum.
    /// Falls back to keyboard/mouse when no XR device is present.
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
        public float slideSpeed = 13f;
        public float jumpVelocity = 9f;
        public float gravity = 36f;
        public float maxSwingSpeed = 24f;
        public float airControl = 0.5f;
        public LayerMask groundLayer = Physics.DefaultRaycastLayers;

        [Header("Input (keyboard fallback)")]
        public KeyCode leftGrabKey = KeyCode.LeftShift;
        public KeyCode rightGrabKey = KeyCode.RightShift;
        public KeyCode jumpKey = KeyCode.Space;

        CharacterController cc;
        Vector3 velocity;
        bool grounded;
        Vector3 leftLastPos, rightLastPos;

        void Awake()
        {
            cc = GetComponent<CharacterController>();
            if (!playerRig) playerRig = transform;
            if (!head) head = Camera.main ? Camera.main.transform : transform;
        }

        void Update()
        {
            bool lGrab = GetGrab(XRNode.LeftHand, leftGrabKey);
            bool rGrab = GetGrab(XRNode.RightHand, rightGrabKey);

            Vector3 lPos = leftHand ? leftHand.position : transform.position;
            Vector3 rPos = rightHand ? rightHand.position : transform.position;

            Vector3 swing = Vector3.zero;

            if (lGrab)
            {
                Vector3 d = leftLastPos - lPos;
                swing += ProjectOnPlane(d);
            }
            if (rGrab)
            {
                Vector3 d = rightLastPos - rPos;
                swing += ProjectOnPlane(d);
            }

            if (lGrab || rGrab)
            {
                Vector3 move = swing * slideSpeed * Time.deltaTime;
                if (move.magnitude > maxSwingSpeed * Time.deltaTime)
                    move = move.normalized * maxSwingSpeed * Time.deltaTime;
                cc.Move(move);
                velocity = move / Time.deltaTime;
            }
            else
            {
                velocity *= Mathf.Pow(0.4f, Time.deltaTime * 10f);
                if (grounded) cc.Move(velocity * Time.deltaTime * airControl);
            }

            grounded = cc.isGrounded;

            if (grounded && GetJump())
                velocity.y = jumpVelocity;

            ApplyGravity();

            leftLastPos = lPos;
            rightLastPos = rPos;
        }

        bool GetGrab(XRNode node, KeyCode key)
        {
            if (Input.GetKey(key)) return true;
            var dev = InputDevices.GetDeviceAtXRNode(node);
            if (!dev.isValid) return false;
            dev.TryGetFeatureValue(CommonUsages.gripButton, out bool v);
            return v;
        }

        bool GetJump()
        {
            if (Input.GetKeyDown(jumpKey)) return true;
            foreach (var node in new[] { XRNode.LeftHand, XRNode.RightHand })
            {
                var dev = InputDevices.GetDeviceAtXRNode(node);
                if (dev.isValid && dev.TryGetFeatureValue(CommonUsages.primaryButton, out bool v) && v)
                    return true;
            }
            return false;
        }

        Vector3 ProjectOnPlane(Vector3 v)
        {
            Vector3 flat = new Vector3(v.x, 0f, v.z);
            if (head != null)
            {
                // swing relative to camera facing
                Vector3 f = head.forward; f.y = 0; f.Normalize();
                Vector3 r = head.right; r.y = 0; r.Normalize();
                return flat.x * r + flat.z * f;
            }
            return flat;
        }

        void ApplyGravity()
        {
            if (!grounded)
            {
                velocity.y -= gravity * Time.deltaTime;
                cc.Move(velocity * Time.deltaTime);
            }
            else if (velocity.y < 0f)
            {
                velocity.y = -2f;
            }
        }

        public Vector3 GetVelocity() => velocity;
    }
}
