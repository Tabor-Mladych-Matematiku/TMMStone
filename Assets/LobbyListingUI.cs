using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LobbyListingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI playerCount;
    [SerializeField] Button joinButton;
    public void Initialize(Lobby lobby)
    {
        playerCount.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers;
        Name.text = lobby.Name;
        joinButton.onClick.AddListener(() => JoinedLobbyUI.Instance.JoinButtonClicked(lobby.Id));
    }
}
