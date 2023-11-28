using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;
using UnityEngine;

namespace Walkie
{
    [BepInPlugin("rr.Walkie", "WalkieUse", "1.2.3")]
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class WalkieToggle : BaseUnityPlugin
    {
        private Harmony _harmony = new Harmony("Flashlight");
        private void Awake()
        {
            this._harmony.PatchAll(typeof(WalkieToggle));
            this.Logger.LogInfo("------Walkie done.------");
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
            if (Keyboard.current.rKey.wasPressedThisFrame)
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
            if (Keyboard.current.rKey.wasReleasedThisFrame)
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
