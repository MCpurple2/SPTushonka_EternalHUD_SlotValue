using System;
using System.Collections.Generic;
using EFT.InventoryLogic;
using EternalHUD;
using UnityEngine;

namespace EternalHudSlotValue
{
    internal sealed class InventoryValue
    {
        private List<EternalHUD.InfoClass.NameInfo> catalog;
        private readonly Dictionary<string, EternalHUD.InfoClass.NameInfo> prices =
            new Dictionary<string, EternalHUD.InfoClass.NameInfo>(StringComparer.OrdinalIgnoreCase);
        private int indexedCount;
        private Item lastRoot;
        private float nextRefresh;
        private decimal? total;
        internal int Missing { get; private set; }

        internal decimal? Read(Item root)
        {
            if (!Supports(root))
                return null;

            var rows = ShowMethodPatch.NameList;
            bool changed = !ReferenceEquals(catalog, rows) || indexedCount != (rows?.Count ?? 0);
            float now = Time.unscaledTime;
            if (!changed && ReferenceEquals(root, lastRoot) && now < nextRefresh)
                return total;

            if (changed)
            {
                prices.Clear();
                catalog = rows;
                indexedCount = rows?.Count ?? 0;
                if (rows != null)
                    foreach (var row in rows)
                        if (!string.IsNullOrEmpty(row?.StringName) && !prices.ContainsKey(row.StringName))
                            prices.Add(row.StringName, row);
            }

            decimal sum = 0;
            int missing = 0;
            var seen = new HashSet<Item>();
            var pending = new Stack<Item>();
            pending.Push(root);
            while (pending.Count > 0)
            {
                var item = pending.Pop();
                if (item == null || !seen.Add(item))
                    continue;

                // A trader offer's root count is stock. Price one item and its contents.
                int quantity = ReferenceEquals(item, root) ? 1 : item.StackObjectsCount;
                if (quantity > 0)
                {
                    if (prices.TryGetValue(item.StringTemplateId, out var row) && row.SellPrice > 0)
                        sum += (decimal)row.SellPrice * quantity;
                    else
                        missing++;
                }

                // This also visits ammo-box stacks, which are outside CompoundItem.
                if (item is GClass3248 parent)
                    foreach (var container in parent.Containers)
                        foreach (var child in container.Items)
                        {
                            if (container is Slot slot && slot.Locked && child is ArmorPlateItemClass)
                                continue;
                            pending.Push(child);
                        }
            }
            lastRoot = root;
            nextRefresh = now + 0.25f;
            Missing = missing;
            total = sum;
            return total;
        }

        private static bool Supports(Item item)
        {
            return item is Weapon || item is ArmorItemClass || item is HeadwearItemClass ||
                item is SearchableItemItemClass || item is SimpleContainerItemClass ||
                item is LockableContainerItemClass || item is RandomLootContainerItemClass ||
                item is MagazineItemClass || item is AmmoBox ||
                (item is CompoundItem compound && compound.Grids != null && compound.Grids.Length > 0);
        }
    }
}

