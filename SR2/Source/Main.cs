using System.Diagnostics;
using System.Reflection;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using SRTweaks;

namespace SRTweaks
{
    [BepInPlugin("nl.R00tDroid.SRTweaks", "Slime Rancher Tweaks", "0.0.0")]
    public class Main : BasePlugin
    {
        private void Awake()
        {
        }

        public override void Load()
        {
            Log.LogInfo("Plugin loaded");
        }
    }
}
