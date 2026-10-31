using System;
using System.Collections.Generic;
using System.Linq;
using EFT;
using EFT.InventoryLogic;

namespace GogglesWheel
{
    /// <summary>헬멧에 달린 여닫이를 긁어 모으고, 고른 것을 실제로 여닫는다.</summary>
    public static class HeadwearScanner
    {
        /// <summary>
        /// 설계 §6-3 의 순서로 목록을 만든다. 헬멧 **자신도 포함**한다 —
        /// 게임의 GetItemComponentsInChildren 이 그렇게 돌고, 힌지 헬멧은 자기 자신이 여닫이다.
        /// </summary>
        public static IReadOnlyList<ToggleTarget> Scan(Player player)
        {
            var result = new List<ToggleTarget>();

            var helmet = GetHelmet(player);
            if (helmet == null)
            {
                return result;
            }

            try
            {
                foreach (var togglable in helmet.GetItemComponentsInChildren<TogglableComponent>())
                {
                    var item = togglable?.Item;
                    if (item == null)
                    {
                        continue;
                    }

                    var kind = Classify(item, helmet);
                    if (kind == ToggleKind.HelmetVisor && !Plugin.Settings.IncludeHelmetVisor)
                    {
                        continue;
                    }

                    result.Add(new ToggleTarget(item.Id, kind, NameOf(item, kind)));
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("헬멧을 훑다 멈췄다: " + e.Message);
                return new List<ToggleTarget>();
            }

            return result.OrderBy(SortKey).ToList();
        }

        /// <summary>고른 것을 여닫는다. 바닐라와 같은 네트워크 경로를 탄다.</summary>
        public static bool Toggle(Player player, string targetId)
        {
            if (player == null || string.IsNullOrEmpty(targetId))
            {
                return false;
            }

            try
            {
                var helmet = GetHelmet(player);
                if (helmet == null)
                {
                    return false;
                }

                var togglable = helmet.GetItemComponentsInChildren<TogglableComponent>()
                    .FirstOrDefault(c => c?.Item != null && c.Item.Id == targetId);

                if (togglable == null)
                {
                    return false;
                }

                player.InventoryController.TryRunNetworkTransaction(togglable.Set(!togglable.On, simulate: true));
                return true;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("여닫다 멈췄다: " + e.Message);
                return false;
            }
        }

        private static CompoundItem GetHelmet(Player player)
        {
            try
            {
                var slot = player?.InventoryController?.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear);
                return slot?.ContainedItem as CompoundItem;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static ToggleKind Classify(Item item, Item helmet)
        {
            if (item.GetItemComponent<NightVisionComponent>() != null)
            {
                return ToggleKind.NightVision;
            }

            if (item.GetItemComponent<ThermalVisionComponent>() != null)
            {
                return ToggleKind.Thermal;
            }

            if (item.GetItemComponent<FaceShieldComponent>() != null)
            {
                return ToggleKind.FaceShield;
            }

            if (helmet != null && item.Id == helmet.Id)
            {
                return ToggleKind.HelmetVisor;
            }

            return ToggleKind.Other;
        }

        private static int SortKey(ToggleTarget target)
        {
            switch (target.Kind)
            {
                case ToggleKind.NightVision: return 1;
                case ToggleKind.Thermal: return 2;
                case ToggleKind.FaceShield: return 3;
                case ToggleKind.HelmetVisor: return 4;
                default: return 5;
            }
        }

        private static string NameOf(Item item, ToggleKind kind)
        {
            try
            {
                var localized = item.LocalizedName();
                if (!string.IsNullOrEmpty(localized))
                {
                    return localized;
                }
            }
            catch (Exception)
            {
                // locale 이 아직 없으면 아래 이름을 쓴다.
            }

            switch (kind)
            {
                case ToggleKind.NightVision: return "야시경";
                case ToggleKind.Thermal: return "열화상";
                case ToggleKind.FaceShield: return "페이스실드";
                case ToggleKind.HelmetVisor: return "바이저";
                default: return "여닫이";
            }
        }
    }
}
