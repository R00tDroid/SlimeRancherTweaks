using UnityEngine;

namespace SRTweaks
{
    public class SRTweaksConfigUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(50, 50, 500, 650);
        private Vector2 scrollPosition = new Vector2();
        private bool windowVisible = false;
        private SRInput.InputMode previousInput;

        void Awake()
        {
            LoadConfig();

            if (Main.tweaks.Length > 0)
            {
                currentTab = Main.tweaks[0];
            }
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
                LoadConfig();
                
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
            foreach (ITweakBase tweak in Main.tweaks)
            {
                ITweakSettingsUI ui = tweak.GetSettingsUI();
                if (ui != null)
                {
                    ui.Load();
                }
            }
        }

        void SaveConfig()
        {
            foreach (ITweakBase tweak in Main.tweaks)
            {
                ITweakSettingsUI ui = tweak.GetSettingsUI();
                if (ui != null)
                {
                    ui.Save();
                }
            }

            LoadConfig();

            Main.ApplySettings();
        }

        void OnGUI()
        {
            if (windowVisible)
            {
                windowRect = GUI.Window(0, windowRect, WindowLayout, "Slime Rancher Drone Tweaks");
            }
        }

        private ITweakBase currentTab = null;

        void WindowLayout(int windowId)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Close"))
            {
                HideWindow();
            }

            if (GUILayout.Button("Apply"))
            {
                SaveConfig();
            }

            GUILayout.Space(5);

            GUILayoutOption[] tabButtonStyle = new GUILayoutOption[] { GUILayout.Width(100) };

            GUILayout.BeginHorizontal();
            foreach (ITweakBase tweak in Main.tweaks)
            {
                ITweakSettingsUI ui = tweak.GetSettingsUI();
                if (ui != null)
                {
                    GUI.enabled = tweak != currentTab;
                    if (GUILayout.Button(ui.GetTabName(), tabButtonStyle))
                    {
                        currentTab = tweak;
                    }
                }
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (currentTab != null)
            {
                ITweakSettingsUI ui = currentTab.GetSettingsUI();
                if (ui != null)
                {
                    ui.OnGUI();
                    GUILayout.Space(2);
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
            
            GUI.DragWindow();
        }
    }
}