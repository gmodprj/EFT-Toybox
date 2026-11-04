using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 계획 4 Task 2. 칸 단위로 무엇을 사고 무엇을 벗길지 정한다("전부 새로 구매" 결정에 맞춘다).
public class GearBuildPlanPolicyTests
{
    private const string BuildRoot = "build-root";
    private const string CurrentRoot = "equipment-root";

    private static GearItemRow Item(string id, string tpl, string parent, string slot) => new(id, tpl, parent, slot);

    // 아래로 따라 내려갈 자리의 어휘. 서버의 ChildSlotNamesOf 가 템플릿에서 읽어 주는 것을
    // 테스트가 손으로 적는다 - 부착물 칸(Slots)만이 아니라 탄 자리(Cartridges)와 격자(Grids)도
    // 포함한다는 것이 2026-11-04 의 판단이다(산 탄창이 비고 조끼가 텅 비던 결함).
    private static IEnumerable<string> Slots(string tpl) => tpl switch
    {
        "gun" => ["mod_scope", "mod_magazine"],
        "mag" => ["cartridges"],
        "vest" => ["mod_plate_front", "main"],
        _ => [],
    };

    [Fact]
    public void 빌드에만_있는_칸은_사기만_한다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "helmet", BuildRoot, "Headwear")], BuildRoot,
            [], CurrentRoot, Slots);

        var slot = Assert.Single(plan.Slots);
        Assert.Equal("Headwear", slot.SlotId);
        Assert.Equal(GearSlotOutcome.Added, slot.Outcome);
        Assert.Equal(["helmet"], slot.ItemsToBuy.Select(i => i.Tpl));
        Assert.Empty(slot.ItemsToStash);
    }

    [Fact]
    public void 팀원에게만_있는_칸은_벗기기만_한다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [], BuildRoot,
            [Item("c1", "helmet", CurrentRoot, "Headwear")], CurrentRoot, Slots);

        var slot = Assert.Single(plan.Slots);
        Assert.Equal(GearSlotOutcome.Removed, slot.Outcome);
        Assert.Equal(["helmet"], slot.ItemsToStash.Select(i => i.Tpl));
        Assert.Empty(slot.ItemsToBuy);
    }

    [Fact]
    public void 같은_것을_입고_있으면_사지도_벗기지도_않는다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "helmet", BuildRoot, "Headwear")], BuildRoot,
            [Item("c1", "helmet", CurrentRoot, "Headwear")], CurrentRoot, Slots);

        var slot = Assert.Single(plan.Slots);
        Assert.Equal(GearSlotOutcome.Unchanged, slot.Outcome);
        Assert.Empty(slot.ItemsToBuy);
        Assert.Empty(slot.ItemsToStash);
        Assert.True(plan.NothingToDo);
    }

    [Fact]
    public void 다른_것을_입고_있으면_통째로_바꾼다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "helmet-new", BuildRoot, "Headwear")], BuildRoot,
            [Item("c1", "helmet-old", CurrentRoot, "Headwear")], CurrentRoot, Slots);

        var slot = Assert.Single(plan.Slots);
        Assert.Equal(GearSlotOutcome.Replaced, slot.Outcome);
        Assert.Equal(["helmet-new"], slot.ItemsToBuy.Select(i => i.Tpl));
        Assert.Equal(["helmet-old"], slot.ItemsToStash.Select(i => i.Tpl));
    }

    // 같은 총이라도 조준경이 다르면 사용자가 고른 것과 다른 장비다.
    [Fact]
    public void 부착물이_하나라도_다르면_다른_장비다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "gun", BuildRoot, "FirstPrimaryWeapon"), Item("b2", "scope-a", "b1", "mod_scope")], BuildRoot,
            [Item("c1", "gun", CurrentRoot, "FirstPrimaryWeapon"), Item("c2", "scope-b", "c1", "mod_scope")], CurrentRoot, Slots);

        var slot = Assert.Single(plan.Slots);
        Assert.Equal(GearSlotOutcome.Replaced, slot.Outcome);
        Assert.Equal(2, slot.ItemsToBuy.Count);
        Assert.Equal(2, slot.ItemsToStash.Count);
    }

    [Fact]
    public void 부착물까지_같으면_그대로_둔다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "gun", BuildRoot, "FirstPrimaryWeapon"), Item("b2", "scope-a", "b1", "mod_scope")], BuildRoot,
            [Item("c1", "gun", CurrentRoot, "FirstPrimaryWeapon"), Item("c2", "scope-a", "c1", "mod_scope")], CurrentRoot, Slots);

        Assert.Equal(GearSlotOutcome.Unchanged, Assert.Single(plan.Slots).Outcome);
    }

    // 조끼 안의 짐도 산다. 2026-11-04 이전에는 일부러 뺐는데, 사용자가 고른 꾸러미 화면에 그 짐이
    // 들어 있으므로 "보여 준 것과 산 것이 다른" 결함이었다(실측: 조끼 안이 텅 빈 채로 들어왔다).
    [Fact]
    public void 격자_안의_짐도_산다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "vest", BuildRoot, "TacticalVest"), Item("b2", "cargo", "b1", "main")], BuildRoot,
            [], CurrentRoot, Slots);

        var slot = Assert.Single(plan.Slots);
        Assert.Equal(["vest", "cargo"], slot.ItemsToBuy.Select(i => i.Tpl));
    }

    // 탄창 안의 탄도 산다. 탄 자리는 Slots 가 아니라 Cartridges 에 있어서, 어휘가 좁던 동안
    // 산 탄창이 0/50 으로 들어왔다(2026-11-04 사용자 실측).
    [Fact]
    public void 탄창_안의_탄도_산다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [
                Item("b1", "gun", BuildRoot, "FirstPrimaryWeapon"),
                Item("b2", "mag", "b1", "mod_magazine"),
                Item("b3", "ammo", "b2", "cartridges"),
            ], BuildRoot,
            [], CurrentRoot, Slots);

        Assert.Equal(["gun", "mag", "ammo"], Assert.Single(plan.Slots).ItemsToBuy.Select(i => i.Tpl));
    }

    [Fact]
    public void 방탄판은_장비의_일부로_센다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "vest", BuildRoot, "TacticalVest"), Item("b2", "plate", "b1", "mod_plate_front")], BuildRoot,
            [], CurrentRoot, Slots);

        Assert.Equal(["vest", "plate"], Assert.Single(plan.Slots).ItemsToBuy.Select(i => i.Tpl));
    }

    // 뿌리 칸이 아닌 것(주머니·보안 컨테이너·완장·칼집)은 빌드가 건드리지 않는다.
    [Theory]
    [InlineData("Pockets")]
    [InlineData("SecuredContainer")]
    [InlineData("ArmBand")]
    [InlineData("Scabbard")]
    public void 뿌리_칸이_아닌_자리는_계획에_들어오지_않는다(string slot)
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "thing", BuildRoot, slot)], BuildRoot,
            [Item("c1", "other", CurrentRoot, slot)], CurrentRoot, Slots);

        Assert.Empty(plan.Slots);
    }

    [Fact]
    public void 여러_칸을_한_계획에_담는다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "helmet", BuildRoot, "Headwear"), Item("b2", "gun", BuildRoot, "FirstPrimaryWeapon")], BuildRoot,
            [Item("c1", "helmet", CurrentRoot, "Headwear"), Item("c2", "old-gun", CurrentRoot, "FirstPrimaryWeapon")], CurrentRoot, Slots);

        Assert.Equal(2, plan.Slots.Count);
        Assert.Equal(["gun"], plan.AllItemsToBuy.Select(i => i.Tpl));
        Assert.Equal(["old-gun"], plan.AllItemsToStash.Select(i => i.Tpl));
        Assert.False(plan.NothingToDo);
    }

    [Fact]
    public void 빈_입력도_던지지_않는다()
    {
        var plan = GearBuildPlanPolicy.Plan(null, null, null, null, null);

        Assert.Empty(plan.Slots);
        Assert.True(plan.NothingToDo);
    }

    // 부착물 칸 목록을 주지 않으면 뿌리 아이템만 본다 - 조용히 하위를 상상하지 않는다.
    [Fact]
    public void 부착물_칸을_모르면_뿌리만_센다()
    {
        var plan = GearBuildPlanPolicy.Plan(
            [Item("b1", "gun", BuildRoot, "FirstPrimaryWeapon"), Item("b2", "scope-a", "b1", "mod_scope")], BuildRoot,
            [], CurrentRoot, null);

        Assert.Equal(["gun"], Assert.Single(plan.Slots).ItemsToBuy.Select(i => i.Tpl));
    }
}
