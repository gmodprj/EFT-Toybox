using System.IO;
using System.Net.Http;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ProfilePorter.App;

/// <summary>
/// 아이템 아이콘을 구해 온다. **AirdropCaller.App 의 같은 이름 클래스와 같은 길**이고,
/// 기본 캐시 폴더도 같다(`E:\Spt\icon-cache`) — 이미 받아 둔 것을 다시 받지 않으려는 것이다.
///
/// **게임 안에서는 못 꺼낸다.** `items.json` 의 `Prefab` 은 3D 모델이고 아이콘은 게임이 그
/// 모델을 렌더해 만든다. 그래서 `Give Ui` 와 같이 tarkov.dev 에서 받아 디스크에 쌓아 둔다.
///
/// **밖으로 나가는 통신이다** — 아이템 id 가 그쪽으로 간다. 설정에서 끌 수 있고, 끄면 격자에
/// 이름만 그린다.
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
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("ProfilePorter/0.1 (+SPT mod)");
    }

    /// <summary>
    /// 아이콘을 돌려준다. 디스크에 있으면 읽고, 없으면 받아서 쌓아 둔다.
    /// 못 구하면 null — 부르는 쪽이 이름만 그린다.
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
            // 격자를 못 그릴 이유는 없다.
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

    /// <summary>원본이 64×64 다. 격자 칸이 그보다 크므로 **줄이지 않고** 그대로 디코드한다.</summary>
    private static ImageSource Decode(byte[] bytes)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = new MemoryStream(bytes);
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
