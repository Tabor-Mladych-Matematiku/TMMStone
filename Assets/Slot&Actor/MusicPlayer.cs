using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class AssetReferemceAudioClip : AssetReferenceT<AudioClip>
{
    public AssetReferemceAudioClip(string guid) : base(guid)
    {
    }
}
public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AssetReferemceAudioClip[] musicTrackAddressables;
    AudioClip[] musicTracks;
    private AudioSource player;
    int musicIndex = 0;
    int musicsLoaded = 0;
    private void Start()
    {
        player = GetComponent<AudioSource>();
        musicTracks = new AudioClip[musicTrackAddressables.Length];
        for (int i = 0; i < musicTrackAddressables.Length; i++)
        {
            int index = i;
            musicTrackAddressables[i].LoadAssetAsync().Completed += (handle) => 
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    TrackLoaded(index, handle.Result);
                }
            };
        }
        
    }
    private void TrackLoaded(int i,AudioClip a)
    {
        musicTracks[i] = a;
        musicsLoaded++;
        if (musicsLoaded == musicTrackAddressables.Length)
        {
            if (!Debug.isDebugBuild) PlayNext();
        }
    }
    private void PlayNext()
    {
        player.clip = musicTracks[musicIndex];
        Invoke(nameof(PlayNext), player.clip.length);
        musicIndex++;
        player.Play();
    }
}
