using UnityEngine;

namespace GoreSandbox
{
    /// <summary>
    /// Put on each hand. Deals punch damage based on hand velocity.
    /// BrassKnuckles on the same hand multiplies damage.
    /// </summary>
    public class PunchImpact : MonoBehaviour
    {
        public float baseDamage = 12f;
        public float velocityMultiplier = 2.5f;
        public float minVelocity = 2.5f;

        VHand hand;

        void Awake() => hand = GetComponent<VHand>();

        void OnCollisionEnter(Collision col)
        {
            if (hand == null || !hand.isGrabbing) return; // only punch with an open/swinging hand
            float vel = hand.GetVelocity().magnitude;
            if (vel < minVelocity) return;

            var dmg = col.gameObject.GetComponentInParent<Damageable>();
            if (dmg == null) return;

            float knuckleMult = 1f;
            var knuckles = GetComponentInChildren<BrassKnuckles>();
            if (knuckles != null) knuckleMult = knuckles.multiplier;

            float damage = (baseDamage + vel * velocityMultiplier) * knuckleMult;
            dmg.TakeDamage(damage, col.contacts[0].point, col.contacts[0].normal, DamageType.Blunt, gameObject);

            var rb = col.rigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddForceAtPosition(col.relativeVelocity.normalized * damage * 0.15f, col.contacts[0].point, ForceMode.Impulse);
        }
    }
}
