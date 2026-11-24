using UnityEngine;

namespace GoreSandbox
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        public Transform player;
        public Transform spawnPoint;

        [Header("Spawn")]
        public GameObject npcPrefab;
        public GameObject brassKnucklesPrefab;
        public GameObject swordPrefab;
        public GameObject brickPrefab;
        public GameObject vasePrefab;
        public GameObject platePrefab;
        public GameObject cupPrefab;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SpawnNPC() => Spawner.Spawn(npcPrefab, GetSpawnPosition(), Quaternion.identity);

        public void SpawnItem(string type)
        {
            GameObject prefab = type switch
            {
                "brass" => brassKnucklesPrefab,
                "sword" => swordPrefab,
                "brick" => brickPrefab,
                "vase" => vasePrefab,
                "plate" => platePrefab,
                "cup" => cupPrefab,
                _ => brickPrefab
            };
            Spawner.Spawn(prefab, GetSpawnPosition(), Quaternion.identity);
        }

        Vector3 GetSpawnPosition()
        {
            if (player != null)
                return player.position + player.forward * 2f + Vector3.up * 1f;
            return spawnPoint != null ? spawnPoint.position : Vector3.zero;
        }
    }
}
