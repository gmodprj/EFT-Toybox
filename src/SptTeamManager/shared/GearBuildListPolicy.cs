using System;
using System.Collections.Generic;
using System.Linq;

namespace pitTeam.Shared
{
    // 장비 빌드 한 건이 목록에 설 때의 모습. 서버의 EquipmentBuild 를 SPT 타입 없이 옮긴 것이다 -
    // 판정이 서버 프로젝트 밖(테스트)에서 검증되어야 해서 모델 자체를 참조하지 않는다.
    // ItemCount 는 **루트를 뺀** 아이템 수가 아니라 빌드가 들고 있는 전체 수다. 화면에 "몇 점" 으로 보여 주려는
    // 값이 아니라 "비어 있지 않다" 를 가리는 값이므로 해석을 더하지 않는다.
    public sealed record GearBuildCandidate(string Id, string? Name, int ItemCount);

    // 목록에 실제로 싣는 한 줄.
    public sealed record GearBuildRow(string Id, string Name);

    // 두 묶음. **한 리스트에 섞어 담고 꼬리표로 가르지 않는다** - 꼬리표는 다음 사람이 떨어뜨릴 수 있지만
    // 자리가 둘이면 떨어뜨릴 수가 없다. 사용자가 요구한 "별도로 구분하여 표시" 를 자료 구조가 지킨다.
    public sealed record GearBuildList(IReadOnlyList<GearBuildRow> TeammateBuilds, IReadOnlyList<GearBuildRow> PlayerBuilds);

    // 두 프로필의 저장된 장비 빌드를 화면에 보일 목록으로 정리한다. 순수하고, 던지지 않는다.
    public static class GearBuildListPolicy
    {
        // 이름이 없는 빌드(EFT 에서 이름을 비워 저장할 수 있다)를 부를 말. 빈 줄로 두면 누를 수는 있는데
        // 무엇인지 알 수 없는 줄이 된다.
        public const string UnnamedBuild = "이름 없는 빌드";

        // 아이템이 없는 빌드는 뺀다: 입힐 것이 없으니 고를 수 있으면 안 된다.
        // id 가 없는 빌드도 뺀다: 고른 뒤 서버가 다시 찾을 열쇠가 없다.
        // 같은 프로필 안에서 id 가 겹치면 처음 것만 남긴다(뒤엣것은 같은 빌드의 중복 기록이다).
        // **두 프로필 사이의 id 겹침은 남겨 둔다** - 프로필을 복사해 만든 경우 같은 id 가 양쪽에 있을 수 있고,
        // 그 둘은 서로 다른 프로필의 서로 다른 빌드다. 묶음이 나뉘어 있으므로 섞일 염려가 없다.
        public static GearBuildList Build(
            IEnumerable<GearBuildCandidate>? teammateBuilds,
            IEnumerable<GearBuildCandidate>? playerBuilds)
        {
            return new GearBuildList(Rows(teammateBuilds), Rows(playerBuilds));
        }

        private static IReadOnlyList<GearBuildRow> Rows(IEnumerable<GearBuildCandidate>? candidates)
        {
            if (candidates == null) return Array.Empty<GearBuildRow>();

            var seen = new HashSet<string>(StringComparer.Ordinal);
            var rows = new List<GearBuildRow>();

            foreach (var candidate in candidates)
            {
                if (candidate == null) continue;

                var id = candidate.Id?.Trim();
                if (string.IsNullOrEmpty(id)) continue;
                if (candidate.ItemCount <= 0) continue;
                if (!seen.Add(id!)) continue;

                var name = string.IsNullOrWhiteSpace(candidate.Name) ? UnnamedBuild : candidate.Name!.Trim();
                rows.Add(new GearBuildRow(id!, name));
            }

            // 이름순. 저장 순서는 EFT 가 정하는 것이라 사용자가 예측할 수 없다.
            // 같은 이름이 여럿일 수 있으므로(이름은 열쇠가 아니다) id 로 한 번 더 갈라 순서를 고정한다 -
            // 그러지 않으면 목록을 다시 열 때마다 줄 순서가 바뀔 수 있다.
            return rows
                .OrderBy(row => row.Name, StringComparer.CurrentCulture)
                .ThenBy(row => row.Id, StringComparer.Ordinal)
                .ToList();
        }
    }
}
