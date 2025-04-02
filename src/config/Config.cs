using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
namespace SCE
{
    internal class Config
    {
        public Dictionary<string, object?> Values { get; set; } = new();

        #region Get

        public T Get<T>(string name)
        {
            return (T)(Values[name] ?? throw new NullReferenceException());
        }

        public T GetOrDefault<T>(string name, T def)
        {
            if (!TryGetAs<T>(name, out var val))
                return def;
            return val;
        }

        public bool TryGetAs<T>(string name, [MaybeNullWhen(false)] out T value)
        {
            if (!Values.TryGetValue(name, out var obj) || obj == null)
            {
                value = default;
                return false;
            }
            value = (T)obj;
            return true;
        }

        #endregion

        public string Create()
        {
            return JsonSerializer.Serialize(Values, new JsonSerializerOptions() 
                { WriteIndented = true });
        }

        public bool CreateAsFile(string path)
        {
            if (File.Exists(path))
                return false;
            File.WriteAllText(path, Create());
            return true;
        }

        public void RebuildFromFile(string path)
        {
            Rebuild(File.ReadAllText(path));
        }

        public void Rebuild(string json)
        {
            var cfg = JsonSerializer.Deserialize<Dictionary<string, object?>>(json);
            if (cfg == null)
                return;
            foreach (var pair in cfg)
            {
                if (Values.ContainsKey(pair.Key))
                    Values[pair.Key] = pair.Value;
            }
        }
    }
}
