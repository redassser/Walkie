using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using UnityEngine;
using System.IO;
using BepInEx.Logging;

namespace Walkie
{
    [BepInPlugin("rr.Walkie", "WalkieUse", "1.3.0")]
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class WalkieToggle : BaseUnityPlugin
    {
        static string path = Application.persistentDataPath + "/walkiebutton.txt";
        internal static ManualLogSource logSource;
        static InputActionAsset asset;
        static string defaultkey = "/Keyboard/r";

        private Harmony _harmony = new Harmony("Walkie");
        private void Awake()
        {
            this._harmony.PatchAll(typeof(WalkieToggle));
            this.Logger.LogInfo("------Walkie done.------");
            WalkieToggle.logSource = base.Logger;
        }
        public static void setAsset(string thing)
        {
            asset = InputActionAsset.FromJson(@"
                {
                    ""maps"" : [
                        {
                            ""name"" : ""Walkie"",
                            ""actions"": [
                                {""name"": ""togglew"", ""type"" : ""button""}
                            ],
                            ""bindings"" : [
                                {""path"" : """ + thing + @""", ""action"": ""togglew""}
                            ]
                        }
                    ]
                }");
        }
        [HarmonyPatch(typeof(IngamePlayerSettings), "CompleteRebind")]
        [HarmonyPrefix]
        public static void SavingToFile(IngamePlayerSettings __instance)
        {
            if (__instance.rebindingOperation.action.name != "togglew") return;
            File.WriteAllText(path, __instance.rebindingOperation.action.controls[0].path);
            string thing = defaultkey;
            if (File.Exists(path))
            {
                thing = File.ReadAllText(path);
            }
            setAsset(thing);
        }

        [HarmonyPatch(typeof(KepRemapPanel), "LoadKeybindsUI")]
        [HarmonyPrefix]
        public static void Testing(KepRemapPanel __instance)
        {
            string thing = defaultkey;
            if (!File.Exists(path))
            {
                File.WriteAllText(path, defaultkey);
            } else
            {
                thing = File.ReadAllText(path);
            }

            for (int index1 = 0; index1 < __instance.remappableKeys.Count; ++index1)
            {
                if (__instance.remappableKeys[index1].ControlName == "Walkie") return;
            }
            RemappableKey fl = new RemappableKey();
            setAsset(thing);
            InputActionReference inp = InputActionReference.Create(asset.FindAction("Walkie/togglew"));
            fl.ControlName = "Walkie";
            fl.currentInput = inp;

            __instance.remappableKeys.Add(fl);
        }
        [HarmonyPatch(typeof(PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void ReadInput(PlayerControllerB __instance)
        {
            GrabbableObject pocketWalkie = null;
            if ((!__instance.IsOwner || !__instance.isPlayerControlled || __instance.IsServer && !__instance.isHostPlayerObject) && !__instance.isTestingPlayer)
                return;
            if (__instance.inTerminalMenu || __instance.isTypingChat) return;
            if (ShipBuildModeManager.Instance.InBuildMode) return;
            if (!Application.isFocused) return;
            for (int index = 0; index < __instance.ItemSlots.Length; ++index)
            {
                if (__instance.ItemSlots[index] is WalkieTalkie && __instance.ItemSlots[index].isBeingUsed)
                {
                    pocketWalkie = __instance.ItemSlots[index];
                    break;
                }
            }
            if (pocketWalkie == null) return;
            string thing = defaultkey;
            if (!File.Exists(path))
            {
                File.WriteAllText(path, defaultkey);
            } else
            {
                thing = File.ReadAllText(path);
            }
            if (!asset || !asset.enabled) { setAsset(thing); asset.Enable(); }
            if (asset.FindAction("Walkie/togglew").WasPressedThisFrame())
            {
                try
                {
                    if (__instance.currentlyHeldObjectServer is WalkieTalkie)
                    {
                        __instance.currentlyHeldObjectServer.UseItemOnClient(true);
                    } else if (pocketWalkie != null)
                    {
                        pocketWalkie.UseItemOnClient(true);
                    }
                } catch { }
            }
            if (asset.FindAction("Walkie/togglew").WasReleasedThisFrame())
            {
                try
                {
                    if (__instance.currentlyHeldObjectServer is WalkieTalkie)
                    {
                        __instance.currentlyHeldObjectServer.UseItemOnClient(false);
                    } else if (pocketWalkie != null)
                    {
                        pocketWalkie.UseItemOnClient(false);
                    }
                } catch { }
            }
        }
    }
}
