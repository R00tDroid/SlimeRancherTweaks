using System.Globalization;
using SRML.Console;
using SRML.SR.SaveSystem.Data;
using UnityEngine;

namespace SRTweaks
{
    public class CorralTweaks : ITweak<CorralTweaks>
    {
        public static uint AirNetDurabilityMultiplier = 100; // Default 100;
        public static float AirNetRecoverDelay = 0.1f; // Default 0.1;
        public static float AirNetRecoverDuration = 0.1f; // Default 0.1;
        public static float CollectorDelayHours = 1.0f; // default 1.0
        public static uint ItemsPerFeed = 6; // default 6;


        public override void Load()
        {
            GameObject corralPrefab = SRSingleton<GameContext>.Instance.LookupDirector.GetPlotPrefab(LandPlot.Id.CORRAL);
            Main.Log("Injecting CorralTweaksComponent into LandPlot.Id.CORRAL: " + corralPrefab);
            if (corralPrefab.GetComponent<CorralTweaksComponent>() == null)
            {
                CorralTweaksComponent corralTweaks = corralPrefab.AddComponent<CorralTweaksComponent>();
                Main.Log("Created: " + corralTweaks);
            }
            else
            {
                Main.Log("already present");
            }
        }

        public override void ApplySettings()
        {
            CorralTweaksComponent[] tweakComponents = SRBehaviour.FindObjectsOfType<CorralTweaksComponent>();
            foreach (CorralTweaksComponent tweakComponent in tweakComponents)
            {
                tweakComponent.ApplySettings();
            }
        }

        public override void SaveSettings(SettingsStorage data)
        {
            data.SetValue("AirNetDurabilityMultiplier", AirNetDurabilityMultiplier);
            data.SetValue("AirNetRecoverDelay", AirNetRecoverDelay);
            data.SetValue("AirNetRecoverDuration", AirNetRecoverDuration);
            data.SetValue("CollectorDelayHours", CollectorDelayHours);
            data.SetValue("ItemsPerFeed", ItemsPerFeed);
        }

        public override void LoadSettings(SettingsStorage data)
        {
            AirNetDurabilityMultiplier = Main.GetSaveValue<uint>(data, "AirNetDurabilityMultiplier", 100);
            AirNetRecoverDelay = Main.GetSaveValue<float>(data, "AirNetRecoverDelay", 0.1f);
            AirNetRecoverDuration = Main.GetSaveValue<float>(data, "AirNetRecoverDuration", 0.1f);
            CollectorDelayHours = Main.GetSaveValue<float>(data, "CollectorDelayHours", 1.0f);
            ItemsPerFeed = Main.GetSaveValue<uint>(data, "ItemsPerFeed", 6);
        }

        private ITweakSettingsUI SettingsUI = new CorralTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }
    }

    public class CorralTweaksSettingsUI : ITweakSettingsUI
    {
        private NumberField<uint> airNetDurabilityMultiplier = new NumberField<uint>();
        private NumberField<float> airNetRecoverDelay = new NumberField<float>();
        private NumberField<float> airNetRecoverDuration = new NumberField<float>();
        private NumberField<float> collectorDelayHours = new NumberField<float>();
        private NumberField<uint> itemsPerFeed = new NumberField<uint>();

        public override string GetTabName()
        {
            return "Corrals";
        }

        public override void OnGUI()
        {
            GUILayout.Label("AirNet durability (default: 100)");
            airNetDurabilityMultiplier.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("AirNet recovery delay in game hours (default: 0.1)");
            airNetRecoverDelay.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("AirNet recovery time in game hours (default: 0.1)");
            airNetRecoverDuration.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("Plot collector delay in game hours (default: 1.0)");
            collectorDelayHours.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("Feeder items to drop per cycle (default: 6)");
            itemsPerFeed.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
        }

        public override void Load()
        {
            airNetDurabilityMultiplier.Load(CorralTweaks.AirNetDurabilityMultiplier);
            airNetRecoverDelay.Load(CorralTweaks.AirNetRecoverDelay);
            airNetRecoverDuration.Load(CorralTweaks.AirNetRecoverDuration);
            collectorDelayHours.Load(CorralTweaks.CollectorDelayHours);
            itemsPerFeed.Load(CorralTweaks.ItemsPerFeed);
        }

        public override void Save()
        {
            airNetDurabilityMultiplier.Save(ref CorralTweaks.AirNetDurabilityMultiplier);
            airNetRecoverDelay.Save(ref CorralTweaks.AirNetRecoverDelay);
            airNetRecoverDuration.Save(ref CorralTweaks.AirNetRecoverDuration);
            collectorDelayHours.Save(ref CorralTweaks.CollectorDelayHours);
            itemsPerFeed.Save(ref CorralTweaks.ItemsPerFeed);
        } 
    }

    class CorralTweaksComponent : MonoBehaviour
    {
        public void OnEnable()
        {
            ApplySettings();
        }

        public void ApplySettings()
        {
            Main.Log("Apply corral tweaks: " + gameObject);

            // Set airnet durability by reducing hit damage
            float airNetDurability = CorralTweaks.AirNetDurabilityMultiplier / 100.0f;
            foreach (AirNet airnet in gameObject.GetComponentsInChildren<AirNet>())
            {
                airnet.dmgPerImpulse = 1f / (float)airnet.hitForceToDestroy / airNetDurability;
                airnet.recoverStartTime = CorralTweaks.AirNetRecoverDelay;
                airnet.hoursToRecover = CorralTweaks.AirNetRecoverDuration;
            }

            // Set plort collector delay
            foreach (PlortCollector plortCollector in gameObject.GetComponentsInChildren<PlortCollector>())
            {
                plortCollector.collectPeriod = CorralTweaks.CollectorDelayHours;

                if (plortCollector.model.collectorNextTime - plortCollector.timeDir.worldModel.worldTime > 3600.0 * (double)plortCollector.collectPeriod)
                {
                    Main.Log("Reset plort collector schedule");
                    plortCollector.model.collectorNextTime = plortCollector.timeDir.worldModel.worldTime;
                }
            }

            // Set amount of items a feeder will drop
            foreach (SlimeFeeder slimeFeeder in gameObject.GetComponentsInChildren<SlimeFeeder>())
            {
                slimeFeeder.itemsPerFeeding = (int)CorralTweaks.ItemsPerFeed;
            }
        }
    }
}
