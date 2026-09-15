using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SysKill.EditorTools
{
    /// <summary>
    /// manifest.json(sprite-generator-game-assets-v2) 을 표현하는 데이터 모델.
    /// JsonUtility 는 동적 키 딕셔너리("clips")를 파싱하지 못하므로 하단의
    /// 경량 <see cref="MiniJson"/> 파서를 사용한다.
    /// </summary>
    public sealed class SpriteSheetManifest
    {
        public int frameSize = 64;
        public int frameIndexBase = 0;
        public List<string> directionOrder = new() { "up", "left", "down", "right" };
        public List<ClipDef> clips = new();

        public sealed class ClipDef
        {
            public string name;              // 클립 이름 (idle, walk ...)
            public string file;              // 시트 경로 (예: "core/walk.png"), 폴더 기준 상대경로
            public int columns = 1;          // 프레임(가로) 개수
            public int rows = 1;             // 방향(세로) 개수
            public List<int> cycle = new();  // 사용할 컬럼 인덱스 목록(재생 순서)
            public List<string> directions = new(); // 행 → 방향 매핑(위에서 아래로)
            public bool holdLastFrame;       // 마지막 프레임 유지(사망 등)
        }

        /// <summary>폴더 경로에서 manifest.json 을 찾아 파싱한다. 없으면 null.</summary>
        public static SpriteSheetManifest LoadFromFolder(string absoluteFolderPath)
        {
            string path = Path.Combine(absoluteFolderPath, "manifest.json");
            if (!File.Exists(path))
                return null;
            return Parse(File.ReadAllText(path));
        }

        public static SpriteSheetManifest Parse(string json)
        {
            if (!(MiniJson.Deserialize(json) is Dictionary<string, object> root))
                throw new FormatException("manifest.json 루트가 객체가 아닙니다.");

            var manifest = new SpriteSheetManifest
            {
                frameSize = GetInt(root, "frameSize", 64),
                frameIndexBase = GetInt(root, "frameIndexBase", 0),
            };

            if (root.TryGetValue("directionOrder", out var dirOrder) && dirOrder is List<object> doList)
            {
                manifest.directionOrder = new List<string>();
                foreach (var o in doList) manifest.directionOrder.Add(Convert.ToString(o));
            }

            if (!(root.TryGetValue("clips", out var clipsObj) && clipsObj is Dictionary<string, object> clipsMap))
                throw new FormatException("manifest.json 에 clips 객체가 없습니다.");

            foreach (var kvp in clipsMap)
            {
                if (!(kvp.Value is Dictionary<string, object> c))
                    continue;

                var clip = new ClipDef
                {
                    name = kvp.Key,
                    file = GetString(c, "file", null),
                    columns = GetInt(c, "columns", 1),
                    rows = GetInt(c, "rows", 1),
                    holdLastFrame = GetBool(c, "holdLastFrame", false),
                    cycle = GetIntList(c, "cycle"),
                    directions = GetStringList(c, "directions"),
                };

                // cycle 이 비어있으면 0..columns-1 로 채움
                if (clip.cycle.Count == 0)
                    for (int i = 0; i < clip.columns; i++) clip.cycle.Add(i);
                // frameIndexBase 보정 → 0 기반 컬럼 인덱스로 정규화
                if (manifest.frameIndexBase != 0)
                    for (int i = 0; i < clip.cycle.Count; i++) clip.cycle[i] -= manifest.frameIndexBase;
                // directions 가 비어있으면 directionOrder 에서 rows 개수만큼 사용
                if (clip.directions.Count == 0)
                    for (int r = 0; r < clip.rows && r < manifest.directionOrder.Count; r++)
                        clip.directions.Add(manifest.directionOrder[r]);

                manifest.clips.Add(clip);
            }

            return manifest;
        }

        // ---- 파싱 헬퍼 ----
        static int GetInt(Dictionary<string, object> d, string k, int def) =>
            d.TryGetValue(k, out var v) && v != null ? (int)Math.Round(Convert.ToDouble(v, CultureInfo.InvariantCulture)) : def;

        static bool GetBool(Dictionary<string, object> d, string k, bool def) =>
            d.TryGetValue(k, out var v) && v is bool b ? b : def;

        static string GetString(Dictionary<string, object> d, string k, string def) =>
            d.TryGetValue(k, out var v) && v != null ? Convert.ToString(v) : def;

        static List<int> GetIntList(Dictionary<string, object> d, string k)
        {
            var list = new List<int>();
            if (d.TryGetValue(k, out var v) && v is List<object> arr)
                foreach (var o in arr)
                    list.Add((int)Math.Round(Convert.ToDouble(o, CultureInfo.InvariantCulture)));
            return list;
        }

        static List<string> GetStringList(Dictionary<string, object> d, string k)
        {
            var list = new List<string>();
            if (d.TryGetValue(k, out var v) && v is List<object> arr)
                foreach (var o in arr)
                    list.Add(Convert.ToString(o));
            return list;
        }
    }

    /// <summary>
    /// 의존성 없는 최소 JSON 파서. object/array/string/number(double)/bool/null 을
    /// Dictionary&lt;string,object&gt; / List&lt;object&gt; / string / double / bool / null 로 반환한다.
    /// </summary>
    internal static class MiniJson
    {
        public static object Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            int idx = 0;
            var result = ParseValue(json, ref idx);
            return result;
        }

        static object ParseValue(string s, ref int i)
        {
            SkipWhitespace(s, ref i);
            char c = s[i];
            switch (c)
            {
                case '{': return ParseObject(s, ref i);
                case '[': return ParseArray(s, ref i);
                case '"': return ParseString(s, ref i);
                case 't':
                case 'f': return ParseBool(s, ref i);
                case 'n': i += 4; return null; // null
                default: return ParseNumber(s, ref i);
            }
        }

        static Dictionary<string, object> ParseObject(string s, ref int i)
        {
            var dict = new Dictionary<string, object>();
            i++; // {
            SkipWhitespace(s, ref i);
            if (s[i] == '}') { i++; return dict; }
            while (true)
            {
                SkipWhitespace(s, ref i);
                string key = ParseString(s, ref i);
                SkipWhitespace(s, ref i);
                i++; // ':'
                var val = ParseValue(s, ref i);
                dict[key] = val;
                SkipWhitespace(s, ref i);
                char c = s[i++];
                if (c == '}') break;
                // c == ','
            }
            return dict;
        }

        static List<object> ParseArray(string s, ref int i)
        {
            var list = new List<object>();
            i++; // [
            SkipWhitespace(s, ref i);
            if (s[i] == ']') { i++; return list; }
            while (true)
            {
                var val = ParseValue(s, ref i);
                list.Add(val);
                SkipWhitespace(s, ref i);
                char c = s[i++];
                if (c == ']') break;
                // c == ','
            }
            return list;
        }

        static string ParseString(string s, ref int i)
        {
            var sb = new StringBuilder();
            i++; // opening quote
            while (true)
            {
                char c = s[i++];
                if (c == '"') break;
                if (c == '\\')
                {
                    char e = s[i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16));
                            i += 4;
                            break;
                    }
                }
                else sb.Append(c);
            }
            return sb.ToString();
        }

        static object ParseBool(string s, ref int i)
        {
            if (s[i] == 't') { i += 4; return true; }
            i += 5; return false;
        }

        static object ParseNumber(string s, ref int i)
        {
            int start = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            return double.Parse(s.Substring(start, i - start), CultureInfo.InvariantCulture);
        }

        static void SkipWhitespace(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }
    }
}
