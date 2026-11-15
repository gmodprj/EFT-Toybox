namespace PrestigeStashRelax;

/// <summary>한 단계의 전송 창고 값. 원본과 적용 결과 둘 다 이 꼴로 다룬다.</summary>
public sealed record StashShape(int XCellCount, int YCellCount, List<string> IncludedItems);

/// <summary>
/// **원본 → 설정 → 새 값**을 만드는 순수 계산. SPT 어셈블리를 쓰지 않아 시험에서 그대로 돈다.
///
/// 늘 **원본에서 시작한다.** 지금 값에 더하면 두 번 적용할 때 세로가 두 번 늘어난다
/// (`regenerating-must-be-idempotent`). 그래서 호출하는 쪽은 원본 사본을 들고 있어야 하고,
/// 이 함수는 그 사본을 건드리지 않는다.
/// </summary>
public static class StashPlan
{
    /// <summary>칸 수의 아래끝. 0 이나 음수면 화면이 아무것도 못 그린다.</summary>
    public const int MinCells = 1;

    /// <summary>
    /// 격자의 위끝. **화면이 어디까지 그리는지 아직 안 재 봤다**(설계 §6-2) — 지금 값은
    /// 재는 동안 실수로 엄청난 수를 넣지 않게 막는 울타리일 뿐이고, 측정이 끝나면 실측값으로
    /// 바꾼다.
    /// </summary>
    public const int MaxCellsUntilMeasured = 64;

    public static StashShape Build(StashShape origin, Config config)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(config);

        var x = config.XCellCount > 0 ? config.XCellCount : origin.XCellCount;

        var y = origin.YCellCount + config.YCellAdd;
        if (config.YCellCount > 0)
        {
            y = config.YCellCount;
        }

        x = Clamp(x);
        y = Clamp(y);

        // 원본 순서를 지킨 채 뒤에 더한다. 중복은 들어가지 않는다.
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var items = new List<string>();

        void Add(string id)
        {
            if (!string.IsNullOrWhiteSpace(id) && seen.Add(id))
            {
                items.Add(id);
            }
        }

        if (!config.ReplaceFilter)
        {
            foreach (var id in origin.IncludedItems)
            {
                Add(id);
            }
        }

        if (config.AllowEverything)
        {
            // 뿌리 하나면 나머지 스위치는 의미가 없지만, 측정 중에 무엇을 켰는지 로그로
            // 남는 편이 나으므로 아래 스위치도 그대로 처리한다.
            Add(CategoryCatalog.RootItem);
        }

        if (config.AllowBags)
        {
            Add(CategoryCatalog.Backpack);
            Add(CategoryCatalog.SimpleContainer);
        }

        if (config.AllowVest)
        {
            Add(CategoryCatalog.Vest);
        }

        if (config.AllowSecureContainer)
        {
            Add(CategoryCatalog.MobContainer);
        }

        if (config.AllowMoney)
        {
            Add(CategoryCatalog.Money);
        }

        if (config.AllowAmmo)
        {
            Add(CategoryCatalog.Ammo);
            Add(CategoryCatalog.AmmoBox);
            Add(CategoryCatalog.Magazine);
        }

        if (config.AllowKeys)
        {
            Add(CategoryCatalog.Key);
            Add(CategoryCatalog.Keycard);
        }

        if (config.AllowWeaponsEverywhere)
        {
            Add(CategoryCatalog.Weapon);
            Add(CategoryCatalog.Mod);
        }

        if (config.AllowArmorEverywhere)
        {
            Add(CategoryCatalog.ArmoredEquipment);
        }

        foreach (var id in config.ExtraIds)
        {
            Add(id);
        }

        return new StashShape(x, y, items);
    }

    private static int Clamp(int value) =>
        value < MinCells ? MinCells
        : value > MaxCellsUntilMeasured ? MaxCellsUntilMeasured
        : value;
}
