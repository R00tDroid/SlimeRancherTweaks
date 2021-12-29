using UnityEngine;

namespace SRDrones
{
    public class SRDronesConfigUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 350, 250);
        private bool windowVisible = false;

        private string droneLimit;

        void Awake()
        {
            LoadConfig();
        }

        public void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.Y))
            {
                ShowWindow();
            }
        }

        public void ShowWindow()
        {
            if (!windowVisible)
            {
                windowVisible = true;
                SRInput.Instance.SetInputMode(SRInput.InputMode.PAUSE, this.gameObject.GetInstanceID());
            }
        }

        void HideWindow()
        {
            if (windowVisible)
            {
                windowVisible = false;
                SRInput.Instance.ClearInputMode(this.gameObject.GetInstanceID());
            }
        }

        void LoadConfig()
        {
            droneLimit = Main.DroneLimit.ToString();
        }

        void SaveConfig()
        {
            if (uint.TryParse(droneLimit, out uint newValue))
            {
                Main.DroneLimit = newValue;
            }

            LoadConfig();
        }

        void OnGUI()
        {
            if (windowVisible)
            {
                windowRect = GUI.Window(0, windowRect, WindowLayout, "Slime Rancher Drone Tweaks");
            }
        }

        void WindowLayout(int windowId)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Close"))
            {
                HideWindow();
            }

            if (GUILayout.Button("Save"))
            {
                SaveConfig();
            }

            GUILayout.Space(10);

            GUILayout.Label("Maximum amount of drones per ranch expansion");
            string newValue = GUILayout.TextField(droneLimit, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != droneLimit)
            {
                if (uint.TryParse(newValue, out uint dummy))
                {
                    droneLimit = newValue;
                }
            }

            GUILayout.EndVertical();
            
            GUI.DragWindow();
        }
    }
}