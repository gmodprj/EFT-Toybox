using Newtonsoft.Json;
using pitTeam.Shared;
using Xunit;

namespace SptTeamManager.Tests;

public class StmRosterWireTests
{
    private const string Own = "777";

    private static string Env(string members, string candidates = "[]", int err = 0, string errmsg = "null") =>
        $"{{\"err\":{err},\"errmsg\":{errmsg},\"data\":{{\"members\":{members},\"candidates\":{candidates}}}}}";

    private static string Row(string id, int aid, string status, string nick = "N", int level = 10, string side = "Usec") =>
        $"{{\"profileId\":\"{id}\",\"aid\":{aid},\"nickname\":\"{nick}\",\"level\":{level},\"side\":\"{side}\",\"status\":\"{status}\"}}";

    [Fact]
    public void 깨진_JSON_은_예외로_나가고_성공으로_읽히지_않는다()
    {
        Assert.ThrowsAny<JsonException>(() => StmRosterWire.Interpret("{not json", Own));
    }

    [Fact]
    public void 빈_응답과_객체가_아닌_응답은_실패다()
    {
        Assert.False(StmRosterWire.Interpret("", Own).Ok);
        Assert.False(StmRosterWire.Interpret("[]", Own).Ok);
    }

    [Fact]
    public void err_가_0이_아니면_실패이고_메시지를_담는다()
    {
        var s = StmRosterWire.Interpret(Env("[]", err: 400, errmsg: "\"StmProfileMemberFailed\""), Own);
        Assert.False(s.Ok);
        Assert.Equal("StmProfileMemberFailed", s.Error);
        Assert.Empty(s.Members);
    }

    [Fact]
    public void err_가_없으면_실패다()
    {
        Assert.False(StmRosterWire.Interpret("{\"data\":{}}", Own).Ok);
    }

    [Fact]
    public void data_가_없거나_객체가_아니면_실패다()
    {
        Assert.False(StmRosterWire.Interpret("{\"err\":0,\"data\":null}", Own).Ok);
        Assert.False(StmRosterWire.Interpret("{\"err\":0}", Own).Ok);
    }

    [Fact]
    public void 서버가_CurrentSession_으로_표시한_행은_멤버에서_빠진다()
    {
        var s = StmRosterWire.Interpret(Env($"[{Row("a", 1, "CurrentSession")},{Row("b", 2, "Ready")}]"), Own);
        Assert.True(s.Ok);
        Assert.Equal("b", Assert.Single(s.Members).ProfileId);
    }

    [Fact]
    public void 서버가_Ready_라고_해도_Aid_가_내_계정이면_멤버와_후보_모두에서_빠진다()
    {
        var s = StmRosterWire.Interpret(
            Env($"[{Row("a", 777, "Ready")},{Row("b", 2, "Ready")}]", $"[{Row("c", 777, "Ready")},{Row("d", 3, "Ready")}]"), Own);
        Assert.Equal("b", Assert.Single(s.Members).ProfileId);
        Assert.Equal("d", Assert.Single(s.Candidates).ProfileId);
    }

    [Fact]
    public void 알_수_없는_상태_문자열은_Unreadable_이고_후보가_되지_못한다()
    {
        var s = StmRosterWire.Interpret(Env($"[{Row("a", 1, "Bogus")}]", $"[{Row("c", 3, "Bogus")}]"), Own);
        Assert.Equal(RosterRowStatus.Unreadable, Assert.Single(s.Members).Status);
        Assert.Empty(s.Candidates);
        Assert.Equal(RosterRowStatus.Unreadable, StmRosterWire.ParseStatus("99"));
        Assert.Equal(RosterRowStatus.Unreadable, StmRosterWire.ParseStatus(null!));
        Assert.Equal(RosterRowStatus.Ready, StmRosterWire.ParseStatus(" ready "));
    }

    [Fact]
    public void 후보는_Ready_이고_Aid_가_있는_것만_남는다()
    {
        var s = StmRosterWire.Interpret(
            Env("[]", $"[{Row("a", 0, "Ready")},{Row("b", 2, "Empty")},{Row("c", 3, "Ready")}]"), Own);
        Assert.Equal("c", Assert.Single(s.Candidates).ProfileId);
    }

    [Fact]
    public void 내_계정을_모르면_후보는_비고_멤버는_서버_상태만으로_남으며_OwnAccountKnown_은_false다()
    {
        var s = StmRosterWire.Interpret(
            Env($"[{Row("a", 777, "Ready")},{Row("b", 2, "CurrentSession")}]", $"[{Row("c", 3, "Ready")}]"), null!);
        Assert.True(s.Ok);
        Assert.False(s.OwnAccountKnown);
        Assert.Empty(s.Candidates);
        Assert.Equal("a", Assert.Single(s.Members).ProfileId);
    }

    [Fact]
    public void 행의_필드가_그대로_옮겨진다()
    {
        var s = StmRosterWire.Interpret(Env($"[{Row(" p1 ", 42, "Empty", "Nick", 33, "Bear")}]"), Own);
        var r = Assert.Single(s.Members);
        Assert.Equal("p1", r.ProfileId);
        Assert.Equal(42, r.Aid);
        Assert.Equal("Nick", r.Nickname);
        Assert.Equal(33, r.Level);
        Assert.Equal("Bear", r.Side);
        Assert.Equal(RosterRowStatus.Empty, r.Status);
        Assert.True(s.OwnAccountKnown);
    }

    [Fact]
    public void profileId_가_빈_행은_버린다()
    {
        var s = StmRosterWire.Interpret(Env($"[{Row("", 1, "Ready")},{{\"aid\":2}}]"), Own);
        Assert.Empty(s.Members);
    }
}
