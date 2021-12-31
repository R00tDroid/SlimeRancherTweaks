using System;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using SRML;
using SRML.SR.SaveSystem.Data;
using UnityEngine;

namespace SRTweaks
{
    public class MapTweaks : ITweak<MapTweaks>
    {
        public static bool ShowGordos = false; // Default false;
        public static bool ShowGordosWithoutDiscovery = false; // Default false;
        public override void PreLoad()
        {
            MethodInfo methodOriginal = typeof(GordoDisplayOnMap).GetMethod("ShowOnMap");
            MethodInfo methodNew = typeof(GordoDisplayOnMapPatch).GetMethod("PreFix");

            Main.Log("Patching GordoDisplayOnMap.ShowOnMap: " + methodOriginal + " > " + methodNew);

            Harmony harmony = HarmonyPatcher.GetInstance();
            harmony.Patch(methodOriginal, new HarmonyMethod(methodNew));
        }

        public override void GameLoaded()
        {
        }

        public override void ApplySettings()
        {
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("ShowGordos", ShowGordos);
            data.SetValue("ShowGordosWithoutDiscovery", ShowGordosWithoutDiscovery);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            ShowGordos = Main.GetSaveValue<bool>(data, "ShowGordos", false);
            ShowGordosWithoutDiscovery = Main.GetSaveValue<bool>(data, "ShowGordosWithoutDiscovery", false);
        }

        private ITweakSettingsUI SettingsUI = new MapTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }
    }

    public class MapTweaksSettingsUI : ITweakSettingsUI
    {
        private bool showGordos; 
        private bool showGordosWithoutDiscovery;

        public override string GetTabName()
        {
            return "Map";
        }

        public override void OnGUI()
        {
            showGordos = GUILayout.Toggle(showGordos, "Show Gordos");
            if (showGordos)
            {
                showGordosWithoutDiscovery = GUILayout.Toggle(showGordosWithoutDiscovery, "Show unfed Gordos");
            }
        }

        public override void Load()
        {
            showGordos = MapTweaks.ShowGordos;
            showGordosWithoutDiscovery = MapTweaks.ShowGordosWithoutDiscovery;
        }

        public override void Save()
        {
            MapTweaks.ShowGordos = showGordos;
            MapTweaks.ShowGordosWithoutDiscovery = showGordosWithoutDiscovery;
        } 
    }

    public class GordoDisplayOnMapPatch
    {
        public static bool PreFix(GordoDisplayOnMap __instance, ref bool __result)
        {
            if (!MapTweaks.ShowGordos)
            {
                return true;
            }
            else
            {
                int eatenCount = __instance.gordoEat.GetEatenCount();
                GordoNearBurstOnGameMode component = (GordoNearBurstOnGameMode)__instance.gameObject.GetComponent<GordoNearBurstOnGameMode>();
                eatenCount -= component == null ? 0 : __instance.gordoEat.GetTargetCount() - (int)component.remaining;
                __result =  eatenCount > 0 || MapTweaks.ShowGordosWithoutDiscovery;
                return false;
            }
        }
    }
}
