using UnityEngine;
using System.Collections;

namespace GoreSandbox
{
    /// <summary>
    /// Bricks, vases, plates, cups - shatter into small pieces on hard impact.
    /// </summary>
    public class Breakable : MonoBehaviour
    {
        public float breakVelocity = 6f;
        public int shardCount = 14;
        public GameObject shardPrefab;

        bool broken;

        void OnCollisionEnter(Collision col)
        {
            if (broken || col.relativeVelocity.magnitude < breakVelocity) return;
            Break(col.contacts[0].point);
        }

        void Break(Vector3 point)
        {
            broken = true;
            HitEffects.SpawnBlood(point, Vector3.up, 20f, null); // dust-ish splash (no texture needed)
            GetComponent<Collider>().enabled = false;
            var rb = GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            for (int i = 0; i < shardCount; i++)
            {
                GameObject shard = shardPrefab != null ? Instantiate(shardPrefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.transform.position = point + Random.insideUnitSphere * 0.1f;
                shard.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);
                shard.transform.rotation = Random.rotation;
                var sr = shard.GetComponent<Rigidbody>();
                if (sr == null) sr = shard.AddComponent<Rigidbody>();
                sr.AddExplosionForce(Random.Range(3f, 9f), point, 0.5f, 1f);
                if (shardPrefab == null) Object.Destroy(shard.GetComponent<Collider>());
                Object.Destroy(shard, Random.Range(1f, 3f));
            }

            Object.Destroy(gameObject, 0.05f);
        }
    }
}
