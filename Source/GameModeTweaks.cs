using System;
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
        public static uint[] PlayerInventoryLevels = new uint[5]
        {
            20,
            30,
            40,
            50,
            100
        }; // Default 20, 30, 40, 50, 100;

        public static uint[] PlayerHealthLevels = new uint[5]
        {
            100,
            150,
            200,
            250,
            350
        }; // Default 100, 150, 200, 250, 350;

        public static uint[] PlayerEnergyLevels = new uint[4]
        {
            100,
            150,
            200,
            250
        }; // Default 100, 150, 200, 250;

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

            for (int i = 0; i < PlayerInventoryLevels.Length ; i++)
            {
                data.SetValue("PlayerInventoryLevels" + i, PlayerInventoryLevels[i]);
            }
        }

        public override void LoadSettings(CompoundDataPiece data)
        {
            AllowTarrSpawns = Main.GetSaveValue<bool>(data, "AllowTarrSpawns", true);
            SuppressTutorials = Main.GetSaveValue<bool>(data, "SuppressTutorials", false);
            InstantUpgrades = Main.GetSaveValue<bool>(data, "InstantUpgrades", false);
            ReceiveMails = Main.GetSaveValue<bool>(data, "ReceiveMails", true);
            PlayerDamageMultiplier = Main.GetSaveValue<uint>(data, "PlayerDamageMultiplier", 100);

            for (int i = 0; i < PlayerInventoryLevels.Length; i++)
            {
                PlayerInventoryLevels[i] = Main.GetSaveValue<uint>(data, "PlayerInventoryLevels" + i, (uint)PlayerModel.DEFAULT_MAX_AMMO[i]);
            }
        }

        private ITweakSettingsUI SettingsUI = new GameModeTweaksSettingsUI();
        public override ITweakSettingsUI GetSettingsUI()
        {
            return SettingsUI;
        }

        public static void PlayerModel_ResetPatch(PlayerModel __instance, GameModeSettings modeSettings)
        {
            __instance.maxAmmo = (int)PlayerInventoryLevels[0];
            __instance.maxHealth = (int)PlayerHealthLevels[0];
            __instance.maxEnergy = (int)PlayerEnergyLevels[0];
        }

        public static bool PlayerModel_ApplyUpgradePatch(PlayerModel __instance, PlayerState.Upgrade upgrade, bool isFirstTime)
        {
            switch (upgrade)
            {
                case PlayerState.Upgrade.AMMO_1:
                {
                    __instance.maxAmmo = (int)PlayerInventoryLevels[1];
                    return false;
                }
                case PlayerState.Upgrade.AMMO_2:
                {
                    __instance.maxAmmo = (int)PlayerInventoryLevels[2];
                    return false;
                }
                case PlayerState.Upgrade.AMMO_3:
                {
                    __instance.maxAmmo = (int)PlayerInventoryLevels[3];
                    return false;
                }
                case PlayerState.Upgrade.AMMO_4:
                {
                    __instance.maxAmmo = (int)PlayerInventoryLevels[4];
                    return false;
                }

                case PlayerState.Upgrade.HEALTH_1:
                {
                    __instance.maxHealth = Math.Max(__instance.maxHealth, (int)PlayerHealthLevels[1]);
                    if ((double) __instance.currHealth >= (double) __instance.maxHealth)
                        return false;
                    __instance.healthBurstAfter = Math.Min(__instance.healthBurstAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;
                }
                case PlayerState.Upgrade.HEALTH_2:
                {
                    __instance.maxHealth = Math.Max(__instance.maxHealth, (int)PlayerHealthLevels[2]);
                    if ((double) __instance.currHealth >= (double) __instance.maxHealth)
                        return false;;
                    __instance.healthBurstAfter = Math.Min(__instance.healthBurstAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;;
                }
                case PlayerState.Upgrade.HEALTH_3:
                {
                    __instance.maxHealth = Math.Max(__instance.maxHealth, (int)PlayerHealthLevels[3]);
                    if ((double) __instance.currHealth >= (double) __instance.maxHealth)
                        return false;;
                    __instance.healthBurstAfter = Math.Min(__instance.healthBurstAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;;
                }
                case PlayerState.Upgrade.HEALTH_4:
                {
                    __instance.maxHealth = Math.Max(__instance.maxHealth, (int)PlayerHealthLevels[4]);
                    if ((double)__instance.currHealth >= (double)__instance.maxHealth)
                        return false;;
                    __instance.healthBurstAfter = Math.Min(__instance.healthBurstAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;;
                }

                case PlayerState.Upgrade.ENERGY_1:
                {
                    __instance.maxEnergy = Math.Max(__instance.maxEnergy, (int)PlayerEnergyLevels[1]);
                    if ((double) __instance.currEnergy >= (double) __instance.maxEnergy)
                        return false;;
                    __instance.energyRecoverAfter = Math.Min(__instance.energyRecoverAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;;
                }
                case PlayerState.Upgrade.ENERGY_2:
                {
                    __instance.maxEnergy = Math.Max(__instance.maxEnergy, (int)PlayerEnergyLevels[2]);
                    if ((double) __instance.currEnergy >= (double) __instance.maxEnergy)
                        return false;;
                    __instance.energyRecoverAfter = Math.Min(__instance.energyRecoverAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;;
                }
                case PlayerState.Upgrade.ENERGY_3:
                {
                    __instance.maxEnergy = Math.Max(__instance.maxEnergy, (int)PlayerEnergyLevels[3]);
                    if ((double) __instance.currEnergy >= (double) __instance.maxEnergy)
                        return false;;
                    __instance.energyRecoverAfter = Math.Min(__instance.energyRecoverAfter,
                        __instance.worldModel.worldTime + 300.0);
                    return false;;
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
        private NumberField<uint> playerDamageMultiplier = new NumberField<uint>();

        private NumberField<uint>[] playerInventoryLevels;

        public GameModeTweaksSettingsUI()
        {
            playerInventoryLevels = new NumberField<uint>[GameModeTweaks.PlayerInventoryLevels.Length];
            for (int i = 0; i < GameModeTweaks.PlayerInventoryLevels.Length; i++)
            {
                playerInventoryLevels[i] = new NumberField<uint>();
            }
        }

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
            playerDamageMultiplier.ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });

            GUILayout.Label("Player inventory levels (default: 20, 30, 40, 50, 100)");
            GUILayout.BeginHorizontal();
            for (int i = 0; i < GameModeTweaks.PlayerInventoryLevels.Length; i++)
            { 
                playerInventoryLevels[i].ShowGUI(new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
            }
            GUILayout.EndHorizontal();
        }

        public override void Load()
        {
            allowTarrSpawns = GameModeTweaks.AllowTarrSpawns;
            suppressTutorials = GameModeTweaks.SuppressTutorials;
            instantUpgrades = GameModeTweaks.InstantUpgrades;
            receiveMails = GameModeTweaks.ReceiveMails;
            playerDamageMultiplier.Load(GameModeTweaks.PlayerDamageMultiplier);

            for (int i = 0; i < GameModeTweaks.PlayerInventoryLevels.Length; i++)
            {
                playerInventoryLevels[i].Load(GameModeTweaks.PlayerInventoryLevels[i]);
            }
        }

        public override void Save()
        {
            GameModeTweaks.AllowTarrSpawns = allowTarrSpawns;
            GameModeTweaks.SuppressTutorials = suppressTutorials;
            GameModeTweaks.InstantUpgrades = instantUpgrades;
            GameModeTweaks.ReceiveMails = receiveMails;
             playerDamageMultiplier.Save(ref GameModeTweaks.PlayerDamageMultiplier);

            for (int i = 0; i < GameModeTweaks.PlayerInventoryLevels.Length; i++)
            { 
                playerInventoryLevels[i].Save(ref GameModeTweaks.PlayerInventoryLevels[i]);
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
