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
        public static bool HidePlayer = false; // Default false;

        public override void PreLoad()
        {
            Harmony harmony = HarmonyPatcher.GetInstance();

            MethodInfo methodOriginal = typeof(GordoDisplayOnMap).GetMethod("ShowOnMap");
            MethodInfo methodNew = typeof(MapTweaks).GetMethod("GordoShowOnMap");
            Main.Log("Patching GordoDisplayOnMap.ShowOnMap: " + methodOriginal + " > " + methodNew);
            harmony.Patch(methodOriginal, new HarmonyMethod(methodNew));

            methodOriginal = typeof(PlayerDisplayOnMap).GetMethod("ShowOnMap");
            methodNew = typeof(MapTweaks).GetMethod("PlayerShowOnMap");
            Main.Log("Patching PlayerDisplayOnMap.ShowOnMap: " + methodOriginal + " > " + methodNew);
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
            data.SetValue("HidePlayer", HidePlayer);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            ShowGordos = Main.GetSaveValue<bool>(data, "ShowGordos", false);
            ShowGordosWithoutDiscovery = Main.GetSaveValue<bool>(data, "ShowGordosWithoutDiscovery", false);
            HidePlayer = Main.GetSaveValue<bool>(data, "HidePlayer", false);
        }

        private ITweakSettingsUI SettingsUI = new MapTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }

        public static bool GordoShowOnMap(GordoDisplayOnMap __instance, ref bool __result)
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
                __result = eatenCount > 0 || MapTweaks.ShowGordosWithoutDiscovery;
                return false;
            }
        }

        public static bool PlayerShowOnMap(ref bool __result)
        {
            if (!MapTweaks.HidePlayer)
            {
                return true;
            }
            else
            {
                __result = false;
                return false;
            }
        }
    }

    public class MapTweaksSettingsUI : ITweakSettingsUI
    {
        private bool showGordos; 
        private bool showGordosWithoutDiscovery;
        private bool hidePlayer;

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
            hidePlayer = GUILayout.Toggle(hidePlayer, "Hide Player");
        }

        public override void Load()
        {
            showGordos = MapTweaks.ShowGordos;
            showGordosWithoutDiscovery = MapTweaks.ShowGordosWithoutDiscovery;
            hidePlayer = MapTweaks.HidePlayer;
        }

        public override void Save()
        {
            MapTweaks.ShowGordos = showGordos;
            MapTweaks.ShowGordosWithoutDiscovery = showGordosWithoutDiscovery;
            MapTweaks.HidePlayer = hidePlayer;
        }
    }
}
