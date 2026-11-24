using UnityEngine;

namespace GoreSandbox
{
    public static class Spawner
    {
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            GameObject go = Object.Instantiate(prefab, position, rotation);

            // ensure it has a rigidbody for physics sandbox
            if (go.GetComponent<Rigidbody>() == null)
                go.AddComponent<Rigidbody>();

            return go;
        }
    }
}
