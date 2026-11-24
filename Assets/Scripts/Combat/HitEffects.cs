using UnityEngine;

namespace GoreSandbox
{
    public static class HitEffects
    {
        public static void SpawnBlood(Vector3 point, Vector3 normal, float amount, Transform parent = null)
        {
            int count = Mathf.CeilToInt(amount / 8f);
            if (count < 1) count = 1;

            GameObject go = new GameObject("BloodSplatter");
            if (parent != null) go.transform.SetParent(parent, true);
            go.transform.position = point;
            go.transform.rotation = Quaternion.LookRotation(normal);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.2f;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.08f);
            main.startColor = new Color(0.55f, 0.05f, 0.05f, 1f);
            main.gravityModifier = 1.2f;
            main.maxParticles = Mathf.Max(50, count * 4);

            var emit = ps.emission;
            emit.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });

            var shape = ps.shape;
            shape.radius = 0.05f;

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.space = ParticleSystemSimulationSpace.World;

            Object.Destroy(go, 3f);
        }

        public static void SpawnDecal(Vector3 point, Vector3 normal, Texture2D tex, float size = 0.3f, Transform parent = null)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = point + normal * 0.005f;
            go.transform.rotation = Quaternion.LookRotation(normal);
            go.transform.localScale = Vector3.one * size;
            if (parent != null) go.transform.SetParent(parent, true);

            var mr = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse")) {
                mainTexture = tex,
                color = new Color(0.6f, 0.06f, 0.06f, 1f)
            };
            mr.material = mat;

            Object.Destroy(go, Random.Range(4f, 10f));
        }
    }
}
