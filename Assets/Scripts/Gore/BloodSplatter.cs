using UnityEngine;

namespace GoreSandbox
{
    [RequireComponent(typeof(ParticleSystem))]
    public class BloodSplatter : MonoBehaviour
    {
        public float lifetime = 2f;

        void Start() => Destroy(gameObject, lifetime);

        public void Configure(Vector3 normal, float amount)
        {
            var ps = GetComponent<ParticleSystem>();
            var main = ps.main;
            int count = Mathf.CeilToInt(amount / 6f);
            if (count < 1) count = 1;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 18f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.1f);
            main.startColor = new Color(0.5f, 0.04f, 0.04f, 1f);
            main.gravityModifier = 1.4f;
            var emit = ps.emission;
            emit.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
            transform.rotation = Quaternion.LookRotation(normal);
        }
    }
}
