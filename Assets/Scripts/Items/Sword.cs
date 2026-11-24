using UnityEngine;

namespace GoreSandbox
{
    public class Sword : MonoBehaviour
    {
        [Header("Slash")]
        public float slashDamage = 35f;
        public float minVelocity = 4f;
        public float dismemberChance = 0.15f;
        public float knockback = 8f;

        void OnCollisionEnter(Collision col)
        {
            if (col.relativeVelocity.magnitude < minVelocity) return;
            var dmg = col.gameObject.GetComponentInParent<Damageable>();
            if (dmg == null) return;

            float damage = slashDamage + col.relativeVelocity.magnitude * 2f;
            dmg.TakeDamage(damage, col.contacts[0].point, col.contacts[0].normal, DamageType.Slash, gameObject);

            if (Random.value < dismemberChance && col.relativeVelocity.magnitude > 8f)
            {
                // try to dismember nearest limb
                var limbs = dmg.GetComponentsInChildren<Transform>();
                Transform nearest = null; float best = 999f;
                foreach (var l in limbs)
                {
                    if (l == dmg.transform) continue;
                    float d = Vector3.Distance(col.contacts[0].point, l.position);
                    if (d < best) { best = d; nearest = l; }
                }
                if (nearest != null)
                {
                    nearest.SetParent(null);
                    var limbRb = nearest.GetComponent<Rigidbody>();
                    if (limbRb == null) limbRb = nearest.gameObject.AddComponent<Rigidbody>();
                    limbRb.isKinematic = false;
                    limbRb.useGravity = true;
                    limbRb.AddExplosionForce(500f, nearest.position, 1f, 2f);
                    HitEffects.SpawnBlood(nearest.position, Vector3.up, 60f, nearest);
                }
            }

            var colRb = col.rigidbody;
            if (colRb != null && !colRb.isKinematic)
                colRb.AddForceAtPosition(col.relativeVelocity.normalized * knockback, col.contacts[0].point, ForceMode.Impulse);
        }
    }
}
