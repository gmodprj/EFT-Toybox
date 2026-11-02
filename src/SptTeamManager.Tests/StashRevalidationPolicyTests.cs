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
    public void id_가_여럿이면_첫_번째_어긴_항목을_뽑는다()
    {
        var text = "No filter for item A (id: aaaaaaaaaaaaaaaaaaaaaaaa) in slot of item B (id: bbbbbbbbbbbbbbbbbbbbbbbb)";

        Assert.True(StashRevalidationPolicy.TryExtractItemId(text, out var id));
        Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaa", id);
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
}
