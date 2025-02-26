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
    [SerializeField] TMP_InputField PlayerNameField;
    public string PlayerName
    {
        get {
            string r = PlayerNameField.text.Trim();
#if UNITY_ANDROID
        if (r == "") r = "Androidymous";
#else
        if (r == "") r = "An only mouse";
#endif
            return r;
        }
    }
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {

        CreateLobbyButton.onClick.AddListener(() =>
        {
            string n = CreateLobbyName.text.Trim();
            if (n == "") n = "NoName";
            TMMStoneLobby.Instance.CreateLobby(PrivacyToggle.isOn, n);
            JoinedLobbyUI.Instance.Show();

        });
        refreshButton.onClick.AddListener(RefreshButtonClick);
        TMMStoneLobby.Instance.OnLobbyListChanged += LobbyManager_OnLobbyListChanged;
    }
    public void Hide() => gameObject.SetActive(false);
    public void Show() => gameObject.SetActive(true);
    void RefreshButtonClick()
    {
        TMMStoneLobby.Instance.ListLobbies();
    }
    void LobbyManager_OnLobbyListChanged(object sender, TMMStoneLobby.LobbyEventArgs args)
    {
        UpdateLobbyList(args.lobbyList);
    }
    void UpdateLobbyList(List<Lobby> lobbyList)
    {
        foreach (Transform lobby in container)
        {
            Destroy(lobby.gameObject);
        }


        foreach (Lobby lobby in lobbyList)
        {
            var instance = Instantiate(lobbyListingPrefab, container);
            LobbyListingUI ui = instance.GetComponent<LobbyListingUI>();
            Debug.Log(ui.ToString());
            ui.Initialize(lobby);
        }
    }
}
