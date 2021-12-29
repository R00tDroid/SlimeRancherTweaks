using System;
using UnityEngine;
using System.Reflection;
using SRML;
using HarmonyLib;
using SRML.SR;
using SRML.Console;
using Console = SRML.Console.Console;

namespace SRDrones
{
    public class Main : ModEntryPoint
    {
        public static uint DroneLimit = 2; // Default 2;
        public static uint DroneSpeedMultiplier = 100; // Default 100;
        public static uint DroneInventoryMax = 100; // Default 50

        public override void PreLoad()
        {
            HarmonyPatcher.GetInstance().PatchAll(Assembly.GetExecutingAssembly());

            Console.RegisterCommand(new SetDroneLimitCommand());

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
                    MethodInfo methodNew =
                        typeof(Main).GetMethod("GetDroneInventoryLimit", BindingFlags.Static | BindingFlags.Public);

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
        public static bool GetDroneInventoryLimit(ref int __result)
        {
            __result = (int) DroneInventoryMax;
            return false;
        }

        public static void Log(string logString)
        {
            Debug.Log(logString);
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
                Main.Log("Drone limit: " + Main.DroneLimit + " (default: 2)");
                return true;
            }

            if (!int.TryParse(args[0], out int newLimit))
            {
                return false;
            }

            if (newLimit < 0)
            {
                return true;
            }

            Main.DroneLimit = (uint)newLimit;
            return true;
        }
    }
}
