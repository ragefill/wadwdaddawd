using UnityEngine;
using System.Collections.Generic;

namespace GoreSandbox
{
    public class GoreManager : MonoBehaviour
    {
        public static GoreManager Instance { get; private set; }

        [Header("Limits")]
        public int maxBloodParticles = 2000;
        public int maxDecals = 60;
        public float decalSize = 0.35f;

        static readonly List<GameObject> decals = new();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SpawnPool(Vector3 point, float radius)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = point;
            go.transform.localScale = new Vector3(radius, 0.02f, radius);
            var mr = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                color = new Color(0.35f, 0.03f, 0.03f, 0.9f)
            };
            mr.material = mat;
            decals.Add(go);
            Object.Destroy(go, Random.Range(8f, 16f));
            Trim();
        }

        public void AddDecal(Vector3 point, Vector3 normal)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Object.Destroy(go.GetComponent<Collider>());
            go.transform.position = point + normal * 0.01f;
            go.transform.rotation = Quaternion.LookRotation(normal);
            go.transform.localScale = Vector3.one * decalSize;
            var mr = go.GetComponent<MeshRenderer>();
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")) {
                color = new Color(0.5f, 0.04f, 0.04f, 1f)
            };
            mr.material = mat;
            decals.Add(go);
            Object.Destroy(go, Random.Range(6f, 12f));
            Trim();
        }

        void Trim()
        {
            decals.RemoveAll(d => d == null);
            while (decals.Count > maxDecals)
            {
                var old = decals[0];
                decals.RemoveAt(0);
                if (old != null) Destroy(old);
            }
        }
    }
}
