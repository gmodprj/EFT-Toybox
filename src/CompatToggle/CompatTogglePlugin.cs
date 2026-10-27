using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace CompatToggle
{
    [BepInPlugin(PluginGuid, "Compat Toggle", "0.1.0")]
    public class CompatTogglePlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "spt.compattoggle";

        private static ConfigEntry<bool> _master;
        private static ConfigEntry<bool> _weaponSlots;
        private static ConfigEntry<bool> _attachmentSlots;
        private static ConfigEntry<bool> _chamberAmmo;
        private static ConfigEntry<bool> _magazineCalibers;
        private static ConfigEntry<bool> _magazineAmmo;
        private static ConfigEntry<bool> _ignoreConflicts;
        private static ConfigEntry<bool> _logDecisions;

        /// <summary>
        /// 현재 F12 값으로 만든 묶음. 캐시하지 않는다 — 토글 변경이 다음 판정부터 반영되어야 한다.
        /// </summary>
        public static ToggleSet Toggles => new ToggleSet(
            _master != null && _master.Value,
            _weaponSlots != null && _weaponSlots.Value,
            _attachmentSlots != null && _attachmentSlots.Value,
            _chamberAmmo != null && _chamberAmmo.Value,
            _magazineCalibers != null && _magazineCalibers.Value,
            _magazineAmmo != null && _magazineAmmo.Value,
            _ignoreConflicts != null && _ignoreConflicts.Value);

        public static bool LogDecisions => _logDecisions != null && _logDecisions.Value;

        private static ManualLogSource _log;

        /// <summary>
        /// 거부될 판정을 허용으로 바꾼 건을 남긴다. 호출자는 바꾼 경우에만 부른다.
        /// CheckCompatibility 는 인벤토리 조작마다 여러 번 불리는 뜨거운 경로이므로,
        /// 문자열을 만들기 전에 빠져나간다.
        /// </summary>
        public static void LogForced(string kind, string slotId, string itemName)
        {
            if (_logDecisions == null || !_logDecisions.Value || _log == null)
            {
                return;
            }

            _log.LogInfo($"forced allow: {kind} slot='{slotId}' item='{itemName}'");
        }

        private void Awake()
        {
            _master = Config.Bind("General", "Master switch", true,
                "Turn every compatibility toggle below off at once. Off means vanilla rules.");

            _logDecisions = Config.Bind("General", "Log decisions", false,
                "Log every check this plugin turned from 'rejected' into 'allowed'. For finding out which combinations the vanilla rules were blocking.");

            _weaponSlots = Config.Bind("Compatibility", "Weapon slots", true,
                "Let any mod go into any attachment slot on a weapon body.");

            _attachmentSlots = Config.Bind("Compatibility", "Attachment slots", true,
                "Let any mod go into any sub-slot of another mod.");

            _chamberAmmo = Config.Bind("Compatibility", "Chamber ammo", true,
                "Let any round go into a chamber. With 'Magazine ammo' off you can chamber any round, but magazines still only take their own caliber.");

            _magazineCalibers = Config.Bind("Compatibility", "Magazine calibers", true,
                "Let any magazine go into any weapon. With 'Magazine ammo' on as well, this is the usual pairing: the best magazine you own fits any weapon.");

            _magazineAmmo = Config.Bind("Compatibility", "Magazine ammo", true,
                "Let any round go into any magazine. With 'Chamber ammo' on but 'Magazine calibers' off, only matching magazines still fit the weapon.");

            _ignoreConflicts = Config.Bind("Compatibility", "Ignore conflicts", true,
                "Ignore ConflictingItems between weapons and mods. Helmets and face gear are left alone, so mods that author those conflicts keep working.");

            _log = Logger;

            new Patches.SlotCompatibilityPatch().Enable();
            new Patches.ConflictPatch().Enable();
            new Patches.StackSlotCompatibilityPatch().Enable();

            Logger.LogInfo("Compat Toggle loaded.");
        }
    }
}
