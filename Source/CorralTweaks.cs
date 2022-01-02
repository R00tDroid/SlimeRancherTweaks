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

        public override void PreLoad()
        {
            SRML.Console.Console.RegisterCommand(new SetAirNetDurabilityCommand());
            SRML.Console.Console.RegisterCommand(new SetAirNetRecoveryCommand());
            SRML.Console.Console.RegisterCommand(new SetCollectorDelayCommand());
        }

        public override void GameLoaded()
        {
        }

        public override void ApplySettings()
        {
            // Set airnet durability by reducing hit damage
            AirNet[] airnets = SRBehaviour.FindObjectsOfType<AirNet>();
            float airNetDurability = AirNetDurabilityMultiplier / 100.0f;
            foreach (AirNet airnet in airnets)
            {
                airnet.dmgPerImpulse = 1f / (float) airnet.hitForceToDestroy / airNetDurability;
                airnet.recoverStartTime = AirNetRecoverDelay;
                airnet.hoursToRecover = AirNetRecoverDuration;
            }

            // Set plort collector delay
            PlortCollector[] plortCollectors = SRBehaviour.FindObjectsOfType<PlortCollector>();
            foreach (PlortCollector plortCollector in plortCollectors)
            {
                plortCollector.collectPeriod = CollectorDelayHours;

                if (plortCollector.model.collectorNextTime - plortCollector.timeDir.worldModel.worldTime > 3600.0 * (double)plortCollector.collectPeriod)
                {
                    Main.Log("Reset plort collector schedule");
                    plortCollector.model.collectorNextTime = plortCollector.timeDir.worldModel.worldTime;
                }
            }

            // Set amount of items a feeder will drop
            SlimeFeeder[] slimeFeeders = SRBehaviour.FindObjectsOfType<SlimeFeeder>();
            foreach (SlimeFeeder slimeFeeder in slimeFeeders)
            {
                slimeFeeder.itemsPerFeeding = (int)ItemsPerFeed;
            }
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("AirNetDurabilityMultiplier", AirNetDurabilityMultiplier);
            data.SetValue("AirNetRecoverDelay", AirNetRecoverDelay);
            data.SetValue("AirNetRecoverDuration", AirNetRecoverDuration);
            data.SetValue("CollectorDelayHours", CollectorDelayHours);
            data.SetValue("ItemsPerFeed", ItemsPerFeed);
        }

        public override void LoadSettings(CompoundDataPiece data)
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
        private string airNetDurabilityMultiplier;
        private string airNetRecoverDelay;
        private string airNetRecoverDuration;
        private string collectorDelayHours;
        private string itemsPerFeed;

        public override string GetTabName()
        {
            return "Corrals";
        }

        public override void OnGUI()
        {
            GUILayout.Label("AirNet durability (default: 100)");
            string newValue = GUILayout.TextField(airNetDurabilityMultiplier, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != airNetDurabilityMultiplier)
            {
                if (uint.TryParse(newValue, out uint dummy))
                {
                    airNetDurabilityMultiplier = newValue;
                }
            }

            GUILayout.Label("AirNet recovery delay in game hours (default: 0.1)");
            newValue = GUILayout.TextField(airNetRecoverDelay, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != airNetRecoverDelay)
            {
                if (float.TryParse(newValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float dummy))
                {
                    airNetRecoverDelay = newValue;
                }
            }

            GUILayout.Label("AirNet recovery time in game hours (default: 0.1)");
            newValue = GUILayout.TextField(airNetRecoverDuration, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != airNetRecoverDuration)
            {
                if (float.TryParse(newValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float dummy))
                {
                    airNetRecoverDuration = newValue;
                }
            }

            GUILayout.Label("Plot collector delay in game hours (default: 1.0)");
            newValue = GUILayout.TextField(collectorDelayHours, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != collectorDelayHours)
            {
                if (float.TryParse(newValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float dummy))
                {
                    collectorDelayHours = newValue;
                }
            }

            GUILayout.Label("Feeder items to drop per cycle (default: 6)");
            newValue = GUILayout.TextField(itemsPerFeed, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != itemsPerFeed)
            {
                if (uint.TryParse(newValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out uint dummy))
                {
                    itemsPerFeed = newValue;
                }
            }
        }

        public override void Load()
        {
            airNetDurabilityMultiplier = CorralTweaks.AirNetDurabilityMultiplier.ToString();
            airNetRecoverDelay = CorralTweaks.AirNetRecoverDelay.ToString(CultureInfo.InvariantCulture);
            airNetRecoverDuration = CorralTweaks.AirNetRecoverDuration.ToString(CultureInfo.InvariantCulture);
            collectorDelayHours = CorralTweaks.CollectorDelayHours.ToString(CultureInfo.InvariantCulture);
            itemsPerFeed = CorralTweaks.ItemsPerFeed.ToString(CultureInfo.InvariantCulture);
        }

        public override void Save()
        {
            if (uint.TryParse(airNetDurabilityMultiplier, out uint newValue))
            {
                CorralTweaks.AirNetDurabilityMultiplier = newValue;
            }

            if (float.TryParse(airNetRecoverDelay, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float newFloatValue))
            {
                CorralTweaks.AirNetRecoverDelay = newFloatValue;
            }

            if (float.TryParse(airNetRecoverDuration, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out newFloatValue))
            {
                CorralTweaks.AirNetRecoverDuration = newFloatValue;
            }

            if (float.TryParse(collectorDelayHours, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out newFloatValue))
            {
                CorralTweaks.CollectorDelayHours = newFloatValue;
            }

            if (uint.TryParse(itemsPerFeed, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out uint newShortValue))
            {
                CorralTweaks.ItemsPerFeed = newShortValue;
            }
        } 
    }

    public class SetAirNetDurabilityCommand : ConsoleCommand
    {
        public override string Usage => "airnetdurability [percentage]";
        public override string ID => "airnetdurability";
        public override string Description => "gets or sets the AirNet durability multiplier";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("AirNet durability: " + CorralTweaks.AirNetDurabilityMultiplier + "% (default: 100)");
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

            CorralTweaks.AirNetDurabilityMultiplier = (uint)newValue;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetAirNetRecoveryCommand : ConsoleCommand
    {
        public override string Usage => "airnetrecovery [delay] [duration]";
        public override string ID => "airnetrecovery";
        public override string Description => "gets or sets the AirNet recovery delay and duration in game hours";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 2)
            {
                Main.Log("AirNet recovery: delay " + CorralTweaks.AirNetRecoverDelay.ToString(CultureInfo.InvariantCulture) + ", duration " + CorralTweaks.AirNetRecoverDuration.ToString(CultureInfo.InvariantCulture) + " (default: 0.1, 0.1)");
                return true;
            }

            if (!float.TryParse(args[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float newValue1))
            {
                return false;
            }

            if (!float.TryParse(args[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float newValue2))
            {
                return false;
            }

            if (newValue1 < 0)
            {
                return false;
            }

            if (newValue2 < 0)
            {
                return false;
            }

            CorralTweaks.AirNetRecoverDelay = newValue1;
            CorralTweaks.AirNetRecoverDuration = newValue2;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetCollectorDelayCommand : ConsoleCommand
    {
        public override string Usage => "collectordelay [game hours]";
        public override string ID => "collectordelay";
        public override string Description => "gets or sets the plort collector delay in game hours";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Plot collector delay: " + CorralTweaks.CollectorDelayHours + " (default: 1)");
                return true;
            }

            if (!float.TryParse(args[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out float newValue))
            {
                return false;
            }

            if (newValue < 0)
            {
                return false;
            }

            CorralTweaks.CollectorDelayHours = newValue;
            Main.ApplySettings();
            return true;
        }
    }
}
