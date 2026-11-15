namespace PrestigeStashRelax;

/// <summary>
/// 전송 창고 필터에 넣을 **분류(Node) id** 표.
///
/// 필터(`includedItems`)에는 구체 아이템 id 와 분류 id 가 섞여 들어간다 — 바닐라가 이미
/// 분류를 7~11개 쓰고 있다(P1 은 BuildingMaterial·Fuel·FoodDrink·Map·Drugs·Medical,
/// P3 이상은 거기에 ArmoredEquipment·Headphones·Weapon·Mod 를 더한다). 그래서 분류 하나를
/// 더하면 그 아래가 한꺼번에 열린다 — **다만 클라이언트가 부모 사슬을 거슬러 맞추는지는
/// 아직 안 재 봤다**(설계 §6-1). 그것을 재는 것이 이 모드의 첫 일이다.
///
/// id 는 2026-11-15 에 설치본 `templateTable.Items` 에서 `_type == "Node"` 인 것만 골라
/// 이름으로 찾아 적었다.
/// </summary>
public static class CategoryCatalog
{
    /// <summary>배낭. `SearchableItem` 아래다.</summary>
    public const string Backpack = "5448e53e4bdc2d60728b4567";

    /// <summary>가방·케이스 전반(SICC, 서류함, 탄 가방 …). `CompoundItem` 아래다.</summary>
    public const string SimpleContainer = "5795f317245977243854e041";

    /// <summary>보안 컨테이너(감마 등). 열면 안에 든 것이 그대로 넘어간다.</summary>
    public const string MobContainer = "5448bf274bdc2dfc2f8b456a";

    /// <summary>전술 조끼. `ArmoredEquipment` 가 아니라 `Equipment` 아래라 P3 이상에서도 막혀 있다.</summary>
    public const string Vest = "5448e5284bdc2dcb718b4567";

    /// <summary>돈.</summary>
    public const string Money = "543be5dd4bdc2deb348b4569";

    /// <summary>탄.</summary>
    public const string Ammo = "5485a8684bdc2da71d8b4567";

    /// <summary>탄 박스.</summary>
    public const string AmmoBox = "543be5cb4bdc2deb348b4568";

    /// <summary>탄창.</summary>
    public const string Magazine = "5448bc234bdc2d3c308b4569";

    /// <summary>열쇠.</summary>
    public const string Key = "543be5e94bdc2df1348b4568";

    /// <summary>카드 열쇠.</summary>
    public const string Keycard = "5c164d2286f774194c5e69fa";

    /// <summary>무기. P3 이상에는 이미 들어 있다.</summary>
    public const string Weapon = "5422acb9af1c889c16000029";

    /// <summary>부품. P3 이상에는 이미 들어 있다.</summary>
    public const string Mod = "5448fe124bdc2da5018b4567";

    /// <summary>방어 장비(방탄복·헬멧 등). P3 이상에는 이미 들어 있다.</summary>
    public const string ArmoredEquipment = "57bef4c42459772e8d35a53b";

    /// <summary>
    /// 분류 사슬의 **뿌리**. 클라이언트가 부모를 거슬러 맞춘다면 이 하나로 전부 열린다.
    /// 그래서 "전부 허용"의 측정 대상이 된다.
    /// </summary>
    public const string RootItem = "54009119af1c881c07000029";
}
