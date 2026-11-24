using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR;
using System.IO;
using System.Collections.Generic;

namespace GoreSandbox.Editor
{
    /// <summary>
    /// One-click demo scene builder for Unity 2022.3.2f1 (Built-in renderer).
    /// Tools > Gore Sandbox > Create Scene
    ///
    /// Creates floor, walls, lights, player rig with Gorilla Tag locomotion,
    /// hands with trigger-based punching, GameManager, GoreManager, spawn menu,
    /// and all spawnable prefabs (NPC, brass knuckles, sword, brick, vase, plate, cup).
    /// </summary>
    public static class SceneBuilder
    {
        [MenuItem("Tools/Gore Sandbox/Create Scene")]
        public static void CreateScene()
        {
            // Cleanup previous demo objects
            foreach (var go in Object.FindObjectsOfType<GameObject>(true))
                if (go.name.StartsWith("[GS]")) Object.DestroyImmediate(go);

            // ---------- Materials (built-in shaders, no URP required) ----------
            var floorMat = new Material(Shader.Find("Standard")) { color = new Color(0.18f, 0.18f, 0.2f) };
            var wallMat = new Material(Shader.Find("Standard")) { color = new Color(0.22f, 0.22f, 0.24f) };

            // ---------- Floor ----------
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "[GS] Floor";
            floor.transform.localScale = new Vector3(20, 1, 20);
            floor.GetComponent<MeshRenderer>().material = floorMat;
            var floorRb = floor.AddComponent<Rigidbody>();
            floorRb.isKinematic = true;

            // ---------- Walls ----------
            MakeWall("[GS] WallBack", new Vector3(0, 3, -12), new Vector3(26, 6, 1), wallMat);
            MakeWall("[GS] WallLeft", new Vector3(-13, 3, 0), new Vector3(1, 6, 26), wallMat);
            MakeWall("[GS] WallRight", new Vector3(13, 3, 0), new Vector3(1, 6, 26), wallMat);

            // ---------- Light ----------
            var sun = new GameObject("[GS] Sun");
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.95f, 0.85f);

            // ---------- Player rig (VR) ----------
            var rig = new GameObject("[GS] PlayerRig");
            rig.transform.position = new Vector3(0, 0f, 0);
            var cc = rig.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.center = new Vector3(0, 0.9f, 0);
            cc.radius = 0.2f;
            var locomotion = rig.AddComponent<GorillaLocomotion>();

            // Camera offset - standard VR rig structure
            var camOffset = new GameObject("[GS] CameraOffset");
            camOffset.transform.SetParent(rig.transform, false);
            camOffset.transform.localPosition = new Vector3(0, 1.6f, 0);

            var camGO = new GameObject("[GS] Head");
            camGO.transform.SetParent(camOffset.transform, false);
            camGO.transform.localPosition = Vector3.zero;
            var camera = camGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            camGO.AddComponent<AudioListener>();
            camGO.AddComponent<XRHeadDriver>();

            var player = rig.AddComponent<Player>();
            player.head = camGO.transform;

            var lhand = MakeHand("[GS] LeftHand", camOffset.transform, new Vector3(-0.35f, -0.4f, 0.4f), XRNode.LeftHand);
            var rhand = MakeHand("[GS] RightHand", camOffset.transform, new Vector3(0.35f, -0.4f, 0.4f), XRNode.RightHand);

            locomotion.leftHand = lhand.transform;
            locomotion.rightHand = rhand.transform;
            locomotion.head = camGO.transform;
            locomotion.playerRig = rig.transform;
            player.leftHand = lhand.transform;
            player.rightHand = rhand.transform;

            // ---------- GameManager ----------
            var gmGO = new GameObject("[GS] GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.player = rig.transform;

            // ---------- GoreManager ----------
            new GameObject("[GS] GoreManager").AddComponent<GoreManager>();

            // ---------- Prefabs ----------
            var npcPrefab = MakeNPCPrefab();
            var knucklesPrefab = MakeKnucklesPrefab();
            var swordPrefab = MakeSwordPrefab();
            var brickPrefab = MakeBrickPrefab();
            var vasePrefab = MakeVasePrefab();
            var platePrefab = MakePlatePrefab();
            var cupPrefab = MakeCupPrefab();

            // Assign prefabs to GameManager via SerializedObject
            var so = new SerializedObject(gm);
            so.FindProperty("npcPrefab").objectReferenceValue = npcPrefab;
            so.FindProperty("brassKnucklesPrefab").objectReferenceValue = knucklesPrefab;
            so.FindProperty("swordPrefab").objectReferenceValue = swordPrefab;
            so.FindProperty("brickPrefab").objectReferenceValue = brickPrefab;
            so.FindProperty("vasePrefab").objectReferenceValue = vasePrefab;
            so.FindProperty("platePrefab").objectReferenceValue = platePrefab;
            so.FindProperty("cupPrefab").objectReferenceValue = cupPrefab;
            so.ApplyModifiedProperties();

            // ---------- Spawn menu canvas ----------
            var canvasGO = new GameObject("[GS] SpawnMenuCanvas");
            canvasGO.transform.SetParent(rig.transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasGO.GetComponent<RectTransform>();
            if (rect == null) rect = canvasGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700, 500);
            canvasGO.transform.localScale = Vector3.one * 0.002f;
            canvasGO.transform.localPosition = new Vector3(0, 1.5f, 1.2f);
            canvasGO.transform.localRotation = Quaternion.Euler(0, 180f, 0);
            canvasGO.AddComponent<GraphicRaycaster>();
            var sm = canvasGO.AddComponent<SpawnMenu>();
            sm.canvas = canvas;

            // Buttons
            string[] labels = { "Spawn NPC", "Brass Knuckles", "Sword", "Brick", "Vase", "Plate", "Cup", "Close" };
            float y = 160f;
            foreach (var label in labels)
            {
                var btnGO = new GameObject("[GS] " + label);
                btnGO.transform.SetParent(canvasGO.transform, false);
                var br = btnGO.AddComponent<RectTransform>();
                br.sizeDelta = new Vector2(600, 55);
                br.anchoredPosition = new Vector2(0, y);
                y -= 70f;
                btnGO.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 0.95f);
                var btn = btnGO.AddComponent<Button>();

                var txtGO = new GameObject("[GS] Label");
                txtGO.transform.SetParent(btnGO.transform, false);
                var txt = txtGO.AddComponent<Text>();
                txt.text = label;
                txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                txt.fontSize = 24;
                txt.alignment = TextAnchor.MiddleCenter;
                var tr = txtGO.AddComponent<RectTransform>();
                tr.sizeDelta = br.sizeDelta;
                tr.anchoredPosition = Vector2.zero;

                Wire(label, btn);
            }

            // ---------- Scatter breakable props around the map ----------
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(-5f, 0.5f, -2f));
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(5f, 0.5f, -2f));
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(-8f, 0.5f, 2f));
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(8f, 0.5f, 2f));
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(0f, 0.5f, 6f));
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(-4f, 0.5f, -8f));
            ScatterBreakable(npcPrefab, knucklesPrefab, swordPrefab, brickPrefab, vasePrefab, platePrefab, cupPrefab, new Vector3(4f, 0.5f, -8f));

            // ---------- Save scene & add to build settings ----------
            string scenePath = "Assets/Scenes/GoreSandbox.unity";
            Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), scenePath);

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (!scenes.Exists(s => s.path == scenePath))
                scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();

            Selection.activeObject = rig;
            Debug.Log("[O] Gore Sandbox scene created. Press Play.");
        }

        static void Wire(string label, Button btn)
        {
            switch (label)
            {
                case "Spawn NPC": btn.onClick.AddListener(() => GameManager.Instance?.SpawnNPC()); break;
                case "Brass Knuckles": btn.onClick.AddListener(() => GameManager.Instance?.SpawnItem("brass")); break;
                case "Sword": btn.onClick.AddListener(() => GameManager.Instance?.SpawnItem("sword")); break;
                case "Brick": btn.onClick.AddListener(() => GameManager.Instance?.SpawnItem("brick")); break;
                case "Vase": btn.onClick.AddListener(() => GameManager.Instance?.SpawnItem("vase")); break;
                case "Plate": btn.onClick.AddListener(() => GameManager.Instance?.SpawnItem("plate")); break;
                case "Cup": btn.onClick.AddListener(() => GameManager.Instance?.SpawnItem("cup")); break;
                case "Close":
                    btn.onClick.AddListener(() =>
                    {
                        var sm = Object.FindObjectOfType<SpawnMenu>();
                        if (sm != null) sm.Toggle();
                    });
                    break;
            }
        }

        static GameObject MakeWall(string name, Vector3 pos, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            go.GetComponent<MeshRenderer>().material = mat;
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            return go;
        }

        static GameObject MakeHand(string name, Transform parent, Vector3 localPos, XRNode node)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            // Visible hand mesh
            var vis = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            vis.name = name + "_Visual";
            vis.transform.SetParent(go.transform, false);
            vis.transform.localScale = Vector3.one * 0.12f;
            Object.DestroyImmediate(vis.GetComponent<Collider>());
            var mr = vis.GetComponent<MeshRenderer>();
            mr.sharedMaterial = new Material(Shader.Find("Standard")) { color = new Color(0.85f, 0.75f, 0.65f) };

            // Track real VR controller
            var driver = go.AddComponent<XRHandDriver>();
            driver.node = node;

            var col = go.AddComponent<BoxCollider>();
            col.size = Vector3.one * 0.18f;
            col.isTrigger = true; // trigger so PunchImpact.OnTriggerEnter fires
            var hand = go.AddComponent<VHand>();
            hand.node = node;
            go.AddComponent<PunchImpact>();
            return go;
        }

        // ---------- Prefab creators ----------

        static GameObject MakeNPCPrefab()
        {
            var go = new GameObject("NPC");
            go.transform.localScale = Vector3.one;
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = Vector3.up * 1f;
            body.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
            body.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.6f, 0.3f, 0.3f) };
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var cc = go.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.5f;
            cc.center = Vector3.up * 1f;

            var dmg = go.AddComponent<Damageable>();
            dmg.isNPC = true;
            dmg.health = 100f;

            go.AddComponent<NPC>();

            return SavePrefab(go, "NPC");
        }

        static GameObject MakeKnucklesPrefab()
        {
            var go = new GameObject("BrassKnuckles");
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.SetParent(go.transform, false);
            body.transform.localScale = new Vector3(0.15f, 0.15f, 0.3f);
            body.transform.localPosition = Vector3.forward * 0.2f;
            body.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.7f, 0.6f) };
            Object.DestroyImmediate(body.GetComponent<Collider>());

            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(0.15f, 0.15f, 0.3f);
            col.center = Vector3.forward * 0.2f;

            go.AddComponent<VGrabbable>();
            var w = go.AddComponent<Weapon>();
            w.damageType = DamageType.Blunt;
            w.baseDamage = 8f;
            w.velocityDamageMultiplier = 3f;
            go.AddComponent<BrassKnuckles>();

            return SavePrefab(go, "BrassKnuckles");
        }

        static GameObject MakeSwordPrefab()
        {
            var go = new GameObject("Sword");
            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.SetParent(go.transform, false);
            blade.transform.localPosition = Vector3.forward * 0.5f;
            blade.transform.localScale = new Vector3(0.1f, 0.8f, 0.2f);
            blade.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.8f, 0.9f) };
            Object.DestroyImmediate(blade.GetComponent<Collider>());

            var handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            handle.transform.SetParent(go.transform, false);
            handle.transform.localPosition = Vector3.back * 0.3f;
            handle.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            handle.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.3f, 0.2f, 0.2f) };
            Object.DestroyImmediate(handle.GetComponent<Collider>());

            var col = go.AddComponent<BoxCollider>();
            col.size = new Vector3(0.2f, 1f, 0.3f);
            col.center = Vector3.forward * 0.2f;

            go.AddComponent<VGrabbable>();
            var w = go.AddComponent<Weapon>();
            w.damageType = DamageType.Slash;
            w.baseDamage = 35f;
            w.velocityDamageMultiplier = 4f;
            w.knockback = 10f;
            go.AddComponent<Sword>();

            return SavePrefab(go, "Sword");
        }

        static GameObject MakeBrickPrefab()
        {
            var go = new GameObject("Brick");
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.4f, 0.2f, 0.2f);
            body.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.7f, 0.5f, 0.4f) };
            go.AddComponent<VGrabbable>();
            var br = go.AddComponent<Breakable>();
            br.breakVelocity = 5f;
            br.shardCount = 10;
            return SavePrefab(go, "Brick");
        }

        static GameObject MakeVasePrefab()
        {
            var go = new GameObject("Vase");
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
            body.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.2f, 0.4f, 0.8f) };
            go.AddComponent<VGrabbable>();
            var br = go.AddComponent<Breakable>();
            br.breakVelocity = 4f;
            br.shardCount = 12;
            return SavePrefab(go, "Vase");
        }

        static GameObject MakePlatePrefab()
        {
            var go = new GameObject("Plate");
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.transform.localScale = new Vector3(0.6f, 0.05f, 0.6f);
            body.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.9f, 0.9f, 0.9f) };
            go.AddComponent<VGrabbable>();
            var br = go.AddComponent<Breakable>();
            br.breakVelocity = 4f;
            br.shardCount = 10;
            return SavePrefab(go, "Plate");
        }

        static GameObject MakeCupPrefab()
        {
            var go = new GameObject("Cup");
            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.localScale = new Vector3(0.2f, 0.4f, 0.2f);
            body.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard")) { color = new Color(0.8f, 0.2f, 0.2f) };
            go.AddComponent<VGrabbable>();
            var br = go.AddComponent<Breakable>();
            br.breakVelocity = 4f;
            br.shardCount = 8;
            return SavePrefab(go, "Cup");
        }

        static GameObject SavePrefab(GameObject go, string name)
        {
            Directory.CreateDirectory("Assets/Prefabs");
            string path = "Assets/Prefabs/" + name + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        static void ScatterBreakable(GameObject npc, GameObject knuckles, GameObject sword, GameObject brick, GameObject vase, GameObject plate, GameObject cup, Vector3 pos)
        {
            System.Random rnd = new System.Random(pos.GetHashCode());
            GameObject[] items = { brick, vase, plate, cup };
            int count = 3 + rnd.Next(3);
            for (int i = 0; i < count; i++)
            {
                var prefab = items[rnd.Next(items.Length)];
                var go = Object.Instantiate(prefab, pos + new Vector3(rnd.Next(-30, 31) * 0.1f, 0f, rnd.Next(-30, 31) * 0.1f), Quaternion.Euler(0, rnd.Next(360), 0));
                go.name = "[GS] Prop";
                go.transform.position = new Vector3(go.transform.position.x, 0.5f, go.transform.position.z);
            }
        }
    }
}
