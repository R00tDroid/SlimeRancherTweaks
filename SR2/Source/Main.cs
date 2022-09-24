using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using SRTweaks;

namespace SRTweaks
{
    [BepInPlugin("nl.R00tDroid.SRTweaks", "Slime Rancher Tweaks", "0.0.0")]
    public class Main : BasePlugin
    {
        public static ITweakBase[] tweaks;

        private void Awake()
        {
        }

        public override void Load()
        {
            PluginLog("Plugin loaded");

            tweaks = new ITweakBase[] { GameModeTweaks.Instance, CorralTweaks.Instance, MapTweaks.Instance, DroneTweaks.Instance };
        }

        public static void PluginLog(string logString)
        {
            Log.LogInfo(logString);
        }

        public static void ApplySettings()
        {
            foreach (ITweakBase tweak in tweaks)
            {
                tweak.ApplySettings();
            }
        }
    }

    public abstract class ITweakBase
    {
        public virtual void PreLoad() { }

        public virtual void Load() { }

        public virtual void GameLoaded() { }

        public virtual void ApplySettings() { }

        public abstract void SaveSettings(SettingsStorage data);
        public abstract void LoadSettings(SettingsStorage data);

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

    public class SettingsStorage
    {
        public T GetValue<T>(string key)
        {
            //TODO return value
            return null;
        }


        public void SetValue(string key, object value)
        {
            //TODO store value
        }
    }
}
