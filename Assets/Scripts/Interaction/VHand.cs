using UnityEngine;

namespace GoreSandbox
{
    /// <summary>
    /// Represents one VR hand. Grabs nearby VGrabbable objects, throws them on release.
    /// Keyboard fallback: E to grab/release the nearest grabbable.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class VHand : MonoBehaviour
    {
        public XRNode node;
        public KeyCode grabKey = KeyCode.E;

        public VGrabbable held { get; private set; }
        public bool isGrabbing => held != null;

        FixedJoint joint;
        Collider[] overlap = new Collider[8];

        void Update()
        {
            bool pressed = Input.GetKey(grabKey);
            var dev = InputDevices.GetDeviceAtXRNode(node);
            if (dev.isValid) dev.TryGetFeatureValue(CommonUsages.gripButton, out bool grip);
            else grip = pressed;

            if (grip && held == null)
                TryGrab();
            else if (!grip && held != null)
                Release();
        }

        void TryGrab()
        {
            int n = Physics.OverlapBoxNonAlloc(transform.position, Vector3.one * 0.15f, overlap, transform.rotation);
            VGrabbable best = null;
            float bestDist = 999f;
            for (int i = 0; i < n; i++)
            {
                var g = overlap[i].GetComponentInParent<VGrabbable>();
                if (g == null || g.isHeld) continue;
                float d = Vector3.Distance(transform.position, g.transform.position);
                if (d < bestDist) { bestDist = d; best = g; }
            }
            if (best != null) Grab(best);
        }

        void Grab(VGrabbable g)
        {
            held = g;
            g.OnGrab(this);

            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = g.body;
            joint.massScale = 1f;
            joint.connectedMassScale = 1f;
            joint.breakForce = 99999f;
            joint.breakTorque = 99999f;
        }

        void Release()
        {
            if (held != null)
            {
                held.OnRelease(GetVelocity());
                held = null;
            }
            if (joint != null)
            {
                Destroy(joint);
                joint = null;
            }
        }

        public Vector3 GetVelocity()
        {
            var dev = InputDevices.GetDeviceAtXRNode(node);
            if (dev.isValid && dev.TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 v))
                return v;
            return Vector3.zero;
        }
    }
}
