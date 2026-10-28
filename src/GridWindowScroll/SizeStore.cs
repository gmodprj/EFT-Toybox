using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GridWindowScroll
{
    /// <summary>
    /// 상자별로 정한 보이는 칸 수를 DLL 옆 <c>sizes.json</c> 에 둔다.
    /// 열쇠는 아이템 템플릿 id 라, 같은 종류의 상자는 모두 같은 크기로 열린다.
    /// </summary>
    internal class SizeStore
    {
        private readonly string _path;
        private Dictionary<string, int[]> _map = new Dictionary<string, int[]>();

        internal SizeStore(string path)
        {
            _path = path;
            Load();
        }

        internal int Count => _map.Count;

        internal bool TryGet(string templateId, out int columns, out int rows)
        {
            columns = rows = 0;
            if (string.IsNullOrEmpty(templateId)) return false;
            if (!_map.TryGetValue(templateId, out var v) || v == null || v.Length != 2) return false;

            columns = v[0];
            rows = v[1];
            return columns > 0 && rows > 0;
        }

        internal void Set(string templateId, int columns, int rows)
        {
            if (string.IsNullOrEmpty(templateId)) return;
            _map[templateId] = new[] { columns, rows };
        }

        internal void Clear()
        {
            _map.Clear();
            Save();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_path)) return;
                _map = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(File.ReadAllText(_path))
                       ?? new Dictionary<string, int[]>();
                GridWindowScrollPlugin.Log.LogInfo($"상자별 크기 {_map.Count}개 읽음 ({_path})");
            }
            catch (Exception e)
            {
                // 파일이 깨졌으면 기본값으로 가고, 다음 저장 때 다시 쓴다.
                _map = new Dictionary<string, int[]>();
                GridWindowScrollPlugin.Log.LogWarning("상자별 크기 파일을 읽지 못해 기본값으로 간다: " + e.Message);
            }
        }

        internal void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
                File.WriteAllText(_path, JsonConvert.SerializeObject(_map, Formatting.Indented));
            }
            catch (Exception e)
            {
                GridWindowScrollPlugin.Log.LogError("상자별 크기를 저장하지 못했다: " + e.Message);
            }
        }
    }
}
