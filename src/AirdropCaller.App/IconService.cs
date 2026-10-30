using System.IO;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AirdropCaller.App;

/// <summary>
/// 아이템 아이콘을 구해 온다.
///
/// **게임 안에서는 못 꺼낸다.** `items.json` 의 `Prefab` 은 3D 모델이고 아이콘은 게임이
/// 그 모델을 렌더해 만든다. 디스크의 `user/sptappdata/live` 에 9602개가 있긴 한데
/// 번호와 tpl 을 잇는 표가 없다(`index.json` 의 키가 무엇의 해시인지 못 찾았다).
/// 자세한 것은 doc/specs/2026-10-28-airdrop-caller-design.md 의 10-A 절.
///
/// 그래서 `Give Ui` 와 같은 길을 쓴다 — tarkov.dev 에서 받아 **디스크에 쌓아 둔다.**
/// 한 번 받은 것은 다시 받지 않는다. `icon.jpg` 를 쓰는 이유는 64×64 고정에 0.5~3KB 로
/// 가장 작고, 배경이 거의 검정이라 어두운 목록에 그대로 얹히기 때문이다.
///
/// 밖으로 나가는 통신이므로 설정에서 끌 수 있다. 끄면 분류 아이콘만 쓴다.
/// </summary>
public sealed class IconService : IDisposable
{
    private const string UrlFormat = "https://assets.tarkov.dev/{0}-icon.jpg";

    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(15) };
    private readonly Dictionary<string, ImageSource?> _memory = [];
    private readonly SemaphoreSlim _gate = new(4);
    private readonly string _cacheDir;
    private readonly bool _enabled;

    public IconService(string cacheDir, bool enabled)
    {
        _cacheDir = cacheDir;
        _enabled = enabled;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("AirdropCaller/0.1 (+SPT mod)");
    }

    /// <summary>
    /// 아이콘을 돌려준다. 디스크에 있으면 읽고, 없으면 받아서 쌓아 둔다.
    /// 못 구하면 null — 부르는 쪽이 분류 아이콘으로 넘어간다.
    /// </summary>
    public async Task<ImageSource?> GetAsync(string tpl, CancellationToken ct)
    {
        if (!_enabled || string.IsNullOrWhiteSpace(tpl))
        {
            return null;
        }

        lock (_memory)
        {
            if (_memory.TryGetValue(tpl, out var cached))
            {
                return cached;
            }
        }

        var path = Path.Combine(_cacheDir, tpl + ".jpg");
        var image = Load(path);

        if (image is null)
        {
            // 한꺼번에 수백 개를 받으면 서버에도 우리에게도 무리다. 넷씩만 간다.
            await _gate.WaitAsync(ct);
            try
            {
                image = Load(path) ?? await DownloadAsync(tpl, path, ct);
            }
            finally
            {
                _gate.Release();
            }
        }

        lock (_memory)
        {
            _memory[tpl] = image;
        }

        return image;
    }

    private async Task<ImageSource?> DownloadAsync(string tpl, string path, CancellationToken ct)
    {
        try
        {
            var bytes = await _http.GetByteArrayAsync(string.Format(UrlFormat, tpl), ct);
            if (bytes.Length == 0)
            {
                return null;
            }

            Directory.CreateDirectory(_cacheDir);
            await File.WriteAllBytesAsync(path, bytes, ct);
            return Decode(bytes);
        }
        catch
        {
            // 없는 아이템도 있고 인터넷이 없을 수도 있다. 그림 하나 못 받았다고
            // 목록을 못 보여 줄 이유는 없다.
            return null;
        }
    }

    private static ImageSource? Load(string path)
    {
        try
        {
            return File.Exists(path) ? Decode(File.ReadAllBytes(path)) : null;
        }
        catch
        {
            return null;
        }
    }

    private static ImageSource Decode(byte[] bytes)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = new MemoryStream(bytes);
        image.DecodePixelHeight = 24;
        image.EndInit();
        image.Freeze();
        return image;
    }

    public void Dispose()
    {
        _http.Dispose();
        _gate.Dispose();
    }
}
