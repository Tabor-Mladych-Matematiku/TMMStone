using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class DeckStorage
{
    private const string WebGlIndexKey = "TMMStone.Decks";
    private const string WebGlDeckPrefix = "TMMStone.Deck.";

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
#if UNITY_WEBGL && !UNITY_EDITOR
        return LoadWebGlNames();
#else
        string folder = GetDeckFolder();
        Directory.CreateDirectory(folder);
        List<string> names = new();
        foreach (string file in Directory.GetFiles(folder, "*.json"))
            names.Add(Path.GetFileNameWithoutExtension(file));
        names.Sort(System.StringComparer.CurrentCultureIgnoreCase);
        return names;
#endif
    }

    public static string Read(string name)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.GetString(WebGlDeckPrefix + name);
#else
        return File.ReadAllText(Path.Combine(GetDeckFolder(), name + ".json"));
#endif
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
