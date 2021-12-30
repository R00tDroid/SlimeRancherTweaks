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

            SRML.Console.Console.RegisterCommand(new SetDroneLimitCommand());
            SRML.Console.Console.RegisterCommand(new SetDroneSpeedCommand());
            SRML.Console.Console.RegisterCommand(new SetDroneInventoryCommand());
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
        private string droneLimit;

        public override void OnGUI()
        {
            GUILayout.Label("Drone");
            GUILayout.Space(2);
            GUILayout.Label("Maximum number of drones per ranch expansion");
            string newValue = GUILayout.TextField(droneLimit, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != droneLimit)
            {
                if (uint.TryParse(newValue, out uint dummy))
                {
                    droneLimit = newValue;
                }
            }
        }

        public override void Load()
        {
            droneLimit = DroneTweaks.DroneLimit.ToString();
        }

        public override void Save()
        {
            if (uint.TryParse(droneLimit, out uint newValue))
            {
                DroneTweaks.DroneLimit = newValue;
            }
        }
    }

    public class SetDroneLimitCommand : ConsoleCommand
    {
        public override string Usage => "dronelimit [count]";
        public override string ID => "dronelimit";
        public override string Description => "gets or sets the number of drones per ranch expansion";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Drone limit: " + DroneTweaks.DroneLimit + " (default: 2)");
                return true;
            }

            if (!int.TryParse(args[0], out int newLimit))
            {
                return false;
            }

            if (newLimit < 0)
            {
                return false;
            }

            DroneTweaks.DroneLimit = (uint)newLimit;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetDroneSpeedCommand : ConsoleCommand
    {
        public override string Usage => "dronespeed [percentage]";
        public override string ID => "dronespeed";
        public override string Description => "gets or sets the drone speed multiplier (percentage)";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Drone speed: " + DroneTweaks.DroneSpeedMultiplier + " (default: 100)");
                return true;
            }

            if (!int.TryParse(args[0], out int newValue))
            {
                return false;
            }

            if (newValue < 0)
            {
                return false;
            }

            DroneTweaks.DroneSpeedMultiplier = (uint)newValue;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetDroneInventoryCommand : ConsoleCommand
    {
        public override string Usage => "droneinventory [count]";
        public override string ID => "droneinventory";
        public override string Description => "gets or sets the number of items a drone can hold";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Drone inventory size: " + DroneTweaks.DroneInventoryMax + " (default: 50)");
                return true;
            }

            if (!int.TryParse(args[0], out int newValue))
            {
                return false;
            }

            if (newValue < 1)
            {
                return false;
            }

            DroneTweaks.DroneInventoryMax = (uint)newValue;
            Main.ApplySettings();
            return true;
        }
    }
}
