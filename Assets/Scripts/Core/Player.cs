using UnityEngine;

namespace GoreSandbox
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        public float health = 100f;
        public bool dead;

        public Transform head;
        public Transform leftHand;
        public Transform rightHand;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void TakeDamage(float amount)
        {
            if (dead) return;
            health -= amount;
            if (health <= 0f) Die();
        }

        void Die()
        {
            dead = true;
            Debug.Log("[O] Player died.");
        }
    }
}
