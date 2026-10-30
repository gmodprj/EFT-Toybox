using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using EFT;
using Newtonsoft.Json;

namespace DynamicMapsFactionMarkers
{
    /// <summary>
    /// 게임에 올라온 커스텀 역할을 긁어 분류표에 올린다.
    /// MoreBotsAPI 는 소프트 의존이라 타입 이름으로 찾고, 없으면 설정의 extraRoles 만 쓴다.
    /// </summary>
    public static class RoleSource
    {
        /// <summary>분류표를 채우고 올린 역할 수를 돌려준다.</summary>
        public static int FillCatalog(RoleCatalog catalog)
        {
            var registered = 0;

            // locale 은 아직 안 올라왔다. 이름은 레이드에서 처음 쓸 때 RoleCatalog.ApplyLabels 로 짓는다.
            foreach (var pair in ReadMoreBotsRoles())
            {
                catalog.Register(pair.Key, pair.Value, null);
                if (catalog.TryGet(pair.Key, out _))
                {
                    registered++;
                }
            }

            // MoreBotsAPI 를 거치지 않고 들어온 역할(설정의 extraRoles).
            catalog.RegisterExtraRoles();

            return registered;
        }

        /// <summary>자동 배정된 역할을 사람이 읽고 색을 고를 수 있게 적어 둔다.</summary>
        public static void WriteDiscovered(string folder, RoleCatalog catalog)
        {
            try
            {
                var rows = new List<object>();
                foreach (var entry in catalog.Discovered)
                {
                    rows.Add(new
                    {
                        roleId = entry.RoleId,
                        scavRole = entry.ScavRole,
                        category = entry.Category,
                        note = "색·모양을 고치려면 이 줄을 config.jsonc 의 groups 로 옮겨 적는다",
                    });
                }

                File.WriteAllText(
                    Path.Combine(folder, "discovered-roles.json"),
                    JsonConvert.SerializeObject(rows, Formatting.Indented));
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("discovered-roles.json 을 쓰지 못했다: " + e.Message);
            }
        }

        /// <summary>
        /// `ScavRole/<역할>` 를 게임 locale 에서 찾는다. 없거나 키가 그대로 돌아오면
        /// null 을 돌려주어 분류표가 ScavRole 키를 범주 이름으로 쓰게 한다.
        /// </summary>
        public static string LookupLabel(string scavRole)
        {
            if (string.IsNullOrEmpty(scavRole))
            {
                return null;
            }

            try
            {
                var key = "ScavRole/" + scavRole;
                var label = LocalizationExtensions.Localized(key, null);

                if (string.IsNullOrEmpty(label) || string.Equals(label, key, StringComparison.Ordinal))
                {
                    return null;
                }

                return label;
            }
            catch (Exception)
            {
                // locale 이 아직 없으면 키를 그대로 쓴다. 역할마다 경고를 쏟지 않는다.
                return null;
            }
        }

        /// <summary>MoreBotsAPI 가 들고 있는 역할 id → ScavRole 짝. 없으면 빈 목록.</summary>
        private static IEnumerable<KeyValuePair<int, string>> ReadMoreBotsRoles()
        {
            var rows = new List<KeyValuePair<int, string>>();

            var managerType = FindType("MoreBotsAPI.CustomWildSpawnTypeManager");
            if (managerType == null)
            {
                Plugin.Log.LogInfo("MoreBotsAPI 가 없다. config.jsonc 의 extraRoles 만 쓴다.");
                return rows;
            }

            try
            {
                var getDict = managerType.GetMethod("GetCustomWildSpawnTypeDict", BindingFlags.Public | BindingFlags.Static);
                if (getDict == null || !(getDict.Invoke(null, null) is IDictionary dict))
                {
                    Plugin.Log.LogWarning("MoreBotsAPI 의 역할 목록을 읽지 못했다.");
                    return rows;
                }

                foreach (DictionaryEntry pair in dict)
                {
                    var custom = pair.Value;
                    if (custom == null)
                    {
                        continue;
                    }

                    var type = custom.GetType();
                    var value = type.GetProperty("WildSpawnTypeValue")?.GetValue(custom, null);
                    var scavRole = type.GetProperty("ScavRole")?.GetValue(custom, null) as string;

                    if (value is int roleId && !string.IsNullOrEmpty(scavRole))
                    {
                        rows.Add(new KeyValuePair<int, string>(roleId, scavRole));
                    }
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("MoreBotsAPI 를 읽다 멈췄다: " + e.Message);
            }

            return rows;
        }

        private static Type FindType(string fullName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var type = assembly.GetType(fullName, false);
                    if (type != null)
                    {
                        return type;
                    }
                }
                catch (Exception)
                {
                    // 못 읽는 어셈블리는 넘긴다.
                }
            }

            return null;
        }
    }
}
