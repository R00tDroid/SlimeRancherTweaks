using UnityEngine;
using System.Reflection;
using SRML;
using SRML.SR;
using SRML.SR.SaveSystem;
using SRML.SR.SaveSystem.Registry;

namespace SRTweaks
{
    public class SettingsStorage
    {
        public SettingsStorage(SRML.SR.SaveSystem.Data.CompoundDataPiece inStorage)
        {
            storage = inStorage;
        }

        public T GetValue<T>(string key) => (T)storage.GetValue(key);

        public void SetValue(string key, object value) => storage.SetValue(key, value);

        public bool HasPiece(string key) => storage.HasPiece(key);

        private SRML.SR.SaveSystem.Data.CompoundDataPiece storage;
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

    public class Main : ModEntryPoint
    {
        public static ITweakBase[] tweaks;

        public static T GetSaveValue<T>(SettingsStorage data, string name, T defaultValue)
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
                SettingsStorage storage = new SettingsStorage(data);
                PluginLog("Load");
                foreach (ITweakBase tweak in tweaks)
                {
                    tweak.LoadSettings(storage);
                }
            }));

            SaveRegistry.RegisterWorldDataSaveDelegate((WorldDataSaveDelegate)(data =>
            {
                SettingsStorage storage = new SettingsStorage(data);
                PluginLog("Save");
                foreach (ITweakBase tweak in tweaks)
                {
                    tweak.SaveSettings(storage);
                }
            }));
        }

        public override void Load()
        {
            foreach (ITweakBase tweak in tweaks)
            {
                tweak.Load();
            }
        }

        public static void ApplySettings()
        {
            foreach (ITweakBase tweak in tweaks)
            {
                tweak.ApplySettings();
            }
        }

        public static void PluginLog(string logString)
        {
            Debug.Log("SRTweaks> " + logString);
        }
    }
}
