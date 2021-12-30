using UnityEngine;
using System.Reflection;
using SRML;
using SRML.SR;
using SRML.Console;
using SRML.SR.SaveSystem;
using SRML.SR.SaveSystem.Registry;
using Console = SRML.Console.Console;

namespace SRTweaks
{
    public abstract class ITweak
    {
        public abstract void PreLoad();
        public abstract void GameLoaded();
    }

    public class Main : ModEntryPoint
    {
        private ITweak[] tweaks;

        public static uint DroneLimit = 2; // Default 2;
        public static uint DroneSpeedMultiplier = 100; // Default 100;
        public static uint DroneInventoryMax = 50; // Default 50

        static T GetSaveValue<T>(SRML.SR.SaveSystem.Data.CompoundDataPiece data, string name, T defaultValue)
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

            Console.RegisterCommand(new SetDroneLimitCommand());

            SRCallbacks.OnSaveGameLoaded += context => SRSingleton<SceneContext>.Instance.Player.AddComponent<SRTweaksConfigUI>();

            tweaks = new ITweak[] { new DroneTweaks() };

            foreach (ITweak tweak in tweaks)
            {
                tweak.PreLoad();
            }

            SRCallbacks.OnSaveGameLoaded += (scenecontext) =>
            {
                foreach (ITweak tweak in tweaks)
                {
                    tweak.GameLoaded();
                }
            };

            SaveRegistry.RegisterWorldDataPreLoadDelegate((WorldDataPreLoadDelegate) (data =>
            {
                Log("Load");
                DroneLimit = GetSaveValue<uint>(data, "DroneLimit", 2);
                DroneSpeedMultiplier = GetSaveValue<uint>(data, "DroneSpeedMultiplier", 100);
                DroneInventoryMax = GetSaveValue<uint>(data, "DroneInventoryMax", 50);

                Log("DroneLimit: " + DroneLimit);
            }));

            SaveRegistry.RegisterWorldDataSaveDelegate((WorldDataSaveDelegate)(data =>
            {
                Log("Save");
                data.SetValue("DroneLimit", DroneLimit);
                data.SetValue("DroneSpeedMultiplier", DroneSpeedMultiplier);
                data.SetValue("DroneInventoryMax", DroneInventoryMax);
            }));
        }

        public static void Log(string logString)
        {
            Debug.Log("SRTweaks> " + logString);
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
