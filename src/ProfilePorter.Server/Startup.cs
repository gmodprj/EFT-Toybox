using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Utils;

namespace ProfilePorter.Server;

/// <summary>
/// 켜질 때 한 줄 적는다. 하는 일은 **기록뿐**이다 — DB 도 프로필도 건드리지 않는다.
///
/// 그런데도 두는 이유는 AirdropCaller 와 같다: 이것이 없으면 설정이 깨졌는지를 **창에서 처음
/// 눌렀을 때**에야 알게 된다. 프로필에 쓰는 모드라면 더더욱 켜자마자 보여야 한다.
/// </summary>
[Injectable(InjectionType.Singleton, OnLoadOrder.PostLoad)]
public class ProfilePorterStartup(
    ISptLogger<ProfilePorterStartup> logger,
    ConfigStore store,
    ProfileReader profiles,
    PorterFiles files) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (store.LoadError is not null)
        {
            logger.Warning("[ProfilePorter] 설정을 못 읽어 꺼진 채로 돈다. 요청을 전부 거절한다.");
            return Task.CompletedTask;
        }

        if (!store.Config.ModEnabled)
        {
            logger.Info("[ProfilePorter] 설정에서 꺼져 있다. 요청을 전부 거절한다.");
            return Task.CompletedTask;
        }

        var profileCount = profiles.List().Count;
        var fileCount = files.List().Count;

        logger.Info($"[ProfilePorter] 프로필 {profileCount}개, 내보낸 파일 {fileCount}개 — {files.Folder}");

        if (!store.Config.BackupBeforeImport)
        {
            // 꺼 둔 것은 사람의 선택이지만, 조용히 넘어가지는 않는다.
            logger.Warning("[ProfilePorter] 들여오기 전 백업이 꺼져 있다. 되돌릴 길이 없다.");
        }

        return Task.CompletedTask;
    }
}
