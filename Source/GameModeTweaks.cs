using System.Reflection;
using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;
using SRML;
using SRML.Console;
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
        public static uint PlayerDamageMultiplier = 100; // Default 100;

        public override void PreLoad()
        {
            Harmony harmony = HarmonyPatcher.GetInstance();

            MethodInfo methodOriginal = typeof(PlayerModel).GetMethod("Reset");
            MethodInfo methodNew = typeof(GameModeTweaks).GetMethod("PlayerModel_ResetPatch", BindingFlags.Static | BindingFlags.Public);
            Main.Log("Patching PlayerModel.Reset: " + methodOriginal + " > " + methodNew);
            harmony.Patch(methodOriginal, null, new HarmonyMethod(methodNew));

            methodOriginal = typeof(PlayerModel).GetMethod("ApplyUpgrade");
            methodNew = typeof(GameModeTweaks).GetMethod("PlayerModel_ApplyUpgradePatch", BindingFlags.Static | BindingFlags.Public);
            Main.Log("Patching PlayerModel.ApplyUpgrade: " + methodOriginal + " > " + methodNew);
            harmony.Patch(methodOriginal, new HarmonyMethod(methodNew));

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
            SRSingleton<SceneContext>.Instance.GameModeConfig.GetModeSettings().playerDamageMultiplier = PlayerDamageMultiplier / 100.0f;
        }

        public override void SaveSettings(CompoundDataPiece data)
        {
            data.SetValue("AllowTarrSpawns", AllowTarrSpawns);
            data.SetValue("SuppressTutorials", SuppressTutorials);
            data.SetValue("InstantUpgrades", InstantUpgrades);
            data.SetValue("ReceiveMails", ReceiveMails);
            data.SetValue("PlayerDamageMultiplier", PlayerDamageMultiplier);
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            AllowTarrSpawns = Main.GetSaveValue<bool>(data, "AllowTarrSpawns", true);
            SuppressTutorials = Main.GetSaveValue<bool>(data, "SuppressTutorials", false);
            InstantUpgrades = Main.GetSaveValue<bool>(data, "InstantUpgrades", false);
            ReceiveMails = Main.GetSaveValue<bool>(data, "ReceiveMails", true);
            PlayerDamageMultiplier = Main.GetSaveValue<uint>(data, "PlayerDamageMultiplier", 100);
        }

        private ITweakSettingsUI SettingsUI = new GameModeTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }

        public static readonly int[] DEFAULT_MAX_AMMO = new int[5]
        {
            20,
            30,
            40,
            50,
            100
        };

        public static void PlayerModel_ResetPatch(PlayerModel __instance, GameModeSettings modeSettings)
        {
            Main.Log("Set ammo 0: " + DEFAULT_MAX_AMMO[0]);
            __instance.maxAmmo = DEFAULT_MAX_AMMO[0];
        }

        public static bool PlayerModel_ApplyUpgradePatch(PlayerModel __instance, PlayerState.Upgrade upgrade, bool isFirstTime)
        {
            switch (upgrade)
            {
                case PlayerState.Upgrade.AMMO_1:
                {
                    Main.Log("Set ammo 1: " + DEFAULT_MAX_AMMO[1]);
                    __instance.maxAmmo = DEFAULT_MAX_AMMO[1];
                    return false;
                }
                case PlayerState.Upgrade.AMMO_2:
                {
                    Main.Log("Set ammo 2: " + DEFAULT_MAX_AMMO[2]);
                    __instance.maxAmmo = DEFAULT_MAX_AMMO[2];
                    return false;
                }
                case PlayerState.Upgrade.AMMO_3:
                {
                    Main.Log("Set ammo 3: " + DEFAULT_MAX_AMMO[3]);
                    __instance.maxAmmo = DEFAULT_MAX_AMMO[3];
                    return false;
                }
                case PlayerState.Upgrade.AMMO_4:
                {
                    Main.Log("Set ammo 5: " + DEFAULT_MAX_AMMO[4]);
                    __instance.maxAmmo = DEFAULT_MAX_AMMO[4];
                    return false;
                }
            }

            return true;
        }
    }

    public class GameModeTweaksSettingsUI : ITweakSettingsUI
    {
        private bool allowTarrSpawns;
        private bool suppressTutorials;
        private bool instantUpgrades;
        private bool receiveMails;
        private string playerDamageMultiplier;

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

            GUILayout.Label("Player damage multiplier (default: 100)");
            string newValue = GUILayout.TextField(playerDamageMultiplier, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            if (newValue != playerDamageMultiplier)
            {
                if (uint.TryParse(newValue, out uint dummy))
                {
                    playerDamageMultiplier = newValue;
                }
            }
        }

        public override void Load()
        {
            allowTarrSpawns = GameModeTweaks.AllowTarrSpawns;
            suppressTutorials = GameModeTweaks.SuppressTutorials;
            instantUpgrades = GameModeTweaks.InstantUpgrades;
            receiveMails = GameModeTweaks.ReceiveMails;
            playerDamageMultiplier = GameModeTweaks.PlayerDamageMultiplier.ToString();
        }

        public override void Save()
        {
            GameModeTweaks.AllowTarrSpawns = allowTarrSpawns;
            GameModeTweaks.SuppressTutorials = suppressTutorials;
            GameModeTweaks.InstantUpgrades = instantUpgrades;
            GameModeTweaks.ReceiveMails = receiveMails;

            if (uint.TryParse(playerDamageMultiplier, out uint newValue))
            {
                GameModeTweaks.PlayerDamageMultiplier = newValue;
            }
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
            if (args == null || args.Length < 1) {
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

    public class SetPlayerDamageMultiplierCommand : ConsoleCommand
    {
        public override string Usage => "playerdamage [percentage]";
        public override string ID => "playerdamage";
        public override string Description => "gets or sets the player damage multiplier (percentage)";

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
            {
                Main.Log("Player damage: " + GameModeTweaks.PlayerDamageMultiplier + "% (default: 100)");
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

            GameModeTweaks.PlayerDamageMultiplier = (uint)newValue;
            Main.ApplySettings();
            return true;
        }
    }
}
