using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SRML.Console;
using SRML.SR.SaveSystem.Data;
using UnityEngine;

namespace SRTweaks
{
    public class GameModeTweaks : ITweak<GameModeTweaks>
    {
        public static int SelectedGameMode;
        public static bool SuppressTutorials = false; // Default false;
        public static bool InstantUpgrades = false; // Default false;
        public static bool ReceiveMails = true; // Default true;

        public override void PreLoad()
        {
            SRML.Console.Console.RegisterCommand(new SetSuppressTutorialsCommand());
            SRML.Console.Console.RegisterCommand(new SetInstantUpgradesCommand());
            SRML.Console.Console.RegisterCommand(new SetReceiveMailsCommand());
        }

        public override void GameLoaded()
        {
        }

        public override void ApplySettings()
        {
            // Set Gamemode
            SRSingleton<SceneContext>.Instance.GameModeConfig.gameModel.currGameMode = (PlayerState.GameMode)SelectedGameMode;

            // Set Gamemode spawning
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().assumeExperiencedUser = SuppressTutorials;
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().immediateUpgrades = InstantUpgrades;
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().suppressStory = !ReceiveMails;
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("SuppressTutorials", SuppressTutorials);
            data.SetValue("InstantUpgrades", InstantUpgrades);
            data.SetValue("ReceiveMails", ReceiveMails);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            SelectedGameMode = (int)SRSingleton<SceneContext>.Instance.GameModeConfig.gameModel.currGameMode;
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
        private int selectedGameMode;
        private bool suppressTutorials;
        private bool instantUpgrades;
        private bool receiveMails;

        private OrderedDictionary GameModes;

        public GameModeTweaksSettingsUI()
        {
            GameModes = new OrderedDictionary();
            GameModes.Add("Classic", PlayerState.GameMode.CLASSIC);
            GameModes.Add("Casual", PlayerState.GameMode.CASUAL);
        }

        public override string GetTabName()
        {
            return "Game Mode";
        }



        public override void OnGUI()
        {
            GUILayout.Label("Active game mode");

            GUILayout.BeginHorizontal();
            foreach (DictionaryEntry gameMode in GameModes)
            {
                int mode = (int)gameMode.Value;
                GUI.enabled = selectedGameMode != mode;
                if (GUILayout.Button((string)gameMode.Key))
                {
                    selectedGameMode = mode;
                }
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            suppressTutorials = GUILayout.Toggle(suppressTutorials, "Suppress tutorials (default: false)");
            instantUpgrades = GUILayout.Toggle(instantUpgrades, "Upgrades instantly available (default: false)");
            receiveMails = GUILayout.Toggle(receiveMails, "Receive mails (default: true)");
        }

        public override void Load()
        {
            selectedGameMode = GameModeTweaks.SelectedGameMode;
            suppressTutorials = GameModeTweaks.SuppressTutorials;
            instantUpgrades = GameModeTweaks.InstantUpgrades;
            receiveMails = GameModeTweaks.ReceiveMails;
        }

        public override void Save()
        {
            GameModeTweaks.SelectedGameMode = selectedGameMode;
            GameModeTweaks.SuppressTutorials = suppressTutorials;
            GameModeTweaks.InstantUpgrades = instantUpgrades;
            GameModeTweaks.ReceiveMails = receiveMails;
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
