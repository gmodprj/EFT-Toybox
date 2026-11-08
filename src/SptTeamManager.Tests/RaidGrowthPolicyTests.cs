using System.Collections.Generic;
using System.Linq;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 계획 6 A5-2. 설계: doc/specs/2026-11-08-stm-raid-xp-skills-design.md
//
// 이 시험이 지키는 것은 넷이다(설계 §3 의 "실패로 보는 것"):
//   레벨이 내려가지 않는다 · 표의 끝(79)을 넘지 않는다 · 스킬이 5,100 을 넘지 않는다 ·
//   Level 과 Experience 가 어긋나지 않는다.
// 상한은 **없다**(§4-2·§4-3). 한 판에 레벨이 여러 개 오르는 것과 Strength 가 188 오르는 것은
// 둘 다 **기대 동작**이고, 아래 두 시험이 그것을 못박는다 — 나중에 누가 난간이랍시고 상한을
// 다시 넣으면 그 둘이 빨개진다.
public class RaidGrowthPolicyTests
{
    private const string ProfileId = "6aeedfe7e12f66949c9c74dc";

    // SPT globals 의 exp_table 과 같은 모양(단계별 값). 누적이 아니다.
    // 실제 표의 앞부분 그대로다: 레벨 2 는 1,000, 레벨 3 은 누적 4,017.
    private static readonly long[] Table = [0, 1000, 3017, 4415, 5900, 7800];

    private static RaidGrowth Growth(double experience = 0, params SkillGain[] skills) =>
        new RaidGrowth(ProfileId, experience, skills);

    private static List<SkillState> State(params (string Id, double Progress)[] rows) =>
        rows.Select(r => new SkillState(r.Id, r.Progress)).ToList();

    private static double ProgressOf(GrowthPlan plan, string id) =>
        plan.SkillWrites.Single(w => w.Id == id).Progress;

    private static bool Writes(GrowthPlan plan, string id) =>
        plan.SkillWrites.Any(w => w.Id == id);

    // ---- 경험치와 레벨 ----

    [Fact]
    public void 경험치가_늘고_레벨이_표대로_오른다()
    {
        // 표는 **단계별** 값이라 레벨 3 의 문턱은 0+1000+3017 = 누적 4,017 이다.
        var plan = RaidGrowthPolicy.Decide(Growth(2900), currentExperience: 1200, currentLevel: 2, State(), Table);

        Assert.True(plan.Accepted);
        Assert.Equal(2900, plan.ExperienceDelta);
        Assert.Equal(4100, plan.NewExperience);
        Assert.Equal(3, plan.NewLevel); // 누적 4,017 을 넘었다
    }

    [Fact]
    public void 레벨_1_에_큰_경험치가_오면_여러_레벨이_한번에_오른다()
    {
        // 2026-11-08 실측: Wraith(레벨 1, 898)가 한 판에 3,602 를 받았다. 상한이 없으므로 레벨 3 이다.
        var plan = RaidGrowthPolicy.Decide(Growth(3602), currentExperience: 898, currentLevel: 1, State(), Table);

        Assert.True(plan.Accepted);
        Assert.Equal(4500, plan.NewExperience);
        Assert.Equal(3, plan.NewLevel);
    }

    [Fact]
    public void 레벨은_내려가지_않는다()
    {
        // 적힌 레벨이 표보다 높은 프로필(손으로 만든 값 등). 경험치는 늘지만 레벨은 그대로 둔다.
        var plan = RaidGrowthPolicy.Decide(Growth(10), currentExperience: 1200, currentLevel: 5, State(), Table);

        Assert.True(plan.Accepted);
        Assert.Equal(5, plan.NewLevel);
    }

    [Fact]
    public void 레벨은_표의_끝을_넘지_않는다()
    {
        var plan = RaidGrowthPolicy.Decide(Growth(10_000_000), currentExperience: 0, currentLevel: 1, State(), Table);

        Assert.True(plan.Accepted);
        Assert.Equal(Table.Length, plan.NewLevel); // 표가 여섯 단이면 끝은 6 이다(실제 표는 79)
    }

    [Fact]
    public void 경험치가_0_이하면_경험치를_쓰지_않는다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Strength", 5)), currentExperience: 1200, currentLevel: 2, State(), Table);

        Assert.True(plan.Accepted);           // 스킬이 있으니 쓸 것은 있다
        Assert.Equal(0, plan.ExperienceDelta);
        Assert.Equal(1200, plan.NewExperience);
        Assert.Equal(2, plan.NewLevel);
    }

    [Fact]
    public void 음수_경험치는_없는_값으로_친다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(-500, new SkillGain("Strength", 5)), currentExperience: 1200, currentLevel: 2, State(), Table);

        Assert.Equal(0, plan.ExperienceDelta);
        Assert.Equal(1200, plan.NewExperience);
    }

    [Fact]
    public void 유한수가_아닌_경험치는_없는_값으로_친다()
    {
        foreach (double bad in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            var plan = RaidGrowthPolicy.Decide(
                Growth(bad, new SkillGain("Strength", 5)), currentExperience: 1200, currentLevel: 2, State(), Table);

            Assert.Equal(0, plan.ExperienceDelta);
            Assert.Equal(1200, plan.NewExperience);
        }
    }

    [Fact]
    public void 경험치_표가_없으면_거절한다()
    {
        // 표가 없으면 레벨을 셀 수 없다. 경험치만 올리면 Level 과 Experience 가 어긋난 채 저장된다.
        foreach (IReadOnlyList<long>? table in new IReadOnlyList<long>?[] { null, [] })
        {
            var plan = RaidGrowthPolicy.Decide(Growth(2000), 1200, 2, State(), table);

            Assert.False(plan.Accepted);
            Assert.NotEmpty(plan.Reason);
            Assert.Empty(plan.SkillWrites);
        }
    }

    // ---- 스킬 ----

    [Fact]
    public void 스킬은_그_판의_몫만_더한다()
    {
        // 설계 §4-3: 덮어쓰지 않는다. 4,000 짜리에 188.24 를 더해 4,188.24 다.
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Strength", 188.24)),
            currentExperience: 100, currentLevel: 1, State(("Strength", 4000)), Table);

        Assert.True(plan.Accepted);
        Assert.Equal(4188.24, ProgressOf(plan, "Strength"), 3);
    }

    [Fact]
    public void 한_판에_188_이_올라도_깎지_않는다()
    {
        // 2026-11-08 실측값이다(이 모드의 이동 스킬 보정). 상한을 다시 넣으면 이 시험이 빨개진다.
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Strength", 172.29)),
            currentExperience: 100, currentLevel: 1, State(("Strength", 0)), Table);

        Assert.Equal(172.29, ProgressOf(plan, "Strength"), 3);
    }

    [Fact]
    public void 스킬은_5100_을_넘지_않는다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Endurance", 200)),
            currentExperience: 100, currentLevel: 1, State(("Endurance", 5000)), Table);

        Assert.Equal(5100, ProgressOf(plan, "Endurance"), 3);
    }

    [Fact]
    public void 이미_5100_이면_그_스킬은_쓰지_않는다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Endurance", 200)),
            currentExperience: 100, currentLevel: 1, State(("Endurance", 5100)), Table);

        Assert.False(Writes(plan, "Endurance"));
        Assert.False(plan.Accepted); // 쓸 것이 하나도 없다
    }

    [Fact]
    public void 없는_스킬은_새로_만든다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Pistol", 1.1)),
            currentExperience: 100, currentLevel: 1, State(("Strength", 4000)), Table);

        Assert.True(plan.Accepted);
        Assert.Equal(1.1, ProgressOf(plan, "Pistol"), 3);
        Assert.False(Writes(plan, "Strength")); // 건드리지 않은 것은 쓰지 않는다
    }

    [Fact]
    public void 같은_Id_가_두_번_오면_합쳐서_한_번만_쓴다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("Health", 10), new SkillGain("Health", 1.48)),
            currentExperience: 100, currentLevel: 1, State(("Health", 4000)), Table);

        Assert.Single(plan.SkillWrites);
        Assert.Equal(4011.48, ProgressOf(plan, "Health"), 3);
    }

    [Fact]
    public void 올라가지_않는_스킬은_버린다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0,
                new SkillGain("Endurance", 0),
                new SkillGain("Health", -5),
                new SkillGain("Vitality", double.NaN),
                new SkillGain("Search", double.PositiveInfinity)),
            currentExperience: 100, currentLevel: 1, State(("Endurance", 4000)), Table);

        Assert.Empty(plan.SkillWrites);
        Assert.False(plan.Accepted);
    }

    [Fact]
    public void 이름_없는_스킬은_버린다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain(null, 5), new SkillGain("   ", 5)),
            currentExperience: 100, currentLevel: 1, State(), Table);

        Assert.Empty(plan.SkillWrites);
    }

    [Fact]
    public void 대소문자가_달라도_같은_스킬로_본다()
    {
        var plan = RaidGrowthPolicy.Decide(
            Growth(0, new SkillGain("strength", 1)),
            currentExperience: 100, currentLevel: 1, State(("Strength", 4000)), Table);

        Assert.Single(plan.SkillWrites);
        Assert.Equal("Strength", plan.SkillWrites[0].Id); // 프로필에 있는 이름을 그대로 쓴다
        Assert.Equal(4001, ProgressOf(plan, "Strength"), 3);
    }

    // ---- 거절 ----

    [Fact]
    public void 쓸_것이_하나도_없으면_거절한다()
    {
        var plan = RaidGrowthPolicy.Decide(Growth(0), 1200, 2, State(), Table);

        Assert.False(plan.Accepted);
        Assert.NotEmpty(plan.Reason);
        Assert.Equal(0, plan.ExperienceDelta);
        Assert.Empty(plan.SkillWrites);
    }

    [Fact]
    public void 보고가_없거나_프로필_id_가_비면_거절한다()
    {
        Assert.False(RaidGrowthPolicy.Decide(null, 1200, 2, State(), Table).Accepted);
        Assert.False(RaidGrowthPolicy.Decide(new RaidGrowth(null, 2000, []), 1200, 2, State(), Table).Accepted);
        Assert.False(RaidGrowthPolicy.Decide(new RaidGrowth("  ", 2000, []), 1200, 2, State(), Table).Accepted);
    }

    [Fact]
    public void 거절한_계획은_지금_값을_그대로_들고_있다()
    {
        // 호출자가 거절을 무시하고 써도 바뀌는 것이 없게.
        var plan = RaidGrowthPolicy.Decide(null, 1200, 2, State(), Table);

        Assert.Equal(1200, plan.NewExperience);
        Assert.Equal(2, plan.NewLevel);
        Assert.Equal(0, plan.ExperienceDelta);
    }
}
