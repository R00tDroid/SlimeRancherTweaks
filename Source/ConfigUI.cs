using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace SRTweaks
{
    public class SRTweaksConfigUI : MonoBehaviour
    {
        private Rect windowRect = new Rect(50, 50, 500, 650);
        private Vector2 scrollPosition = new Vector2();
        private bool windowVisible = false;
        private SRInput.InputMode previousInput;

        private string[] tabNames;
        private ITweakSettingsUI[] tabInstances;
        private int currentTab = -1;

        void Awake()
        {
            LoadConfig();

            List<string> names = new List<string>();
            List<ITweakSettingsUI> instances = new List<ITweakSettingsUI>();

            foreach (ITweakBase tweak in Main.tweaks)
            {
                ITweakSettingsUI ui = tweak.GetSettingsUI();
                if (ui != null)
                {
                    names.Add(ui.GetTabName());
                    instances.Add(ui);
                }
            }

            tabNames = names.ToArray();
            tabInstances = instances.ToArray();
            if (tabNames.Length > 0)
            {
                currentTab = 0;
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
                windowRect = GUI.Window(0, windowRect, WindowLayout, "Slime Rancher Game Tweaks");
            }
        }

        void WindowLayout(int windowId)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Save & Close"))
            {
                SaveConfig();
                HideWindow();
            }

            GUILayout.Space(5);

            GUILayoutOption[] tabButtonStyle = new GUILayoutOption[] { GUILayout.Width(100) };

            currentTab = GUILayout.Toolbar(currentTab, tabNames, new GUILayoutOption[] {GUILayout.ExpandWidth(true) });

            GUILayout.Space(5);

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (currentTab >= 0)
            {
                ITweakSettingsUI ui = tabInstances[currentTab];
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

    public class NumberField<T>
    {
        private string valueString;

        public void ShowGUI(GUILayoutOption[] layoutOptions)
        {
            string newValue = GUILayout.TextField(valueString, layoutOptions);
            if (newValue != valueString)
            {
                if (typeof(T) == typeof(float))
                {
                    if (float.TryParse(newValue, out float dummy))
                    {
                        valueString = newValue;
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(newValue, out int dummy))
                    {
                        valueString = newValue;
                    }
                }
                else if (typeof(T) == typeof(uint))
                {
                    if (uint.TryParse(newValue, out uint dummy))
                    {
                        valueString = newValue;
                    }
                }
                else
                {
                    Main.Log("Unsupported type: " + typeof(T).FullName);
                }
            }
        }

        public void Load(T value)
        {
            if (typeof(T) == typeof(float))
            {
                valueString = ((float)(object)value).ToString(CultureInfo.InvariantCulture);
            }
            else if (typeof(T) == typeof(int))
            {
                valueString = ((int)(object)value).ToString();
            }
            else if (typeof(T) == typeof(uint))
            {
                valueString = ((uint)(object)value).ToString();
            }
            else
            {
                Main.Log("Unsupported type: " + typeof(T).FullName);
            }
        }

        public void Save(ref T value)
        {
            if (typeof(T) == typeof(float))
            {
                if (float.TryParse(valueString, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float newValue))
                {
                    value = (T)(object)newValue;
                }
            }
            else if (typeof(T) == typeof(int))
            {
                if (int.TryParse(valueString, out int newValue))
                {
                    value = (T)(object)newValue;
                }
            }
            else if (typeof(T) == typeof(uint))
            {
                if (uint.TryParse(valueString, out uint newValue))
                {
                    value = (T)(object)newValue;
                }
            }
            else
            {
                Main.Log("Unsupported type: " + typeof(T).FullName);
            }
        }
    }
}