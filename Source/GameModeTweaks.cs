﻿using SRML.Console;
using SRML.SR.SaveSystem.Data;
using UnityEngine;

namespace SRTweaks
{
    public class GameModeTweaks : ITweak<GameModeTweaks>
    {
        public static bool AllowTarrSpawns = true; // Default true;
        public static bool SuppressTutorials = false; // Default false;
        public static bool InstantUpgrades = false; // Default false;
        public static bool ReceiveMails = true; // Default true;

        public override void PreLoad()
        {
            SRML.Console.Console.RegisterCommand(new SetTarrSpawnCommand());
            SRML.Console.Console.RegisterCommand(new SetSuppressTutorialsCommand());
            SRML.Console.Console.RegisterCommand(new SetInstantUpgradesCommand());
            SRML.Console.Console.RegisterCommand(new SetReceiveMailsCommand());
        }

        public override void GameLoaded()
        {
        }

        public override void ApplySettings()
        {
            // Set Gamemode spawning
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().preventHostiles = !AllowTarrSpawns;
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().assumeExperiencedUser = SuppressTutorials;
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().immediateUpgrades = InstantUpgrades;
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().suppressStory = !ReceiveMails;
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("AllowTarrSpawns", AllowTarrSpawns);
            data.SetValue("SuppressTutorials", SuppressTutorials);
            data.SetValue("InstantUpgrades", InstantUpgrades);
            data.SetValue("ReceiveMails", ReceiveMails);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            AllowTarrSpawns = Main.GetSaveValue<bool>(data, "AllowTarrSpawns", true);
            SuppressTutorials = Main.GetSaveValue<bool>(data, "SuppressTutorials", false);
            InstantUpgrades = Main.GetSaveValue<bool>(data, "InstantUpgrades", false);
            ReceiveMails = Main.GetSaveValue<bool>(data, "ReceiveMails", true);
        }

        private ITweakSettingsUI SettingsUI = new GameModeTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }
    }

    public class GameModeTweaksSettingsUI : ITweakSettingsUI
    {
        private bool allowTarrSpawns;
        private bool suppressTutorials;
        private bool instantUpgrades;
        private bool receiveMails;

        public override string GetTabName()
        {
            return "Game Mode";
        }

        public override void OnGUI()
        {
            allowTarrSpawns = GUILayout.Toggle(allowTarrSpawns, "Allow Tarr to spawn (default: true)");
            suppressTutorials = GUILayout.Toggle(suppressTutorials, "Suppress tutorials (default: false)");
            instantUpgrades = GUILayout.Toggle(instantUpgrades, "Upgrades instantly available (default: false)");
            receiveMails = GUILayout.Toggle(receiveMails, "Receive mails (default: true)");
        }

        public override void Load()
        {
            allowTarrSpawns = GameModeTweaks.AllowTarrSpawns;
            suppressTutorials = GameModeTweaks.SuppressTutorials;
            instantUpgrades = GameModeTweaks.InstantUpgrades;
            receiveMails = GameModeTweaks.ReceiveMails;
        }

        public override void Save()
        {
            GameModeTweaks.AllowTarrSpawns = allowTarrSpawns;
            GameModeTweaks.SuppressTutorials = suppressTutorials;
            GameModeTweaks.InstantUpgrades = instantUpgrades;
            GameModeTweaks.ReceiveMails = receiveMails;
        }
    }

    public class SetTarrSpawnCommand : ConsoleCommand
    {
        public override string Usage => "allowtarr [True/False]";
        public override string ID => "allowtarr";
        public override string Description => "gets or sets allowance of Tarr spawning";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Can Tarrs spawn: " + GameModeTweaks.AllowTarrSpawns + " (default: True)");
                return true;
            }

            if (!bool.TryParse(args[0], out bool newValue))
            {
                return false;
            }

            GameModeTweaks.AllowTarrSpawns = newValue;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetSuppressTutorialsCommand : ConsoleCommand
    {
        public override string Usage => "suppresstutorials [True/False]";
        public override string ID => "suppresstutorials";
        public override string Description => "gets or sets whether tutorials will be suppressed";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Are tutorials suppressed: " + GameModeTweaks.SuppressTutorials + " (default: False)");
                return true;
            }

            if (!bool.TryParse(args[0], out bool newValue))
            {
                return false;
            }

            GameModeTweaks.SuppressTutorials = newValue;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetInstantUpgradesCommand : ConsoleCommand
    {
        public override string Usage => "instantupgrades [True/False]";
        public override string ID => "instantupgrades";
        public override string Description => "gets or sets whether upgrades will be available instantly";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Are upgrades instantly available: " + GameModeTweaks.InstantUpgrades + " (default: False)");
                return true;
            }

            if (!bool.TryParse(args[0], out bool newValue))
            {
                return false;
            }

            GameModeTweaks.InstantUpgrades = newValue;
            Main.ApplySettings();
            return true;
        }
    }

    public class SetReceiveMailsCommand : ConsoleCommand
    {
        public override string Usage => "receivemails [True/False]";
        public override string ID => "receivemails";
        public override string Description => "gets or sets whether mails will be received";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Will receive mails: " + GameModeTweaks.ReceiveMails + " (default: True)");
                return true;
            }

            if (!bool.TryParse(args[0], out bool newValue))
            {
                return false;
            }

            GameModeTweaks.ReceiveMails = newValue;
            Main.ApplySettings();
            return true;
        }
    }
}
