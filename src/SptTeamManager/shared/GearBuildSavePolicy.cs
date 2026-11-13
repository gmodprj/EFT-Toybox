using System;
using System.Collections.Generic;

namespace pitTeam.Shared
{
    // 장비 꾸러미로 담기의 판정. Accepted 가 거짓이면 **아무것도 쓰지 않는다** — Reason 이 왜인지 말한다.
    public sealed record GearBuildSavePlan(bool Accepted, string Reason, string Name);

    /// <summary>
    /// 편집기의 "꾸러미로 저장" 판정(순수). SPT 타입도 프로필 접근도 없다 — 실제 쓰기는 서버가 한다.
    ///
    /// 이 길은 **아이템을 옮기지 않는다.** 빌드는 프로필의 `userbuilds.equipmentBuilds` 에 들어가는
    /// **본뜬 것**이라, 스태시 전송과 달리 총량 보존을 걱정할 자리가 없다. 그래서 여기서 지키는 것은
    /// 두 가지뿐이다 — **빈 것을 담지 않는 것**과 **이름이 겹치지 않는 것**.
    /// </summary>
    public static class GearBuildSavePolicy
    {
        /// <summary>한 프로필이 가질 수 있는 빌드 수. 게임의 목록이 길어지면 고르기 어렵다.</summary>
        public const int MaxBuilds = 100;

        /// <summary>이름 길이 상한. 게임 목록이 한 줄로 그리므로 길면 잘린다.</summary>
        public const int MaxNameLength = 50;

        /// <summary>
        /// 담아도 되는지 정하고, 담을 **이름**을 짓는다.
        /// </summary>
        /// <param name="nickname">그 팀원의 이름. 비면 "팀원" 으로 적는다.</param>
        /// <param name="equippedCount">지금 입고 있는 것의 수(장비 뿌리는 세지 않는다).</param>
        /// <param name="existingNames">그 프로필에 이미 있는 빌드 이름들.</param>
        /// <param name="now">이름에 붙일 시각(그 자리 시각).</param>
        public static GearBuildSavePlan Decide(
            string? nickname,
            int equippedCount,
            IReadOnlyCollection<string>? existingNames,
            DateTime now)
        {
            if (equippedCount <= 0)
            {
                return Refused("입고 있는 것이 없어 꾸러미로 담지 않는다.");
            }

            var names = existingNames ?? [];
            if (names.Count >= MaxBuilds)
            {
                return Refused($"저장된 꾸러미가 이미 {names.Count}개다({MaxBuilds}개까지). 쓰지 않는 것을 지운 뒤 다시 담는다.");
            }

            var who = string.IsNullOrWhiteSpace(nickname) ? "팀원" : nickname!.Trim();
            var stamp = now.ToString("MM-dd HH:mm");
            var baseName = Clip($"{who} {stamp}");

            // 같은 분에 두 번 담을 수 있다. 조용히 덮지 않고 **뒤에 번호를 붙인다**.
            var taken = new HashSet<string>(names, StringComparer.Ordinal);
            var name = baseName;
            for (var n = 2; taken.Contains(name); n++)
            {
                name = Clip($"{baseName} ({n})");
                if (n > MaxBuilds)
                {
                    return Refused("이름을 지을 수 없다(같은 이름이 너무 많다).");
                }
            }

            return new GearBuildSavePlan(true, $"꾸러미로 담는다 — 입고 있는 것 {equippedCount}개.", name);
        }

        private static string Clip(string text) =>
            text.Length <= MaxNameLength ? text : text[..MaxNameLength].TrimEnd();

        private static GearBuildSavePlan Refused(string reason) => new(false, reason, string.Empty);
    }
}
