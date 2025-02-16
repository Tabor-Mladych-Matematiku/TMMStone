using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Authentication;
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
        //AdditionalData.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers;
        Name.text = player.Data["PlayerName"].Value;
        kickButton.onClick.AddListener(() => TMMStoneLobby.Instance.KickPlayer());
        if (!TMMStoneLobby.Instance.IsLobbyHost() ||player.Id==AuthenticationService.Instance.PlayerId) kickButton.gameObject.SetActive(false);
        else kickButton.gameObject.SetActive(true); ;
    }
    public void Clear()
    {
        kickButton.onClick.RemoveAllListeners();
        kickButton.gameObject.SetActive(false);
        Name.text = "EMPTY";
        AdditionalData.text = "-";
    }
}
