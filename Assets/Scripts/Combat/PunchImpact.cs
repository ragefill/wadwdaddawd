using UnityEngine;

namespace GoreSandbox
{
    /// <summary>
    /// Put on each hand. Deals punch damage based on hand velocity.
    /// BrassKnuckles on the same hand multiplies damage.
    /// Uses OnTriggerEnter because the hand collider is a trigger.
    /// </summary>
    public class PunchImpact : MonoBehaviour
    {
        public float baseDamage = 12f;
        public float velocityMultiplier = 2.5f;
        public float minVelocity = 2.5f;

        VHand hand;

        void Awake() => hand = GetComponent<VHand>();

        void OnTriggerEnter(Collider other)
        {
            if (hand == null || hand.isGrabbing) return; // only punch with an empty hand
            float vel = hand.GetVelocity().magnitude;
            if (vel < minVelocity) return;

            var dmg = other.GetComponentInParent<Damageable>();
            if (dmg == null) return;

            float knuckleMult = 1f;
            var knuckles = GetComponentInChildren<BrassKnuckles>();
            if (knuckles != null) knuckleMult = knuckles.multiplier;

            float damage = (baseDamage + vel * velocityMultiplier) * knuckleMult;

            Vector3 point = other.ClosestPoint(transform.position);
            Vector3 normal = (transform.position - point).normalized;
            if (normal.sqrMagnitude < 0.0001f) normal = transform.up;

            dmg.TakeDamage(damage, point, normal, DamageType.Blunt, gameObject);

            var rb = other.attachedRigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddForceAtPosition(normal * damage * 0.15f, point, ForceMode.Impulse);
        }
    }
}
