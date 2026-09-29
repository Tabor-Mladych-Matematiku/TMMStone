using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace CardGame
{
    public static class CardArt
    {
        private static readonly Dictionary<string, AsyncOperationHandle<Sprite>> Handles = new();

        public static string FaceAddress(string expansion, string cardName) => $"card-face/{expansion}/{ArtName(cardName)}";
        public static string PlainAddress(string expansion, string cardName) => $"card-plain/{expansion}/{ArtName(cardName)}";

        private static string ArtName(string cardName)
        {
            const string tokenSuffix = " (token)";
            return cardName.EndsWith(tokenSuffix, StringComparison.Ordinal)
                ? cardName[..^tokenSuffix.Length]
                : cardName;
        }

        public static void Load(string address, Action<Sprite> completed)
        {
            if (Handles.TryGetValue(address, out AsyncOperationHandle<Sprite> existing))
            {
                if (existing.IsDone)
                {
                    completed(existing.Status == AsyncOperationStatus.Succeeded ? existing.Result : null);
                }
                else
                {
                    existing.Completed += operation => completed(
                        operation.Status == AsyncOperationStatus.Succeeded ? operation.Result : null);
                }
                return;
            }

            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(address);
            Handles.Add(address, handle);
            handle.Completed += operation =>
            {
                if (operation.Status != AsyncOperationStatus.Succeeded)
                    Debug.LogError($"Could not load Addressable card art: {address}");
                completed(operation.Status == AsyncOperationStatus.Succeeded ? operation.Result : null);
            };
        }

        public static void ReleaseAll()
        {
            foreach (AsyncOperationHandle<Sprite> handle in Handles.Values)
                if (handle.IsValid()) Addressables.Release(handle);
            Handles.Clear();
        }
    }
}
