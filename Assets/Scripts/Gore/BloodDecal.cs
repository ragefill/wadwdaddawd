using UnityEngine;
using System.Collections;

namespace GoreSandbox
{
    public class BloodDecal : MonoBehaviour
    {
        public float fadeTime = 6f;

        public void Init(Vector3 point, Vector3 normal, float size)
        {
            transform.position = point + normal * 0.01f;
            transform.rotation = Quaternion.LookRotation(normal);
            transform.localScale = Vector3.one * size;

            var mr = GetComponent<MeshRenderer>();
            if (mr != null)
            {
                var mat = new Material(Shader.Find("Legacy Shaders/Transparent/Cutout/Diffuse")) {
                    color = new Color(0.55f, 0.05f, 0.05f, 1f)
                };
                mr.material = mat;
            }

            StartCoroutine(Fade());
        }

        System.Collections.IEnumerator Fade()
        {
            var mr = GetComponent<MeshRenderer>();
            float t = 0f;
            while (mr != null && t < fadeTime)
            {
                t += Time.deltaTime;
                var c = mr.material.color;
                c.a = Mathf.Lerp(1f, 0f, t / fadeTime);
                mr.material.color = c;
                yield return null;
            }
            Destroy(gameObject);
        }
    }
}
