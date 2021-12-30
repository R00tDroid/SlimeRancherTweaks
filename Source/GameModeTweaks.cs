using SRML.Console;
using SRML.SR.SaveSystem.Data;
using UnityEngine;

namespace SRTweaks
{
    public class GameModeTweaks : ITweak<GameModeTweaks>
    {
        public static bool AllowTarrSpawns = true; // Default true;
        public static bool SuppressTutorials = false; // Default false;

        public override void PreLoad()
        {
            SRML.Console.Console.RegisterCommand(new SetTarrSpawnCommand());
            SRML.Console.Console.RegisterCommand(new SetSuppressTutorialsCommand());
        }

        public override void GameLoaded()
        {
        }

        public override void ApplySettings()
        {
            // Set Gamemode spawning
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().preventHostiles = AllowTarrSpawns;
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().assumeExperiencedUser = SuppressTutorials;
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("AllowTarrSpawns", AllowTarrSpawns);
            data.SetValue("SuppressTutorials", SuppressTutorials);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            AllowTarrSpawns = Main.GetSaveValue<bool>(data, "AllowTarrSpawns", true);
            SuppressTutorials = Main.GetSaveValue<bool>(data, "SuppressTutorials", false);
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

        public override void OnGUI()
        {
            GUILayout.Label("GameMode");
            GUILayout.Space(2);

            allowTarrSpawns = GUILayout.Toggle(allowTarrSpawns, "Allow Tarr to spawn (default: true)");
        }

        public override void Load()
        {
            allowTarrSpawns = GameModeTweaks.AllowTarrSpawns;
        }

        public override void Save()
        {
            GameModeTweaks.AllowTarrSpawns = allowTarrSpawns;
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
}
