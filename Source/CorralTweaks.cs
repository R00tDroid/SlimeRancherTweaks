﻿using System.Globalization;
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

        public override void PreLoad()
        {
            SRML.Console.Console.RegisterCommand(new SetAirNetDurabilityCommand());
            SRML.Console.Console.RegisterCommand(new SetAirNetRecoveryCommand());
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
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("AirNetDurabilityMultiplier", AirNetDurabilityMultiplier);
            data.SetValue("AirNetRecoverDelay", AirNetRecoverDelay);
            data.SetValue("AirNetRecoverDuration", AirNetRecoverDuration);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            AirNetDurabilityMultiplier = Main.GetSaveValue<uint>(data, "AirNetDurabilityMultiplier", 100);
            AirNetRecoverDelay = Main.GetSaveValue<float>(data, "AirNetRecoverDelay", 0.1f);
            AirNetRecoverDuration = Main.GetSaveValue<float>(data, "AirNetRecoverDuration", 0.1f);
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
        }

        public override void Load()
        {
            airNetDurabilityMultiplier = CorralTweaks.AirNetDurabilityMultiplier.ToString();
            airNetRecoverDelay = CorralTweaks.AirNetRecoverDelay.ToString(CultureInfo.InvariantCulture);
            airNetRecoverDuration = CorralTweaks.AirNetRecoverDuration.ToString(CultureInfo.InvariantCulture);
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
}