﻿using System.Globalization;
using UnityEngine;
using System.Reflection;
using SRML;
using SRML.SR;
using SRML.SR.SaveSystem;
using SRML.SR.SaveSystem.Registry;

namespace SRTweaks
{
    public abstract class ITweakBase
    {
        public abstract void PreLoad();
        public abstract void GameLoaded();

        public abstract void ApplySettings();

        public abstract void SaveSettings(SRML.SR.SaveSystem.Data.CompoundDataPiece data);
        public abstract void LoadSettings(SRML.SR.SaveSystem.Data.CompoundDataPiece data);

        public virtual ITweakSettingsUI GetSettingsUI()
        {
            return null;
        }
    }

    public abstract class ITweak<ClassType> : ITweakBase where ClassType : class, new()
    {
        private static ClassType _instance;
        public static ClassType Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ClassType();
                }
                return _instance;
            }
        }
    }

    public abstract class ITweakSettingsUI
    {
        public abstract string GetTabName();

        public abstract void OnGUI();

        public abstract void Load();
        public abstract void Save();
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

    public class Main : ModEntryPoint
    {
        public static ITweakBase[] tweaks;

        public static T GetSaveValue<T>(SRML.SR.SaveSystem.Data.CompoundDataPiece data, string name, T defaultValue)
        {
            if (data.HasPiece(name))
            {
                return data.GetValue<T>(name);
            }
            else
            {
                return defaultValue;
            }
        }

        public override void PreLoad()
        {
            HarmonyPatcher.GetInstance().PatchAll(Assembly.GetExecutingAssembly());

            SRCallbacks.OnSaveGameLoaded += context => SRSingleton<SceneContext>.Instance.Player.AddComponent<SRTweaksConfigUI>();

            tweaks = new ITweakBase[] { GameModeTweaks.Instance, CorralTweaks.Instance, MapTweaks.Instance, DroneTweaks.Instance };

            foreach (ITweakBase tweak in tweaks)
            {
                tweak.PreLoad();
            }

            SRCallbacks.OnSaveGameLoaded += (scenecontext) =>
            {
                foreach (ITweakBase tweak in tweaks)
                {
                    tweak.GameLoaded();
                }

                ApplySettings();
            };

            SaveRegistry.RegisterWorldDataPreLoadDelegate((WorldDataPreLoadDelegate) (data =>
            {
                Log("Load");
                foreach (ITweakBase tweak in tweaks)
                {
                    tweak.LoadSettings(data);
                }
            }));

            SaveRegistry.RegisterWorldDataSaveDelegate((WorldDataSaveDelegate)(data =>
            {
                Log("Save");
                foreach (ITweakBase tweak in tweaks)
                {
                    tweak.SaveSettings(data);
                }
            }));
        }

        public static void ApplySettings()
        {
            foreach (ITweakBase tweak in tweaks)
            {
                tweak.ApplySettings();
            }
        }

        public static void Log(string logString)
        {
            Debug.Log("SRTweaks> " + logString);
        }
    }
}
