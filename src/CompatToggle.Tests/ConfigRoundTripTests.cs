using System.Text.Json;
using WebConfigBridge;
using Xunit;

namespace CompatToggle.Tests
{
    /// <summary>
    /// 남의 설정 파일을 읽어 다시 쓸 때 우리가 모델로 만들지 않은 키가 살아남는지 본다.
    /// 이것이 깨지면 저장 한 번에 남의 설정이 통째로 날아간다 — 특히
    /// RealisticInsurance 는 "// 항목": "설명" 형태의 주석용 키를 최상위에 두고,
    /// RepairSkillExtension 은 우리가 다루지 않는 repairKit 트리를 들고 있다.
    ///
    /// 설치본의 실제 파일을 읽는다. 설치본이 없는 곳에서는 건너뛴다.
    /// </summary>
    public class ConfigRoundTripTests
    {
        private const string ModsRoot = @"E:\Spt\SPT_Runtime\user\mods";

        private static readonly JsonSerializerOptions ReadOptions = new()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };

        private static readonly JsonSerializerOptions WriteOptions = new() { WriteIndented = true };

        [Theory]
        [InlineData(@"RealisticInsurance\config\config.json", typeof(RealisticInsuranceConfig))]
        [InlineData(@"RepairSkillExtension\buffs.jsonc", typeof(RepairSkillExtensionConfig))]
        [InlineData(@"AllinWeaponUnslotted\config\config.jsonc", typeof(AllinWeaponUnslottedConfig))]
        [InlineData(@"HelmetSlotUnion\config.jsonc", typeof(HelmetSlotUnionConfig))]
        [InlineData(@"EquipmentIsEternal\config.jsonc", typeof(EquipmentIsEternalConfig))]
        [InlineData(@"mpstark-dynamicmaps\config.json", typeof(DynamicMapsConfig))]
        [InlineData(@"Moe-TradeMarker\config.json", typeof(TradeMarkerConfig))]
        [InlineData(@"UltimateFlea\config\mod.json", typeof(UltimateFleaModConfig))]
        public void Every_top_level_key_survives_a_round_trip(string relativePath, Type configType)
        {
            var path = Path.Combine(ModsRoot, relativePath);
            if (!File.Exists(path))
            {
                return; // 설치본이 없는 환경
            }

            var original = File.ReadAllText(path);
            var before = TopLevelKeys(original);

            var parsed = JsonSerializer.Deserialize(original, configType, ReadOptions);
            Assert.NotNull(parsed);

            var rewritten = JsonSerializer.Serialize(parsed, configType, WriteOptions);
            var after = TopLevelKeys(rewritten);

            var lost = before.Except(after).ToList();
            Assert.True(lost.Count == 0, $"{relativePath}: 사라진 최상위 키 — {string.Join(", ", lost)}");
        }

        private static HashSet<string> TopLevelKeys(string json)
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });

            return doc.RootElement.EnumerateObject().Select(p => p.Name).ToHashSet();
        }
    }
}
