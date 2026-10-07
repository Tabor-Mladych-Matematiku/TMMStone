using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

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

        /// <summary>
        /// Loads and retains the requested sprites in the same cache used by <see cref="Load"/>.
        /// Later consumers therefore receive an already completed handle instead of briefly
        /// displaying their prefab's placeholder artwork.
        /// </summary>
        public static IEnumerator Preload(IEnumerable<string> addresses)
        {
            HashSet<string> uniqueAddresses = new(addresses);
            int pending = 0;

            foreach (string address in uniqueAddresses)
            {
                pending++;
                AsyncOperationHandle<IList<IResourceLocation>> locations =
                    Addressables.LoadResourceLocationsAsync(address, typeof(Sprite));
                locations.Completed += operation =>
                {
                    bool exists = operation.Status == AsyncOperationStatus.Succeeded
                        && operation.Result.Count > 0;
                    Addressables.Release(operation);

                    if (exists) Load(address, _ => pending--);
                    else pending--;
                };
            }

            yield return new WaitUntil(() => pending == 0);
        }

        public static void ReleaseAll()
        {
            foreach (AsyncOperationHandle<Sprite> handle in Handles.Values)
                if (handle.IsValid()) Addressables.Release(handle);
            Handles.Clear();
        }
    }
}
