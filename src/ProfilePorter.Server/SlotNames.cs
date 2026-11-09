using System.Collections.Generic;

namespace ProfilePorter.Server;

/// <summary>
/// 슬롯 id 를 한국어로 옮긴다(설계 §4.3, 사용자 결정 — **안쪽까지 전부 한국어**).
///
/// **locale DB 에서 끌어오지 않는다.** 아이템 이름과 달리 슬롯 이름은 locale 에 없다.
/// 그래서 여기 한 장짜리 표로 둔다.
///
/// **모르는 것은 받은 그대로 돌려준다.** 글자를 지어내지 않고 빈 칸으로도 만들지 않는다 —
/// 빈 칸이 되면 화면에서 그 자리가 사라진 것처럼 보인다. 모드가 새 슬롯을 더할 수 있으므로
/// 모르는 것이 있는 쪽이 정상이고, 부르는 쪽이 로그로 남겨 사전에 더할 거리를 쌓는다.
/// </summary>
public static class SlotNames
{
    private static readonly Dictionary<string, string> Korean_ = new(StringComparer.Ordinal)
    {
        // 장비 — 인게임 장비 화면의 칸들(설계 §4.3)
        ["Headwear"] = "모자",
        ["Earpiece"] = "헤드셋",
        ["FaceCover"] = "얼굴",
        ["Eyewear"] = "안경",
        ["ArmorVest"] = "방탄복",
        ["TacticalVest"] = "리그",
        ["Backpack"] = "가방",
        ["Pockets"] = "주머니",
        ["ArmBand"] = "완장",
        ["Dogtag"] = "인식표",
        ["FirstPrimaryWeapon"] = "주무기 1",
        ["SecondPrimaryWeapon"] = "주무기 2",
        ["Holster"] = "권총집",
        ["Scabbard"] = "칼집",
        ["SecuredContainer"] = "보안 컨테이너",

        // 주머니 칸
        ["pocket1"] = "주머니 1",
        ["pocket2"] = "주머니 2",
        ["pocket3"] = "주머니 3",
        ["pocket4"] = "주머니 4",
        ["SpecialSlot1"] = "특수 칸 1",
        ["SpecialSlot2"] = "특수 칸 2",
        ["SpecialSlot3"] = "특수 칸 3",

        // 방어 — 판과 소프트아머
        ["Front_plate"] = "전면 플레이트",
        ["Back_plate"] = "후면 플레이트",
        ["Left_side_plate"] = "좌측 플레이트",
        ["Right_side_plate"] = "우측 플레이트",
        ["Soft_armor_front"] = "전면 소프트아머",
        ["Soft_armor_back"] = "후면 소프트아머",
        ["Soft_armor_left"] = "좌측 소프트아머",
        ["soft_armor_right"] = "우측 소프트아머",
        ["Collar"] = "목 보호대",
        ["Groin"] = "사타구니 보호대",
        ["Groin_back"] = "사타구니 보호대(후면)",
        ["Shoulder_l"] = "좌측 어깨",
        ["Shoulder_r"] = "우측 어깨",
        ["Helmet_top"] = "헬멧 상단",
        ["Helmet_back"] = "헬멧 후면",
        ["Helmet_ears"] = "헬멧 귀",
        ["Helmet_eyes"] = "헬멧 눈",
        ["Helmet_jaw"] = "헬멧 턱",

        // 총 부착물
        ["mod_magazine"] = "탄창",
        ["mod_muzzle"] = "총구",
        ["mod_muzzle_000"] = "총구 1",
        ["mod_muzzle_001"] = "총구 2",
        ["mod_sight_front"] = "전방 조준기",
        ["mod_sight_rear"] = "후방 조준기",
        ["mod_scope"] = "조준경",
        ["mod_tactical"] = "전술 장비",
        ["mod_tactical_000"] = "전술 장비 1",
        ["mod_tactical_001"] = "전술 장비 2",
        ["mod_tactical_002"] = "전술 장비 3",
        ["mod_tactical_003"] = "전술 장비 4",
        ["mod_stock"] = "개머리판",
        ["mod_stock_000"] = "개머리판 1",
        ["mod_stock_001"] = "개머리판 2",
        ["mod_stock_akms"] = "개머리판(AKMS)",
        ["mod_handguard"] = "핸드가드",
        ["mod_barrel"] = "총열",
        ["mod_mount"] = "마운트",
        ["mod_mount_000"] = "마운트 1",
        ["mod_mount_001"] = "마운트 2",
        ["mod_mount_002"] = "마운트 3",
        ["mod_pistol_grip"] = "권총 손잡이",
        ["mod_pistolgrip"] = "권총 손잡이",
        ["mod_foregrip"] = "전방 손잡이",
        ["mod_bipod"] = "양각대",
        ["mod_charge"] = "장전 손잡이",
        ["mod_gas_block"] = "가스 블록",
        ["mod_reciever"] = "리시버",
        ["mod_launcher"] = "유탄 발사기",
        ["mod_flashlight"] = "손전등",
        ["mod_equipment"] = "장비",
        ["mod_equipment_000"] = "장비 1",
        ["mod_equipment_001"] = "장비 2",
        ["mod_equipment_002"] = "장비 3",
        ["mod_nvg"] = "야시경",
        ["mod_hammer"] = "공이",
        ["mod_catch"] = "탄창 멈치",
        ["mod_trigger"] = "방아쇠",
        ["patron_in_weapon"] = "약실",
        ["patron_in_weapon_000"] = "약실 1",
        ["patron_in_weapon_001"] = "약실 2",
        ["cartridges"] = "탄약",
        ["camora_000"] = "실린더 1",
        ["camora_001"] = "실린더 2",
        ["camora_002"] = "실린더 3",
        ["camora_003"] = "실린더 4",
        ["camora_004"] = "실린더 5",

        // 격자 이름 — 상자 안의 칸은 보통 `main` 이다
        ["main"] = "안쪽",
        ["hideout"] = "스태시",
    };

    /// <summary>한국어 이름. **모르면 받은 그대로** 돌려준다. 빈 것은 빈 글자.</summary>
    public static string Korean(string? slotId)
    {
        if (string.IsNullOrEmpty(slotId))
        {
            return string.Empty;
        }

        return Korean_.TryGetValue(slotId, out var name) ? name : slotId;
    }

    /// <summary>사전이 아는 이름인가. 부르는 쪽이 모르는 것을 로그로 남기는 데 쓴다.</summary>
    public static bool Knows(string? slotId) =>
        !string.IsNullOrEmpty(slotId) && Korean_.ContainsKey(slotId);
}
