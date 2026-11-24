using UnityEngine;
using UnityEngine.Events;

namespace GoreSandbox
{
    public enum DamageType { Blunt, Slash, Pierce }

    [System.Serializable]
    public class DamageEvent : UnityEvent<float, Vector3, DamageType, GameObject> { }

    public class Damageable : MonoBehaviour
    {
        [Header("Health")]
        public float health = 100f;
        public float maxHealth = 100f;
        public bool isNPC;
        public bool immortal;

        [Header("Dismemberment")]
        public Transform[] limbs; // child parts that can detach
        public float slashDismemberThreshold = 30f;
        public GameObject detachedPartPrefab; // optional replacement visual for detached limb

        [Header("Ragdoll")]
        public Animator animator;
        public Rigidbody[] ragdollBodies;
        public Collider[] ragdollColliders;

        [Header("Events")]
        public DamageEvent onDamage;
        public UnityEvent onDie;

        bool dead;
        float lastDamageTime;

        public float Health => health;
        public bool Dead => dead;

        protected virtual void Awake()
        {
            maxHealth = health;
            SetRagdoll(false);
        }

        public void TakeDamage(float amount, Vector3 hitPoint, Vector3 normal, DamageType type, GameObject instigator)
        {
            if (dead || immortal) return;

            health -= amount;
            lastDamageTime = Time.time;
            onDamage?.Invoke(amount, hitPoint, type, instigator);

            // gore
            HitEffects.SpawnBlood(hitPoint, normal, amount, transform);

            // dismember on heavy slash
            if (type == DamageType.Slash && amount >= slashDismemberThreshold && limbs.Length > 0)
                TryDismember(hitPoint);

            if (health <= 0f) Die(instigator);
        }

        void TryDismember(Vector3 hitPoint)
        {
            float best = 999f; int idx = -1;
            for (int i = 0; i < limbs.Length; i++)
            {
                float d = Vector3.Distance(hitPoint, limbs[i].position);
                if (d < best) { best = d; idx = i; }
            }
            if (idx < 0) return;

            var limb = limbs[idx];
            DetachLimb(limb);
            // remove from array so it won't detach again
            var next = new Transform[limbs.Length - 1];
            int k = 0;
            for (int i = 0; i < limbs.Length; i++) if (i != idx) next[k++] = limbs[i];
            limbs = next;
        }

        void DetachLimb(Transform limb)
        {
            limb.SetParent(null);
            var rb = limb.GetComponent<Rigidbody>();
            if (rb == null) rb = limb.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.AddExplosionForce(400f, limb.position, 1f, 1f);

            var col = limb.GetComponent<Collider>();
            if (col == null) col = limb.gameObject.AddComponent<BoxCollider>();
            col.isTrigger = false;

            HitEffects.SpawnBlood(limb.position, Vector3.up, 40f, limb);
        }

        void Die(GameObject instigator)
        {
            if (dead) return;
            dead = true;
            onDie?.Invoke();

            if (animator != null) animator.enabled = false;
            SetRagdoll(true);

            // let the body fall over if it was using a CharacterController
            var cc = GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false;
                var rb = gameObject.AddComponent<Rigidbody>();
                rb.interpolation = RigidbodyInterpolation.Interpolate;
            }

            // spawn a big blood pool
            GoreManager.Instance?.SpawnPool(transform.position + Vector3.up * 0.5f, 6f);

            if (isNPC)
                Destroy(gameObject, 8f);
        }

        void SetRagdoll(bool ragdoll)
        {
            if (ragdollBodies != null)
                foreach (var rb in ragdollBodies) { if (rb != null) rb.isKinematic = !ragdoll; }
            if (ragdollColliders != null)
                foreach (var c in ragdollColliders) { if (c != null) c.enabled = ragdoll; }

            var mainCols = GetComponentsInChildren<Collider>();
            foreach (var c in mainCols) c.enabled = !ragdoll;
            var mainRb = GetComponent<Rigidbody>();
            if (mainRb != null) mainRb.isKinematic = ragdoll;
        }
    }
}
