using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] AudioClip[] musicTracks;
    private AudioSource player;
    int musicIndex = 0;
    private void Start()
    {
        player = GetComponent<AudioSource>();
        if (!Debug.isDebugBuild)
            PlayNext();
    }
    private void PlayNext()
    {
        player.clip = musicTracks[musicIndex];
        Invoke(nameof(PlayNext), player.clip.length);
        musicIndex++;
        player.Play();
    }
}
