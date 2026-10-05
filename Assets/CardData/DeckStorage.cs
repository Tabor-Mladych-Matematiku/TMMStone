using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DeckStorage
{
    private const string WebGlIndexKey = "TMMStone.Decks";
    private const string WebGlDeckPrefix = "TMMStone.Deck.";
    private static readonly Dictionary<string, string> DefaultDeckResources = new(System.StringComparer.OrdinalIgnoreCase)
    {
        { "Combo Matematik", "CardData/DefaultDecks/combo-matematik" },
        { "Funfact Informatik", "CardData/DefaultDecks/funfact-informatik" },
        { "Mech Inženýr", "CardData/DefaultDecks/mech-inzenyr" },
        { "Zvíře Fyzik", "CardData/DefaultDecks/zvire-fyzik" }
    };

    public static string Save(string requestedName, string json)
    {
        string name = GetUniqueName(requestedName);
#if UNITY_WEBGL && !UNITY_EDITOR
        List<string> names = LoadWebGlNames();
        names.Add(name);
        PlayerPrefs.SetString(WebGlDeckPrefix + name, json);
        PlayerPrefs.SetString(WebGlIndexKey, MiniJson.JsonEncode(names));
        PlayerPrefs.Save();
#else
        string folder = GetDeckFolder();
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, name + ".json"), json);
#endif
        return name;
    }

    public static List<string> GetDeckNames()
    {
        HashSet<string> names = new(DefaultDeckResources.Keys, System.StringComparer.OrdinalIgnoreCase);
#if UNITY_WEBGL && !UNITY_EDITOR
        foreach (string name in LoadWebGlNames())
            names.Add(name);
#else
        string folder = GetDeckFolder();
        Directory.CreateDirectory(folder);
        foreach (string file in Directory.GetFiles(folder, "*.json"))
            names.Add(Path.GetFileNameWithoutExtension(file));
#endif
        List<string> sortedNames = new(names);
        sortedNames.Sort(System.StringComparer.CurrentCultureIgnoreCase);
        return sortedNames;
    }

    public static string Read(string name)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        string key = WebGlDeckPrefix + name;
        if (PlayerPrefs.HasKey(key))
            return PlayerPrefs.GetString(key);
#else
        string path = Path.Combine(GetDeckFolder(), name + ".json");
        if (File.Exists(path))
            return File.ReadAllText(path);
#endif
        if (DefaultDeckResources.TryGetValue(name, out string resourcePath))
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset != null) return asset.text;
            Debug.LogError($"Missing default deck resource at Resources/{resourcePath}.json");
        }

        throw new FileNotFoundException($"Deck '{name}' does not exist.");
    }

    private static string GetUniqueName(string requestedName)
    {
        HashSet<string> existing = new(GetDeckNames(), System.StringComparer.OrdinalIgnoreCase);
        if (!existing.Contains(requestedName)) return requestedName;

        int suffix = 1;
        while (existing.Contains($"{requestedName} ({suffix})")) suffix++;
        return $"{requestedName} ({suffix})";
    }

    private static string GetDeckFolder() => Path.Combine(Application.persistentDataPath, "Decks");

#if UNITY_WEBGL && !UNITY_EDITOR
    private static List<string> LoadWebGlNames()
    {
        string json = PlayerPrefs.GetString(WebGlIndexKey, "[]");
        List<string> names = new();
        foreach (object value in (List<object>)MiniJson.JsonDecode(json))
            names.Add((string)value);
        return names;
    }
#endif
}
