using BepInEx;
using HarmonyLib;
using GameNetcodeStuff;
using UnityEngine.InputSystem;

namespace Walkie
{
    [BepInPlugin("rr.Walkie", "WalkieUse", "1.0.0")]
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
            for (int index = 0; index < __instance.ItemSlots.Length; ++index)
            {
                if (__instance.ItemSlots[index] is WalkieTalkie && __instance.ItemSlots[index].isBeingUsed)
                {
                    pocketWalkie = __instance.ItemSlots[index];
                    break;
                }
            }
            if (pocketWalkie == null) return;
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                if (__instance.currentlyHeldObjectServer is WalkieTalkie)
                {
                    __instance.currentlyHeldObjectServer.UseItemOnClient(true);
                } else if (pocketWalkie != null)
                {
                    pocketWalkie.UseItemOnClient(true);
                }
            }
            if (Keyboard.current.tKey.wasReleasedThisFrame)
            {
                if (__instance.currentlyHeldObjectServer is WalkieTalkie)
                {
                    __instance.currentlyHeldObjectServer.UseItemOnClient(false);
                } else if (pocketWalkie != null)
                {
                    pocketWalkie.UseItemOnClient(false);
                }
            }
        }
    }
}
