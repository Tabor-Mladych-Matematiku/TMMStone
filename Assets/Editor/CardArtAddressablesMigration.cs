#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

[InitializeOnLoad]
public static class CardArtAddressablesMigration
{
    private const string Root = "Assets/CardArt";

    static CardArtAddressablesMigration()
    {
        EditorApplication.delayCall += RunIfNeeded;
    }

    [MenuItem("Tools/TMMStone/Migrate Card Art To Addressables")]
    public static void Run()
    {
        AssetDatabase.StartAssetEditing();
        try
        {
            EnsureFolder("Assets", "CardArt");
            EnsureFolder(Root, "CardFaces");
            EnsureFolder(Root, "CardPlainImages");

            MoveIfPresent("Assets/Resources/CardData/V2", $"{Root}/CardFaces/V2");
            MoveIfPresent("Assets/Resources/CardData/GULAG", $"{Root}/CardFaces/GULAG");
            MoveIfPresent("Assets/Resources/CardData/Tokeny", $"{Root}/CardFaces/Tokeny");
            MoveIfPresent("Assets/Resources/CardData/card-back.png", $"{Root}/CardFaces/card-back.png");
            MoveIfPresent("Assets/Resources/CardPlainImages/V2", $"{Root}/CardPlainImages/V2");
            MoveIfPresent("Assets/Resources/CardPlainImages/GULAG", $"{Root}/CardPlainImages/GULAG");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null)
            throw new InvalidOperationException("Addressable Asset Settings are missing.");

        ConfigureGroup(settings, "Card Faces V2", $"{Root}/CardFaces/V2", "card-face/V2");
        ConfigureGroup(settings, "Card Faces GULAG", $"{Root}/CardFaces/GULAG", "card-face/GULAG");
        ConfigureGroup(settings, "Card Faces Tokeny", $"{Root}/CardFaces/Tokeny", "card-face/Tokeny");
        ConfigureGroup(settings, "Card Plain V2", $"{Root}/CardPlainImages/V2", "card-plain/V2");
        ConfigureGroup(settings, "Card Plain GULAG", $"{Root}/CardPlainImages/GULAG", "card-plain/GULAG");
        ConfigureSingleAsset(settings, "Card Faces V2", $"{Root}/CardFaces/card-back.png", "card-face/card-back");

        settings.BuildAddressablesWithPlayerBuild = AddressableAssetSettings.PlayerBuildOption.BuildWithPlayer;
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        Debug.Log("Card artwork migration to remote Addressables completed.");
    }

    private static void RunIfNeeded()
    {
        if (AssetDatabase.IsValidFolder("Assets/Resources/CardData/V2")
            || AssetDatabase.IsValidFolder("Assets/Resources/CardData/Tokeny")
            || AssetDatabase.IsValidFolder("Assets/Resources/CardPlainImages/V2")
            || (AssetDatabase.IsValidFolder($"{Root}/CardFaces/V2")
                && AddressableAssetSettingsDefaultObject.Settings?.FindGroup("Card Faces V2") == null)
            || (AssetDatabase.IsValidFolder($"{Root}/CardFaces/Tokeny")
                && AddressableAssetSettingsDefaultObject.Settings?.FindGroup("Card Faces Tokeny") == null))
            Run();
    }

    private static void ConfigureGroup(
        AddressableAssetSettings settings,
        string groupName,
        string folder,
        string addressPrefix)
    {
        if (!AssetDatabase.IsValidFolder(folder)) return;

        AddressableAssetGroup group = GetOrCreateRemoteGroup(settings, groupName);
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { folder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string relativePath = path[(folder.Length + 1)..];
            string address = $"{addressPrefix}/{Path.ChangeExtension(relativePath, null)}".Replace('\\', '/');
            settings.CreateOrMoveEntry(guid, group).address = address;
        }
    }

    private static void ConfigureSingleAsset(
        AddressableAssetSettings settings,
        string groupName,
        string path,
        string address)
    {
        string guid = AssetDatabase.AssetPathToGUID(path);
        if (string.IsNullOrEmpty(guid)) return;
        settings.CreateOrMoveEntry(guid, GetOrCreateRemoteGroup(settings, groupName)).address = address;
    }

    private static AddressableAssetGroup GetOrCreateRemoteGroup(AddressableAssetSettings settings, string groupName)
    {
        AddressableAssetGroup group = settings.FindGroup(groupName)
            ?? settings.CreateGroup(
                groupName,
                false,
                false,
                false,
                null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema));

        BundledAssetGroupSchema schema = group.GetSchema<BundledAssetGroupSchema>();
        schema.BuildPath.SetVariableByName(settings, "Remote.BuildPath");
        schema.LoadPath.SetVariableByName(settings, "Remote.LoadPath");
        schema.Compression = BundledAssetGroupSchema.BundleCompressionMode.LZ4;
        schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
        schema.IncludeInBuild = true;
        EditorUtility.SetDirty(group);
        EditorUtility.SetDirty(schema);
        return group;
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
    }

    private static void MoveIfPresent(string source, string destination)
    {
        if (AssetDatabase.LoadMainAssetAtPath(source) == null && !AssetDatabase.IsValidFolder(source)) return;
        if (AssetDatabase.LoadMainAssetAtPath(destination) != null || AssetDatabase.IsValidFolder(destination)) return;

        string error = AssetDatabase.MoveAsset(source, destination);
        if (!string.IsNullOrEmpty(error)) throw new InvalidOperationException(error);
    }
}
#endif
