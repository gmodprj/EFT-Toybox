using System.Text.Json.Serialization;

namespace ProfilePorter.Server;

/// <summary>`config.jsonc` 의 모양.</summary>
public class Config
{
    /// <summary>false 면 라우트가 전부 거절한다. 설치해 둔 채 끌 수 있다.</summary>
    [JsonPropertyName("ModEnabled")]
    public bool ModEnabled { get; set; } = true;

    /// <summary>요청마다 무엇을 했는지 서버 로그에 적는다.</summary>
    [JsonPropertyName("Debug")]
    public bool Debug { get; set; }

    /// <summary>
    /// 들여올 수 없는 아이템 종류(tpl). 퀘스트 물품처럼 그 프로필의 진행에 묶인 것을 적는다.
    ///
    /// 비워 두어도 된다 — 퀘스트 물품은 `questRaidItems`·`questStashItems` 라는 **별도 루트**에
    /// 있어 스태시 바닥을 고르는 길로는 애초에 들어오지 않는다(설계 §9.1 에서 실측).
    /// 이 목록은 "그래도 막고 싶은 것" 을 사람이 적는 자리다.
    /// </summary>
    [JsonPropertyName("BlockedTemplateIds")]
    public List<string> BlockedTemplateIds { get; set; } = [];

    /// <summary>
    /// 들여오기 전에 받는 쪽 프로필을 백업할지. **기본은 켜짐**이고, 끄는 것을 권하지 않는다.
    /// 들여오기는 되돌릴 수 없는 쓰기다(설계 §4.6).
    /// </summary>
    [JsonPropertyName("BackupBeforeImport")]
    public bool BackupBeforeImport { get; set; } = true;
}
