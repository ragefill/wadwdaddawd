using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace GoreSandbox.Editor
{
    /// <summary>
    /// One-click demo scene builder. Tools > Gore Sandbox > Create Scene.
    /// Creates floor, walls, lights, player rig with Gorilla Tag locomotion,
    /// hands with punch impact, GameManager, GoreManager, and spawn menu.
    /// </summary>
    public static class SceneBuilder
    {
        [MenuItem("Tools/Gore Sandbox/Create Scene")]
        public static void CreateScene()
        {
            // Cleanup previous demo objects
            foreach (var go in Object.FindObjectsOfType<GameObject>(true))
                if (go.name.StartsWith("[GS]")) Object.DestroyImmediate(go);

            // Floor
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "[GS] Floor";
            floor.transform.localScale = new Vector3(20, 1, 20);
            var floorMat = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = new Color(0.18f, 0.18f, 0.2f) };
            floor.GetComponent<MeshRenderer>().material = floorMat;
            var floorRb = floor.AddComponent<Rigidbody>();
            floorRb.isKinematic = true;

            // Containment walls
            MakeWall("[GS] WallBack", new Vector3(0, 3, -12), new Vector3(26, 6, 1));
            MakeWall("[GS] WallLeft", new Vector3(-13, 3, 0), new Vector3(1, 6, 26));
            MakeWall("[GS] WallRight", new Vector3(13, 3, 0), new Vector3(1, 6, 26));

            // Light
            var sun = new GameObject("[GS] Sun");
            sun.transform.rotation = Quaternion.Euler(50, -30, 0);
            var light = sun.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color(1f, 0.95f, 0.85f);

            // Player rig
            var rig = new GameObject("[GS] PlayerRig");
            rig.transform.position = new Vector3(0, 1.2f, 0);
            var locomotion = rig.AddComponent<GorillaLocomotion>();

            var camGO = new GameObject("[GS] Head");
            camGO.transform.SetParent(rig.transform, false);
            camGO.transform.localPosition = new Vector3(0, 1.1f, 0);
            var camera = camGO.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.12f);
            var player = rig.AddComponent<Player>();
            player.head = camGO.transform;

            var lhand = MakeHand("[GS] LeftHand", rig.transform, new Vector3(-0.35f, 0.6f, 0.4f), XRNode.LeftHand);
            var rhand = MakeHand("[GS] RightHand", rig.transform, new Vector3(0.35f, 0.6f, 0.4f), XRNode.RightHand);
            locomotion.leftHand = lhand.transform;
            locomotion.rightHand = rhand.transform;
            locomotion.head = camGO.transform;
            locomotion.playerRig = rig.transform;
            player.leftHand = lhand.transform;
            player.rightHand = rhand.transform;

            // GameManager
            var gmGO = new GameObject("[GS] GameManager");
            var gm = gmGO.AddComponent<GameManager>();
            gm.player = rig.transform;

            // Gore manager
            new GameObject("[GS] GoreManager").AddComponent<GoreManager>();

            // Spawn menu canvas (world space)
            var canvasGO = new GameObject("[GS] SpawnMenuCanvas");
            canvasGO.transform.SetParent(rig.transform, false);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var rect = canvasGO.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(700, 500);
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

        static GameObject MakeWall(string name, Vector3 pos, Vector3 scale)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = scale;
            var rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            return go;
        }

        static GameObject MakeHand(string name, Transform parent, Vector3 localPos, XRNode node)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var col = go.AddComponent<BoxCollider>();
            col.size = Vector3.one * 0.18f;
            col.isTrigger = true;
            var hand = go.AddComponent<VHand>();
            hand.node = node;
            go.AddComponent<PunchImpact>();
            return go;
        }
    }
}
