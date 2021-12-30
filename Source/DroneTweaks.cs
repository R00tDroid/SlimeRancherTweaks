using System;
using UnityEngine;
using System.Reflection;
using SRML;
using HarmonyLib;
using SRML.SR;
using SRML.Console;
using Console = SRML.Console.Console;

namespace SRTweaks
{
    public class DroneTweaks : ITweak
    {
        public override void PreLoad()
        {
            MethodInfo methodOriginal = typeof(DroneAmmo).GetMethod("GetSlotMaxCount", new Type[] { });
            MethodInfo methodNew = typeof(DroneTweaks).GetMethod("GetDroneInventoryLimit", BindingFlags.Static | BindingFlags.Public);

            Debug.Log("Patching drone.ammo.GetSlotMaxCount: " + methodOriginal + " > " + methodNew);

            Harmony harmony = HarmonyPatcher.GetInstance();
            harmony.Patch(methodOriginal, new HarmonyMethod(methodNew));
        }

        public override void GameLoaded()
        {
            // Set drone limit per ranch expansion
            {
                LookupDirector lookupDirector = SRBehaviour.FindObjectOfType<LookupDirector>();
                GadgetDefinition droneGadget = lookupDirector.GetGadgetDefinition(Gadget.Id.DRONE);
                droneGadget.countLimit = (int)Main.DroneLimit;

                droneGadget = lookupDirector.GetGadgetDefinition(Gadget.Id.DRONE_ADVANCED);
                droneGadget.countLimit = (int)Main.DroneLimit;
            }

            // Set drone movement speed
            Drone[] drones = SRBehaviour.FindObjectsOfType<Drone>();
            foreach (Drone drone in drones)
            {
                drone.movement.movementSpeed = 180 * (Main.DroneSpeedMultiplier / 100.0f);
            }
        }

        // Function to override drone inventory limit
        public static bool GetDroneInventoryLimit(ref int __result)
        {
            __result = (int) Main.DroneInventoryMax;
            return false;
        }
    }
}