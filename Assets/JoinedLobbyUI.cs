using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;
using Unity.Services.Authentication;
public class JoinedLobbyUI : MonoBehaviour
{
    public static JoinedLobbyUI Instance { get; private set; }

    [SerializeField] Button LeaveLobbyButton;
    [SerializeField] Button StartGameButton;
    [SerializeField] Toggle PrivacyToggle;
    [SerializeField] List<PlayerListingUI> PlayerListings = new(2);
    [SerializeField] TextMeshProUGUI LobbyName;

    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        LeaveLobbyButton.onClick.AddListener(OnLeaveClick);
        StartGameButton.onClick.AddListener(OnStartClick);

        TMMStoneLobby.Instance.OnLobbyUpdated += UpdateLobbyData;
        Hide();
    }
    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
    private void UpdateLobbyData(object sender, TMMStoneLobby.LobbyUpdateEventArgs e)
    {
        PrivacyToggle.onValueChanged.RemoveAllListeners();
        foreach (PlayerListingUI slot in PlayerListings)
        {
            slot.Clear();
        }
        if (e.lobby != null)
        {
            PrivacyToggle.isOn = e.lobby.IsPrivate;
            PrivacyToggle.onValueChanged.AddListener(OnTogglePrivacyClick);

            if (e.lobby.HostId != AuthenticationService.Instance.PlayerId)
            {
                PrivacyToggle.enabled = false;
                StartGameButton.gameObject.SetActive(false); 
            }
            else {
                PrivacyToggle.enabled = true;
                StartGameButton.gameObject.SetActive(true);
            }

            LobbyName.text = e.lobby.Name;
            for (short i = 0; i < e.lobby.Players.Count; i++)
            {
                PlayerListings[i].Initialize(e.lobby.Players[i]);
            }
        }
        else
        {
            Hide();
        }
    }
    private void OnLeaveClick()
    {
        TMMStoneLobby.Instance.LeaveLobby();
        Hide();
    }

    private void OnStartClick()
    {
        TMMStoneLobby.Instance.StartGame();
        Hide();
    }
    public void JoinButtonClicked(string id)
    {
        TMMStoneLobby.Instance.JoinLobby(id);
        Show();
    }
    private void OnTogglePrivacyClick(bool arg0)
    {
        TMMStoneLobby.Instance.UpdateLobby(new() { IsPrivate = arg0 });
    }
}
