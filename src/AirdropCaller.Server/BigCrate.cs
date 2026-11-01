using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services.Locales;
using SPTarkov.Server.Core.Utils.Cloners;

namespace AirdropCaller.Server;

/// <summary>
/// 바닐라 보급 상자를 베껴 격자만 키운 상자를 아이템 DB 에 하나 집어넣는다.
///
/// 상자 넷(Common·Supply·Medical·Weapon)은 전부 10×12=120칸이다. 그 안에 가방을
/// 넣어 겹쳐 담을 수는 있지만(`ItemSpec.Items`), "상자를 열면 다 보이는" 모양을
/// 바라면 상자 자체가 커야 한다.
///
/// **바닐라 상자를 키우지 않는다.** 그러면 우리가 안 부른 보급까지 전부 커진다.
/// 대신 베껴서 우리 것만 쓴다.
///
/// 클라이언트가 이 아이템을 알아야 한다. `GET /client/items` 가
/// <c>templateTable.Items</c> 를 **그때그때 그대로 흘려보내므로**(DataCallbacks,
/// 캐시 없음) 서버가 켜질 때 넣어 두면 게임이 받아 간다.
///
/// <para>
/// **반드시 <see cref="OnLoadOrder.Preload"/> 여야 한다.** 4.1.6 의
/// <c>DatabaseIntegrityService</c> 가 프로필을 읽은 뒤에 늘어난 아이템을 찾아내
/// 서버를 통째로 멈춰 세운다. 처음에 <c>PostLoad</c> 에 뒀다가 이것으로 서버를
/// 못 켰다(2026-10-30).
/// </para>
/// <code>
/// DatabaseModifiedAfterCutoffException: 1 item(s) were added to the database
/// after profiles loaded: a1d0caffe0000000000c0de1. Add items during
/// OnLoadOrder.Preload, the database is fully loaded by then …
/// </code>
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.Preload)]
public class BigCrate(
    ISptLogger<BigCrate> logger,
    TemplateTable templateTable,
    ICloner cloner,
    LocaleService localeService,
    ConfigStore store) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        // 설정을 못 읽었거나 꺼져 있으면 DB 를 건드리지 않는다.
        if (store.LoadError is null && store.Config.ModEnabled)
        {
            Register(store.Config);
        }

        return Task.CompletedTask;
    }

    /// <summary>베낄 바닐라 상자. `air_drop_common`.</summary>
    private static readonly MongoId Source = new("6223349b3136504a544d1608");

    /// <summary>
    /// 우리 상자의 tpl. 아무 값이나 쓰면 안 되고 24자리 16진수여야 한다(MongoId).
    /// 바닐라·모드 어느 쪽과도 겹치지 않도록 눈에 띄는 값으로 골랐다.
    /// </summary>
    public static readonly MongoId Tpl = new("a1d0caffe0000000000c0de1");

    /// <summary>집어넣기에 성공했나. 실패하면 큰 상자 요청은 보통 상자로 간다.</summary>
    public bool Ready { get; private set; }

    /// <summary>실제로 들어간 칸 수. 로그와 창에 그대로 보인다.</summary>
    public int Cells { get; private set; }

    private void Register(Config config)
    {
        var cellsH = Math.Clamp(config.BigCrateWidth, 1, 64);
        var cellsV = Math.Clamp(config.BigCrateHeight, 1, 64);

        if (templateTable.Items.ContainsKey(Tpl))
        {
            // 서버가 두 번 켜질 일은 없지만, 다른 모드가 같은 id 를 쓰면 덮지 않는다.
            logger.Warning($"[AirdropCaller] 큰 상자 {Tpl} 가 이미 DB 에 있다. 그대로 쓴다.");
            Ready = true;
            Cells = cellsH * cellsV;
            return;
        }

        if (!templateTable.Items.TryGetValue(Source, out var source))
        {
            logger.Error($"[AirdropCaller] 베낄 상자 {Source} 가 DB 에 없다. 큰 상자는 못 만든다.");
            return;
        }

        // 깊게 베낀다. 얕게 베끼면 아래에서 격자를 고칠 때 **바닐라 상자까지** 커진다.
        var copy = cloner.Clone(source);
        if (copy?.Properties?.Grids?.FirstOrDefault() is not { } grid || grid.Properties is null)
        {
            logger.Error($"[AirdropCaller] 베낀 상자에 격자가 없다. 큰 상자는 못 만든다.");
            return;
        }

        copy.Id = Tpl;
        copy.Name = "airdrop_caller_big_crate";
        grid.Parent = Tpl;
        grid.Properties.CellsH = cellsH;
        grid.Properties.CellsV = cellsV;

        templateTable.Items[Tpl] = copy;
        Ready = true;
        Cells = cellsH * cellsV;

        NameIt();
        logger.Info($"[AirdropCaller] 큰 상자 {Tpl} 를 만들었다 — {cellsH}×{cellsV}={Cells}칸.");
    }

    /// <summary>
    /// 이름을 붙여 본다.
    ///
    /// 로케일은 <c>LazyLoad</c> 뒤에 있어, <c>GetLocaleDb</c> 가 돌려주는 것이 살아
    /// 있는 표인지 베낀 것인지 밖에서는 알 수 없다. 그래서 **넣고 다시 읽어 본다** —
    /// 안 붙었으면 그렇다고 적는다. 이름이 없어도 상자는 열린다(창 제목은 상자
    /// 종류에서 온다). 추측으로 "됐다" 고 적어 두지 않으려는 것뿐이다.
    /// </summary>
    private void NameIt()
    {
        const string name = "보급 상자 (대형)";

        try
        {
            var locale = localeService.GetLocaleDb();
            locale[$"{Tpl} Name"] = name;
            locale[$"{Tpl} ShortName"] = "대형";
            locale[$"{Tpl} Description"] = "격자를 키운 보급 상자.";

            if (!localeService.GetLocaleDb().ContainsKey($"{Tpl} Name"))
            {
                logger.Warning("[AirdropCaller] 큰 상자에 이름이 안 붙었다(로케일 표가 베낀 것이다). "
                             + "상자는 그대로 열린다.");
            }
        }
        catch (Exception e)
        {
            logger.Warning($"[AirdropCaller] 큰 상자 이름을 못 붙였다: {e.Message}");
        }
    }
}
