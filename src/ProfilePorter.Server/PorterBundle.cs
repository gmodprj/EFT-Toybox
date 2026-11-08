using System.Collections.Generic;

namespace ProfilePorter.Server;

/// <summary>
/// 내보낸 아이템 하나. 프로필의 `Item` 에서 **판정에 필요한 것만** 옮겨 담는다.
///
/// SPT 타입을 들이지 않는다 — 이 파일과 <see cref="ExportSelection"/>·<see cref="ImportPlanner"/>
/// 는 게임 없이 시험할 수 있는 전부이고, 테스트 프로젝트가 소스를 그대로 링크한다
/// (STM 의 `shared/` 와 같은 방식).
///
/// <see cref="Upd"/> 는 **들여다보지 않는다.** 설계 §4.1 이 "가진 것 그대로" 를 위해
/// `upd` 를 통째로 싣기로 했고, §9.2 에서 20종을 세어 **그 프로필에만 뜻이 있는 필드가 없음**을
/// 확인했다. 그래서 판정은 이것을 불투명한 짐으로 들고 다니기만 한다.
///
/// <see cref="Location"/> 도 같은 **불투명한 짐**이다. 격자에 놓인 것은 `{x, y, r}` 이고 슬롯에
/// 꽂힌 것은 정수 색인이라 SPT 가 `object` 로 둔 자리인데, 2026-11-09 까지 이 기록에 **아예
/// 없어서** 내보낼 때 좌표와 회전이 전부 떨어졌다(설계 §20). 해석하지 않고 그대로 들고 다닌다 —
/// 실측으로 `rotation` 처럼 뜻을 모르는 필드가 섞여 있다(2,892개 중 27개).
/// </summary>
public sealed record PorterItem(
    string Id,
    string? ParentId,
    string? SlotId,
    string TemplateId,
    object? Upd = null,
    object? Location = null);

/// <summary>내보낸 파일 하나의 알맹이. 파일 형식은 설계 §4.1.</summary>
public sealed record PorterBundle(
    string Kind,
    IReadOnlyList<string> RootIds,
    IReadOnlyList<PorterItem> Items)
{
    /// <summary>스태시 바닥에 놓인 것들.</summary>
    public const string KindStash = "stash";

    /// <summary>입고 있던 한 벌.</summary>
    public const string KindLoadout = "loadout";
}
