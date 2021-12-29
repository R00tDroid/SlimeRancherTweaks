using System;
using UnityEngine;
using System.Reflection;
using SRML;
using HarmonyLib;
using SRML.SR;

namespace SRDrones
{
    public class Main : ModEntryPoint
    {
        private static uint DroneLimit = 10; // Default 2;
        private static uint DroneSpeedMultiplier = 100; // Default 100;
        private static uint DroneInventoryMax = 100; // Default 50

        public override void PreLoad()
        {
            HarmonyPatcher.GetInstance().PatchAll(Assembly.GetExecutingAssembly());

            SRCallbacks.OnSaveGameLoaded += (scenecontext) =>
            {
                Debug.Log("Injecting new drone parameters");

                // Set drone limit per ranch expansion
                {
                    LookupDirector lookupDirector = SRBehaviour.FindObjectOfType<LookupDirector>();
                    GadgetDefinition droneGadget = lookupDirector.GetGadgetDefinition(Gadget.Id.DRONE);
                    droneGadget.countLimit = (int) DroneLimit;

                    droneGadget = lookupDirector.GetGadgetDefinition(Gadget.Id.DRONE_ADVANCED);
                    droneGadget.countLimit = (int) DroneLimit;
                }

                // Set drone inventory limit
                {
                    MethodInfo methodOriginal = typeof(DroneAmmo).GetMethod("GetSlotMaxCount", new Type[] { });
                    MethodInfo methodNew = typeof(Main).GetMethod("GetDroneMax", BindingFlags.Static | BindingFlags.Public);

                    Debug.Log("Patching drone.ammo.GetSlotMaxCount: " + methodOriginal + " > " + methodNew);

                    Harmony harmony = HarmonyPatcher.GetInstance();
                    harmony.Patch(methodOriginal, new HarmonyMethod(methodNew));
                }

                // Set drone movement speed
                Drone[] drones = SRBehaviour.FindObjectsOfType<Drone>();
                foreach (Drone drone in drones)
                {
                    drone.movement.movementSpeed = 180 * (DroneSpeedMultiplier / 100.0f);
                }
            };
        }

        // Function to override drone inventory limit
        public static bool GetDroneMax(ref int __result)
        {
            __result = (int)DroneInventoryMax;
            return false;
        }
    }
}
