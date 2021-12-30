using UnityEngine;

namespace SRTweaks
{
    public class SRTweaksConfigUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(20, 20, 350, 250);
        private bool windowVisible = false;
        private SRInput.InputMode previousInput;

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
            bool isPaused = SRSingleton<SceneContext>.Instance.TimeDirector.HasPauser();

            if (!windowVisible && !isPaused)
            {
                windowVisible = true;
                previousInput = SRInput.Instance.GetInputMode();
                SRInput.Instance.SetInputMode(SRInput.InputMode.NONE);
                SRSingleton<SceneContext>.Instance.TimeDirector.Pause();
            }
        }

        void HideWindow()
        {
            if (windowVisible)
            {
                windowVisible = false;
                SRInput.Instance.SetInputMode(previousInput);
                SRSingleton<SceneContext>.Instance.TimeDirector.Unpause();
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