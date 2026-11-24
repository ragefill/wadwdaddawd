using UnityEngine;

namespace GoreSandbox
{
    public class NPC : Damageable
    {
        [Header("AI")]
        public float moveSpeed = 4f;
        public float chaseRange = 12f;
        public float attackRange = 1.6f;
        public float attackCooldown = 1.2f;
        public float attackDamage = 25f;

        CharacterController cc;
        Transform target;
        float lastAttack;
        Vector3 wanderDir;
        float wanderTimer;

        protected override void Awake()
        {
            base.Awake();
            cc = GetComponent<CharacterController>();
            if (cc == null) cc = gameObject.AddComponent<CharacterController>();
            isNPC = true;
            health = Random.Range(80f, 140f);
            maxHealth = health;
        }

        void Update()
        {
            if (dead) return;

            target = Player.Instance != null ? Player.Instance.transform : null;

            if (target != null)
            {
                float d = Vector3.Distance(transform.position, target.position);

                if (d < attackRange && Time.time - lastAttack > attackCooldown)
                {
                    lastAttack = Time.time;
                    Attack(target);
                }
                else if (d < chaseRange)
                {
                    Chase(target.position);
                }
                else
                {
                    Wander();
                }
            }
            else
            {
                Wander();
            }
        }

        void Chase(Vector3 pos)
        {
            Vector3 dir = (pos - transform.position).normalized;
            dir.y = 0;
            cc.Move(dir * moveSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);
        }

        void Wander()
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0f)
            {
                wanderDir = Random.insideUnitSphere * 5f;
                wanderDir.y = 0;
                wanderTimer = Random.Range(2f, 6f);
            }
            Vector3 next = transform.position + wanderDir.normalized * moveSpeed * 0.5f * Time.deltaTime;
            cc.Move(next - transform.position);
            if (wanderDir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(wanderDir), Time.deltaTime * 3f);
        }

        void Attack(Transform victim)
        {
            var dmg = victim.GetComponent<Damageable>();
            if (dmg != null)
                dmg.TakeDamage(attackDamage, victim.position, (victim.position - transform.position).normalized, DamageType.Blunt, gameObject);
        }
    }
}
