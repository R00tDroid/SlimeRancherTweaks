using System;
using System.Reflection;
using SRML;
using HarmonyLib;
using SRML.Console;
using SRML.SR.SaveSystem.Data;
using UnityEngine;

namespace SRTweaks
{
    public class DroneTweaks : ITweak<DroneTweaks>
    {
        public static uint DroneLimit = 2; // Default 2;
        public static uint DroneSpeedMultiplier = 100; // Default 100;
        public static uint DroneInventoryMax = 50; // Default 50

        public override void PreLoad()
        {
            MethodInfo methodOriginal = typeof(DroneAmmo).GetMethod("GetSlotMaxCount", new Type[] { });
            MethodInfo methodNew = typeof(DroneTweaks).GetMethod("GetDroneInventoryLimit", BindingFlags.Static | BindingFlags.Public);

            Main.Log("Patching drone.ammo.GetSlotMaxCount: " + methodOriginal + " > " + methodNew);

            Harmony harmony = HarmonyPatcher.GetInstance();
            harmony.Patch(methodOriginal, new HarmonyMethod(methodNew));
        }

        public override void GameLoaded()
        {
        }

        public override void ApplySettings()
        {
            // Set drone limit per ranch expansion
            {
                LookupDirector lookupDirector = SRBehaviour.FindObjectOfType<LookupDirector>();
                GadgetDefinition droneGadget = lookupDirector.GetGadgetDefinition(Gadget.Id.DRONE);
                droneGadget.countLimit = (int)DroneLimit;

                droneGadget = lookupDirector.GetGadgetDefinition(Gadget.Id.DRONE_ADVANCED);
                droneGadget.countLimit = (int)DroneLimit;
            }

            // Set drone movement speed
            Drone[] drones = SRBehaviour.FindObjectsOfType<Drone>();
            foreach (Drone drone in drones)
            {
                drone.movement.movementSpeed = 180 * (DroneSpeedMultiplier / 100.0f);
                drone.movement.rotationFacingSpeed = 1.0f * (DroneSpeedMultiplier / 100.0f);
            }
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("DroneLimit", DroneLimit);
            data.SetValue("DroneSpeedMultiplier", DroneSpeedMultiplier);
            data.SetValue("DroneInventoryMax", DroneInventoryMax);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            DroneLimit = Main.GetSaveValue<uint>(data, "DroneLimit", 2);
            DroneSpeedMultiplier = Main.GetSaveValue<uint>(data, "DroneSpeedMultiplier", 100);
            DroneInventoryMax = Main.GetSaveValue<uint>(data, "DroneInventoryMax", 50);
        }

        private ITweakSettingsUI SettingsUI = new DroneTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }

        // Function to override drone inventory limit
        public static bool GetDroneInventoryLimit(ref int __result)
        {
            __result = (int)DroneInventoryMax;
            return false;
        }
    }

    public class DroneTweaksSettingsUI : ITweakSettingsUI
    {
        private NumberField<uint> droneLimit = new NumberField<uint>();
        private NumberField<uint> droneSpeedMultiplier = new NumberField<uint>();
        private NumberField<uint> droneInventoryMax = new NumberField<uint>();

        public override string GetTabName()
        {
            return "Drones";
        }

        public override void OnGUI()
        {
            GUILayout.Label("Maximum number of drones per ranch expansion (default: 2)");
            droneLimit.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("Movement speed (default: 100)");
            droneSpeedMultiplier.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("Inventory size (default: 50)");
            droneInventoryMax.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
        }

        public override void Load()
        {
            droneLimit.Load(DroneTweaks.DroneLimit);
            droneSpeedMultiplier.Load(DroneTweaks.DroneSpeedMultiplier);
            droneInventoryMax.Load(DroneTweaks.DroneInventoryMax);
        }

        public override void Save()
        {
            droneLimit.Save(ref DroneTweaks.DroneLimit);
            droneSpeedMultiplier.Save(ref DroneTweaks.DroneSpeedMultiplier);
            droneInventoryMax.Save(ref DroneTweaks.DroneInventoryMax);
        }
    }
}
