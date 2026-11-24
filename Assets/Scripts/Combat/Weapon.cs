using UnityEngine;

namespace GoreSandbox
{
    /// <summary>
    /// Deals damage on high-velocity contact. Attach to swords, knuckles, bricks, etc.
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        public DamageType damageType = DamageType.Blunt;
        public float baseDamage = 20f;
        public float minVelocity = 3f;
        public float velocityDamageMultiplier = 4f;
        public float knockback = 6f;

        bool active => true;

        void OnCollisionEnter(Collision col)
        {
            Apply(col, col.relativeVelocity.magnitude);
        }

        void OnCollisionStay(Collision col)
        {
            Apply(col, col.relativeVelocity.magnitude);
        }

        void Apply(Collision col, float vel)
        {
            if (vel < minVelocity) return;
            var dmg = col.gameObject.GetComponentInParent<Damageable>();
            if (dmg == null) return;

            float damage = baseDamage + vel * velocityDamageMultiplier;
            Vector3 point = col.contacts[0].point;
            Vector3 normal = col.contacts[0].normal;

            dmg.TakeDamage(damage, point, normal, damageType, gameObject);

            var rb = col.rigidbody;
            if (rb != null && !rb.isKinematic)
                rb.AddForceAtPosition(col.relativeVelocity.normalized * knockback * damage * 0.01f, point, ForceMode.Impulse);
        }
    }
}
