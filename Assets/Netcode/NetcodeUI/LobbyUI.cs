using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        if(Debug.isDebugBuild) r += UnityEngine.Random.Range(0, 200);
            return Sanitize(r);
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
            n = Sanitize(n);
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
    /// <summary>
    /// Sanitizes input into valid C# classname
    /// ChatGPTied
    /// </summary>
    /// <param name="input"></param>
    /// <returns>Sanitized input</returns>
    /// <exception cref="ArgumentException"></exception>
    public static string Sanitize(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException("Input cannot be null or empty.");
        }
        var sanitized = new StringBuilder();

        // Ensure the first character is a letter or underscore
        if (!char.IsLetter(input[0]) && input[0] != '_')
        {
            sanitized.Append('_');
        }

        foreach (var ch in input)
        {
            // Allow letters, digits, and underscores
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                sanitized.Append(ch);
            }
            else
            {
                sanitized.Append('_');  // Replace invalid characters with underscores
            }
        }

        // Ensure it does not start with a digit (if it's not already handled)
        if (char.IsDigit(sanitized[0]))
        {
            sanitized.Insert(0, '_');
        }

        // Return sanitized string (e.g., "MyClassName123")
        return sanitized.ToString();
    }
}
