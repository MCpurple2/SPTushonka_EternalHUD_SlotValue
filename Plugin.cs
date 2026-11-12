using System;
using BepInEx;
using EFT.UI;
using EternalHUD;
using HarmonyLib;

namespace EternalHudSlotValue
{
    [BepInPlugin(Id, "EternalHUD Slot Value", "1.0")]
    [BepInDependency("eft.hiddenhiragi.eternalhud")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private const string Id = "spt.eternalhud.slotvalue";
        private void Awake()
        {
            new Harmony(Id).CreateClassProcessor(typeof(TooltipPatch)).Patch();
            Logger.LogInfo("EternalHUD Slot Value loaded");
        }
    }

    [HarmonyPatch(typeof(SimpleTooltip), nameof(SimpleTooltip.SetText), typeof(string))]
    internal static class TooltipPatch
    {
        private static readonly PriceDisplay Display = new PriceDisplay();

        private static readonly InventoryValue Inventory = new InventoryValue();

        private static void Prefix(SimpleTooltip __instance, ref string __0)
        {
            var item = EternalHUD.InfoClass.selectItemObj;
            var line = MyMod._priceText;
            if (item == null || !ReferenceEquals(__instance, MyMod._cacheTooltip) ||
                string.IsNullOrEmpty(line) || string.IsNullOrEmpty(__0))
                return;

            int start = __0.IndexOf(line, StringComparison.Ordinal);
            if (start < 0)
                return;

            var size = item.CalculateCellSize();
            var total = Inventory.Read(item);
            __0 = Display.Insert(__0, line, start, size.X, size.Y, total, total.HasValue ? Inventory.Missing : 0);
        }
    }
}


