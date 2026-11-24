using UnityEngine;
using UnityEngine.UI;

namespace GoreSandbox
{
    /// <summary>
    /// Simple world-space spawn menu toggled near the player's head.
    /// Keys 1-7 also spawn directly (keyboard fallback / testing).
    /// </summary>
    public class SpawnMenu : MonoBehaviour
    {
        public Canvas canvas;
        public Button btnNPC;
        public Button btnKnuckles;
        public Button btnSword;
        public Button btnBrick;
        public Button btnVase;
        public Button btnPlate;
        public Button btnCup;
        public Button btnClose;

        GameManager gm;
        bool open;

        void Awake()
        {
            gm = GameManager.Instance;
            if (canvas != null) canvas.gameObject.SetActive(false);
            Wire(btnNPC, () => gm.SpawnNPC());
            Wire(btnKnuckles, () => gm.SpawnItem("brass"));
            Wire(btnSword, () => gm.SpawnItem("sword"));
            Wire(btnBrick, () => gm.SpawnItem("brick"));
            Wire(btnVase, () => gm.SpawnItem("vase"));
            Wire(btnPlate, () => gm.SpawnItem("plate"));
            Wire(btnCup, () => gm.SpawnItem("cup"));
            Wire(btnClose, Toggle);
        }

        void Wire(Button b, System.Action a)
        {
            if (b != null) b.onClick.AddListener(() => a());
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) Toggle();

            if (Input.GetKeyDown(KeyCode.Alpha1)) gm.SpawnNPC();
            else if (Input.GetKeyDown(KeyCode.Alpha2)) gm.SpawnItem("brass");
            else if (Input.GetKeyDown(KeyCode.Alpha3)) gm.SpawnItem("sword");
            else if (Input.GetKeyDown(KeyCode.Alpha4)) gm.SpawnItem("brick");
            else if (Input.GetKeyDown(KeyCode.Alpha5)) gm.SpawnItem("vase");
            else if (Input.GetKeyDown(KeyCode.Alpha6)) gm.SpawnItem("plate");
            else if (Input.GetKeyDown(KeyCode.Alpha7)) gm.SpawnItem("cup");
        }

        public void Toggle()
        {
            open = !open;
            if (canvas == null) return;
            canvas.gameObject.SetActive(open);
            if (open && Player.Instance != null && Player.Instance.head != null)
            {
                canvas.transform.position = Player.Instance.head.position + Player.Instance.head.forward * 1.5f;
                canvas.transform.rotation = Quaternion.LookRotation(-Player.Instance.head.forward);
            }
        }
    }
}
