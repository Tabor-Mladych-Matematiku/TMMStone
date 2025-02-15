using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerListingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI AdditionalData;
    [SerializeField] Button kickButton;
    public void Initialize(Player player)
    {
        //playerCount.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers;
        Name.text = player.Data["Name"].Value;
        kickButton.onClick.AddListener(() => TMMStoneLobby.Instance.KickPlayer());
        if (TMMStoneLobby.Instance.IsLobbyHost()) kickButton.enabled = false;
    }
}
