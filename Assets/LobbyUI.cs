using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public static LobbyUI Instance { get; private set; }

    [SerializeField] Button CreateLobbyButton;
    [SerializeField] Toggle PrivacyToggle;
    [SerializeField] GameObject lobbyListingPrefab;
    [SerializeField] Transform container;
    [SerializeField] Button refreshButton;
    [SerializeField] TMP_InputField CreateLobbyName;
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        
        CreateLobbyButton.onClick.AddListener(()=> {
            string n = CreateLobbyName.text.Trim();
            if (n == "") n = "NoName";
            TMMStoneLobby.Instance.CreateLobby(PrivacyToggle.isOn,n);
        
        });
        refreshButton.onClick.AddListener( RefreshButtonClick);
        TMMStoneLobby.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }
    void RefreshButtonClick()
    {
        TMMStoneLobby.Instance.ListLobbies();
    }
    void LobbyManager_OnLobbyListChanged(object sender, TMMStoneLobby.LobbyEventArgs args)
    {
        UpdateLobbyList(args.lobbyList);
    }
    // Update is called once per frame
    void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform lobby in container)
        {
            Destroy(lobby.gameObject);
        }


        foreach (Lobby lobby in lobbyList)
        {
            var instance = Instantiate(lobbyListingPrefab,container);
            LobbyListingUI ui = instance.GetComponent<LobbyListingUI>();
            Debug.Log(ui.ToString());
            ui.Initialize(lobby);
        }
    }
}
