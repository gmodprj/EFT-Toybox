using System.Linq;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

// 스태시를 EFT 필터로 재검증한 결과를 어떻게 처리하는가. 판정(필터)은 클라이언트가 하고, 여기서는 그 결과로
// 무엇을 남기고 무엇을 건너뛰며 무엇을 알리는지만 본다. 2026-10-27 사고(9x19 탄창의 5.56 탄 하나가 동료 생성
// 전체를 죽였다)의 재발 방지다.
public class StashRevalidationPolicyTests
{
    private static StashCandidate Ok(string? id) => new(id, true, null);
    private static StashCandidate Bad(string? id, string? reason = "No filter for item") => new(id, false, reason);

    [Fact]
    public void 전부_통과하면_전부_남는다()
    {
        var result = StashRevalidationPolicy.Partition(new[] { Ok("a"), Ok("b"), Ok("c") });

        Assert.Equal(new[] { "a", "b", "c" }, result.KeptIds);
        Assert.Empty(result.Skipped);
    }

    [Fact]
    public void 위반_항목_하나는_그것만_건너뛴다()
    {
        var result = StashRevalidationPolicy.Partition(new[] { Ok("a"), Bad("b"), Ok("c") });

        // 나머지는 그대로 보존된다. 하나 때문에 배치 전체가 죽지 않는 것이 이 수리의 전부다.
        Assert.Equal(new[] { "a", "c" }, result.KeptIds);
        Assert.Equal("b", Assert.Single(result.Skipped).Id);
    }

    [Fact]
    public void 건너뛴_항목은_사유와_함께_보고된다()
    {
        var result = StashRevalidationPolicy.Partition(new[] { Bad("b", "5.56 탄이 9x19 탄창에 들어 있다") });

        var skipped = Assert.Single(result.Skipped);
        Assert.Equal("b", skipped.Id);
        Assert.Equal("5.56 탄이 9x19 탄창에 들어 있다", skipped.Reason);
    }

    [Fact]
    public void 사유가_비어_있어도_보고에는_사유가_남는다()
    {
        // 사유가 없다고 조용히 버리면 사용자는 로그에서 이유를 찾을 수 없다.
        var result = StashRevalidationPolicy.Partition(new[] { Bad("b", null), Bad("c", "  ") });

        Assert.Equal(2, result.Skipped.Count);
        Assert.All(result.Skipped, s => Assert.False(string.IsNullOrWhiteSpace(s.Reason)));
    }

    [Fact]
    public void 전부_위반이면_빈_목록이되_예외는_없다()
    {
        var result = StashRevalidationPolicy.Partition(new[] { Bad("a"), Bad("b") });

        Assert.Empty(result.KeptIds);
        Assert.Equal(2, result.Skipped.Count);
    }

    [Fact]
    public void id_가_없는_항목은_남기지_않는다()
    {
        // 통과(Accepted)했어도 이름을 댈 수 없는 항목은 닫힌 쪽이다. 건너뛰되 알린다.
        var result = StashRevalidationPolicy.Partition(new[] { Ok(null), Ok(""), Ok("  "), Ok("a") });

        Assert.Equal(new[] { "a" }, result.KeptIds);
        Assert.Equal(3, result.Skipped.Count);
        Assert.All(result.Skipped, s => Assert.False(string.IsNullOrWhiteSpace(s.Reason)));
    }

    [Fact]
    public void 입력이_null_이면_빈_결과다()
    {
        var result = StashRevalidationPolicy.Partition(null);

        Assert.Empty(result.KeptIds);
        Assert.Empty(result.Skipped);
    }

    [Fact]
    public void null_항목은_예외없이_건너뛰고_알린다()
    {
        var result = StashRevalidationPolicy.Partition(new StashCandidate[] { null!, Ok("a") });

        Assert.Equal(new[] { "a" }, result.KeptIds);
        Assert.Single(result.Skipped);
    }

    // 2026-10-27 사고 로그의 실제 문구(doc/fixes/2026-10-27-pit-stash-ammo.md). 이 줄이 추출을 통과해야 수리가 동작한다.
    private const string RealErrorLine =
        "No filter for item Pro+ (id: 6adea532010015d86e5ffb7e) in StackSlot cartridges in item mag_glock_glock_big_stick_9x19_33";

    [Fact]
    public void 실제_오류_문구에서_어긴_항목의_id_를_뽑는다()
    {
        Assert.True(StashRevalidationPolicy.TryExtractItemId(RealErrorLine, out var id));
        Assert.Equal("6adea532010015d86e5ffb7e", id);
    }

    [Fact]
    public void id_가_없는_문구는_뽑지_않는다()
    {
        Assert.False(StashRevalidationPolicy.TryExtractItemId("No filter for item Pro+ in StackSlot cartridges", out var id));
        Assert.Null(id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void 비어_있는_문구는_뽑지_않는다(string? text)
    {
        Assert.False(StashRevalidationPolicy.TryExtractItemId(text, out var id));
        Assert.Null(id);
    }

    [Theory]
    [InlineData("item (id: 6adea532010015d86e5ffb7)")]     // 23자리
    [InlineData("item (id: 6adea532010015d86e5ffb7e0)")]   // 25자리
    [InlineData("item (id: 6adea532010015d86e5ffb7z)")]    // 16진이 아님
    [InlineData("item id: 6adea532010015d86e5ffb7e")]      // 괄호 없음
    public void 길이가_틀리거나_형식이_다른_id_는_받아들이지_않는다(string text)
    {
        Assert.False(StashRevalidationPolicy.TryExtractItemId(text, out var id));
        Assert.Null(id);
    }

    [Fact]
    public void 서로_다른_id_가_둘_이상이면_어느_쪽이_어긴_항목인지_모르니_뽑지_않는다()
    {
        // 첫 id 가 어긴 항목이라는 가정은 "부모는 이름으로만 나온다"는 실제 문구 한 줄에 기댄다. 부모까지 id 로 적힌
        // 문구가 나오면 그 가정이 깨진 것이므로 추측하지 않고 닫힌 쪽(뽑지 않음 -> 클라이언트가 던짐)으로 간다.
        var text = "No filter for item A (id: aaaaaaaaaaaaaaaaaaaaaaaa) in slot of item B (id: bbbbbbbbbbbbbbbbbbbbbbbb)";

        Assert.False(StashRevalidationPolicy.TryExtractItemId(text, out var id));
        Assert.Null(id);
    }

    [Fact]
    public void 같은_id_가_반복되면_하나로_본다()
    {
        var text = "item (id: aaaaaaaaaaaaaaaaaaaaaaaa) again (id: AAAAAAAAAAAAAAAAAAAAAAAA)";

        Assert.True(StashRevalidationPolicy.TryExtractItemId(text, out var id));
        Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaa", id);
    }

    [Fact]
    public void 부모가_이름으로만_나오는_문구에서는_자식_id_를_뽑는다()
    {
        // 실제 문구: 어긴 탄약은 id 로, 부모 탄창은 이름(mag_glock_...)으로만 나온다. 그래서 id 는 하나뿐이고 어긴 항목의 것이다.
        Assert.DoesNotContain("(id:", RealErrorLine.Substring(RealErrorLine.IndexOf(") in ") + 1));

        Assert.True(StashRevalidationPolicy.TryExtractItemId(RealErrorLine, out var id));
        Assert.Equal("6adea532010015d86e5ffb7e", id);
    }

    [Fact]
    public void id_와_괄호_사이_공백이_달라도_뽑는다()
    {
        Assert.True(StashRevalidationPolicy.TryExtractItemId("x (id:6adea532010015d86e5ffb7e)", out var id));
        Assert.Equal("6adea532010015d86e5ffb7e", id);
    }

    [Fact]
    public void 실제_오류_줄을_추출해_Partition_에_넘기면_그_항목만_건너뛴다()
    {
        // 클라이언트가 하는 일의 축소판: 오류문 -> id 추출 -> 후보 만들기 -> 정책. 두 반쪽을 잇는 유일한 증거다.
        Assert.True(StashRevalidationPolicy.TryExtractItemId(RealErrorLine, out var rejectedId));
        var ids = new[] { "stash", "mag", rejectedId!, "other" };
        var candidates = ids.Select(i => i == rejectedId ? Bad(i, RealErrorLine) : Ok(i));

        var result = StashRevalidationPolicy.Partition(candidates);

        Assert.Equal(new[] { "stash", "mag", "other" }, result.KeptIds);
        var skipped = Assert.Single(result.Skipped);
        Assert.Equal("6adea532010015d86e5ffb7e", skipped.Id);
        Assert.Equal(RealErrorLine, skipped.Reason);
    }

    private static StashCandidate Node(string id, string? parent, bool accepted = true, string? reason = null) =>
        new(id, accepted, reason, parent);

    [Fact]
    public void 위반_항목의_자식은_함께_건너뛰고_사유에_조상을_적는다()
    {
        var result = StashRevalidationPolicy.Partition(new[]
        {
            Node("stash", null),
            Node("mag", "stash"),
            Node("round", "mag", accepted: false, reason: "No filter"),
        });
        var rejectedMag = StashRevalidationPolicy.Partition(new[]
        {
            Node("stash", null),
            Node("mag", "stash", accepted: false, reason: "No filter"),
            Node("round", "mag"),
        });

        Assert.Equal(new[] { "stash", "mag" }, result.KeptIds);
        Assert.Equal(new[] { "stash" }, rejectedMag.KeptIds);
        var child = Assert.Single(rejectedMag.Skipped, s => s.Id == "round");
        Assert.Contains("mag", child.Reason);
    }

    [Fact]
    public void 손자도_함께_건너뛰고_사유는_처음_어긴_항목을_가리킨다()
    {
        var result = StashRevalidationPolicy.Partition(new[]
        {
            Node("stash", null),
            Node("bad", "stash", accepted: false, reason: "No filter"),
            Node("child", "bad"),
            Node("grandchild", "child"),
        });

        Assert.Equal(new[] { "stash" }, result.KeptIds);
        Assert.Equal(3, result.Skipped.Count);
        // 사용자가 로그에서 따라가야 할 것은 중간 후손이 아니라 원인이 된 항목이다.
        Assert.Contains("bad", Assert.Single(result.Skipped, s => s.Id == "grandchild").Reason);
        Assert.Equal("No filter", Assert.Single(result.Skipped, s => s.Id == "bad").Reason);
    }

    [Fact]
    public void 자손이_먼저_나열돼도_조상을_찾아_건너뛴다()
    {
        var result = StashRevalidationPolicy.Partition(new[]
        {
            Node("grandchild", "child"),
            Node("child", "bad"),
            Node("bad", "stash", accepted: false, reason: "No filter"),
            Node("stash", null),
        });

        Assert.Equal(new[] { "stash" }, result.KeptIds);
    }

    [Fact]
    public void 부모가_목록에_없는_항목은_남긴다()
    {
        // 부모를 알 수 없다고 떨구는 것은 이 정책이 판단할 일이 아니다. 어긴 조상이 확인될 때만 함께 뺀다.
        var result = StashRevalidationPolicy.Partition(new[] { Node("a", "missing-parent"), Node("b", null) });

        Assert.Equal(new[] { "a", "b" }, result.KeptIds);
        Assert.Empty(result.Skipped);
    }

    [Fact]
    public void 위반_항목과_무관한_가지는_그대로_남는다()
    {
        var result = StashRevalidationPolicy.Partition(new[]
        {
            Node("stash", null),
            Node("bad", "stash", accepted: false, reason: "No filter"),
            Node("sibling", "stash"),
            Node("siblingChild", "sibling"),
        });

        Assert.Equal(new[] { "stash", "sibling", "siblingChild" }, result.KeptIds);
    }

    [Fact]
    public void 부모_사슬에_순환이_있어도_끝난다()
    {
        // 망가진 프로필의 a <-> b. 어긴 조상이 없으니 남기되, 무한 루프에 빠지지 않는 것이 핵심이다.
        var result = StashRevalidationPolicy.Partition(new[] { Node("a", "b"), Node("b", "a"), Node("c", null) });

        Assert.Equal(new[] { "a", "b", "c" }, result.KeptIds);
    }

    [Fact]
    public void 순환_속에_어긴_항목이_있으면_순환의_다른_항목도_뺀다()
    {
        var result = StashRevalidationPolicy.Partition(new[]
        {
            Node("a", "b"),
            Node("b", "a", accepted: false, reason: "No filter"),
        });

        Assert.Empty(result.KeptIds);
        Assert.Equal(2, result.Skipped.Count);
    }

    [Fact]
    public void 부모_id_의_대소문자가_달라도_조상을_찾는다()
    {
        var result = StashRevalidationPolicy.Partition(new[]
        {
            Node("AbCdEf", null, accepted: false, reason: "No filter"),
            Node("child", "abcdef"),
        });

        Assert.Empty(result.KeptIds);
    }

    [Fact]
    public void 건너뛴_것만_없으면_설명되지_않는_누락은_없다()
    {
        var missing = StashRevalidationPolicy.FindUnexplainedMissingIds(
            new[] { "stash", "mag", "round" }, new[] { "stash", "mag" }, new[] { "round" });

        Assert.Empty(missing);
    }

    [Fact]
    public void 건너뛰지_않은_항목이_빠졌으면_그_id_를_돌려준다()
    {
        // 계단식 제외나 트리 구성이 건너뛰라 하지 않은 것을 잃었다는 뜻이다. 조용히 통과시키면 안 된다.
        var missing = StashRevalidationPolicy.FindUnexplainedMissingIds(
            new[] { "stash", "mag", "round", "rifle" }, new[] { "stash", "mag" }, new[] { "round" });

        Assert.Equal(new[] { "rifle" }, missing);
    }

    [Fact]
    public void 누락_비교는_대소문자를_가리지_않는다()
    {
        var missing = StashRevalidationPolicy.FindUnexplainedMissingIds(
            new[] { "AbC", "dEf" }, new[] { "abc" }, new[] { "DEF" });

        Assert.Empty(missing);
    }

    [Fact]
    public void 현재_목록을_알_수_없으면_전부_설명되지_않은_누락이다()
    {
        var missing = StashRevalidationPolicy.FindUnexplainedMissingIds(new[] { "a", "b" }, null, new[] { "a" });

        Assert.Equal(new[] { "b" }, missing);
    }

    // 건너뛴 항목이 있는 세션은 클라이언트의 메모리 스태시에 그 아이템이 없다. 서버는 동료 장비 저장 때 제출된 스태시로
    // 플레이어 스태시를 통째로 바꾸므로, 그 상태로 저장하면 아이템이 프로필에서 영구히 사라진다. 그래서 막는다.
    [Fact]
    public void 건너뛴_항목이_기록된_세션은_장비_저장을_거절한다()
    {
        var refusal = StashRevalidationPolicy.EvaluateEditGate(new[] { "6adea532010015d86e5ffb7e" });

        Assert.False(string.IsNullOrWhiteSpace(refusal));
    }

    [Fact]
    public void 건너뛴_항목이_없는_세션은_장비_저장을_허용한다()
    {
        Assert.Null(StashRevalidationPolicy.EvaluateEditGate(new string[0]));
        Assert.Null(StashRevalidationPolicy.EvaluateEditGate(null));
    }

    [Fact]
    public void 거절_메시지는_건너뛴_id_와_서버에_온전하다는_것과_재시작을_말한다()
    {
        var message = StashRevalidationPolicy.EvaluateEditGate(new[] { "aaaaaaaaaaaaaaaaaaaaaaaa", "bbbbbbbbbbbbbbbbbbbbbbbb" });

        Assert.NotNull(message);
        Assert.Contains("aaaaaaaaaaaaaaaaaaaaaaaa", message);
        Assert.Contains("bbbbbbbbbbbbbbbbbbbbbbbb", message);
        Assert.Contains("intact", message);
        Assert.Contains("server", message);
        Assert.Contains("restart", message, System.StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void 기록된_id_가_전부_공백이어도_닫힌_쪽으로_거절한다()
    {
        // 기록이 있다는 사실이 중요하다. id 문자열이 망가졌다고 허용으로 넘어가면 안 된다.
        Assert.False(string.IsNullOrWhiteSpace(StashRevalidationPolicy.EvaluateEditGate(new[] { "", "  " })));
    }
}
