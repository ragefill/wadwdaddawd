using UnityEngine;

namespace GoreSandbox
{
    public class VGrabbable : MonoBehaviour
    {
        public bool isHeld { get; private set; }
        public Rigidbody body { get; private set; }
        public VHand holder { get; private set; }

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            if (body == null) body = gameObject.AddComponent<Rigidbody>();
        }

        public void OnGrab(VHand hand)
        {
            isHeld = true;
            holder = hand;
            body.isKinematic = true;
            body.useGravity = false;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            transform.SetParent(hand.transform, true);
        }

        public void OnRelease(Vector3 throwVelocity)
        {
            isHeld = false;
            holder = null;
            transform.SetParent(null);
            body.isKinematic = false;
            body.useGravity = true;
            body.velocity = throwVelocity;
            body.angularVelocity = Vector3.zero;
        }
    }
}
