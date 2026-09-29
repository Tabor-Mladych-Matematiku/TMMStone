using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class TMMStoneLobby : MonoBehaviour
{
    public static TMMStoneLobby Instance { get; private set; }

    public GameObject loadingScreen;

    public event EventHandler<LobbyUpdateEventArgs> OnLobbyUpdated;
    public class LobbyUpdateEventArgs
    {
        public Lobby lobby;
    }

    public event EventHandler<LobbyEventArgs> OnLobbyListChanged;
    public class LobbyEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby hostLobby;
    private Lobby jl;
    private Lobby JoinedLobby
    {
        get => jl; set
        {
            jl = value;
            OnLobbyUpdated?.Invoke(this, new() { lobby = jl });
        }
    }
    private float heartbeattimer;
    private float lobbyupdatetimer;
    private bool lobbyUpdateInProgress;
    private string PlayerName { get => LobbyUI.Instance.PlayerName; }
    private const string KEY_START_GAME = nameof(KEY_START_GAME);
    public async void Authenticate()
    {
        try
        {
            InitializationOptions opts = new();
            opts.SetProfile(PlayerName);
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
                await UnityServices.InitializeAsync(opts);

            Debug.Log("Unity Services Initialized");
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Signed in anonymously!");
            ListLobbies();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    private void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        HandleLobbyHeartbeat();
        UpdateLobbyData();
    }
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeattimer -= Time.deltaTime;
            if (heartbeattimer < 0f)
            {
                heartbeattimer = 20f;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
    private bool IsPlayerInLobby()
    {
        if (JoinedLobby != null && JoinedLobby.Players != null)
        {
            foreach (Player player in JoinedLobby.Players)
            {
                if (player.Id == AuthenticationService.Instance.PlayerId) return true;
            }
        }
        return false;
    }
    private async void UpdateLobbyData()
    {
        if (JoinedLobby != null && !lobbyUpdateInProgress)
        {
            lobbyupdatetimer -= Time.deltaTime;
            if (lobbyupdatetimer < 0f)
            {
                lobbyupdatetimer = 1.1f;
                lobbyUpdateInProgress = true;
                try
                {
                    JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
                    if (!IsPlayerInLobby())
                    {
                        JoinedLobby = null;
                    }
                    else if (JoinedLobby.Data.TryGetValue(KEY_START_GAME, out DataObject startGameData)
                        && startGameData.Value != "0")
                    {
                        loadingScreen.SetActive(true);
                        if (!IsLobbyHost())
                        {
                            bool relayStarted = await TMMStoneRelay.Instance.JoinRelay(startGameData.Value);
                            if (!relayStarted)
                            {
                                loadingScreen.SetActive(false);
                                return;
                            }
                            LobbyUI.Instance.Hide();
                            JoinedLobbyUI.Instance.Hide();
                        }
                        JoinedLobby = null;
                    }
                }
                catch (LobbyServiceException e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    lobbyUpdateInProgress = false;
                }
            }
        }
    }
    public async void CreateLobby(bool Private, string lobbyName)
    {
        if (!LobbyUI.TryValidateLobbyName(lobbyName, out lobbyName, out string validationError))
        {
            Debug.LogWarning(validationError);
            return;
        }

        try
        {
            CreateLobbyOptions opts = new()
            {
                IsPrivate = Private,
                Player = GetPlayer(),
                Data = new()
                {
                    {KEY_START_GAME,new(DataObject.VisibilityOptions.Member,"0") }
                }
            };
            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, 2, opts);
            JoinedLobby = hostLobby;
            Debug.Log("Hosting lobby: " + hostLobby.Name + " Id: " + hostLobby.Id + " Code: " + hostLobby.LobbyCode);
            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions options = new()
            {
                Filters = new()
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots,"0",QueryFilter.OpOptions.GT)
                }
            };

            QueryResponse query = await Lobbies.Instance.QueryLobbiesAsync(options);
            OnLobbyListChanged?.Invoke(this, new LobbyEventArgs { lobbyList = query.Results });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobby(string LobbyId)
    {
        try
        {
            JoinedLobby = await Lobbies.Instance.JoinLobbyByIdAsync(LobbyId, new()
            {
                Player = GetPlayer()
            });
            PrintPlayers(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void JoinLobbybyCode(string code)
    {
        try
        {
            JoinedLobby = await Lobbies.Instance.JoinLobbyByCodeAsync(code
                , new()
                {
                    Player = GetPlayer()
                }
                );
            PrintPlayers(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void QuickJoin(/*string LobbyId*/)
    {
        try
        {
            JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(new()
            {
                Player = GetPlayer()
            });
            Debug.Log("QuickJoined lobby!");
            PrintPlayers(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>()
                    {
                        { "PlayerName",new(PlayerDataObject.VisibilityOptions.Member,PlayerName) }
                    }
        };
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Lobby " + lobby.Name + " PlayerCount: " + lobby.Players.Count);
        foreach (var player in lobby.Players)
        {
            Debug.Log(player.Id);
        }
    }
    public async void UpdateLobby(UpdateLobbyOptions newopts)
    {
        try
        {
            hostLobby = await Lobbies.Instance.UpdateLobbyAsync(hostLobby.Id, newopts);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void LeaveLobby()
    {
        try
        {
            if (hostLobby != null && hostLobby.Players.Count == 1) { await LobbyService.Instance.DeleteLobbyAsync(hostLobby.Id); }
            else await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
            hostLobby = null;
            JoinedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void KickPlayer()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, JoinedLobby.Players[1].Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public bool IsLobbyHost() => JoinedLobby != null && JoinedLobby.HostId == AuthenticationService.Instance.PlayerId;

    public async void StartGame()
    {
        if (IsLobbyHost())
        {
            if (JoinedLobby.Players.Count == JoinedLobby.MaxPlayers)
            {
                try
                {
                    loadingScreen.SetActive(true);
                    string relaycode = await TMMStoneRelay.Instance.CreateRelay();
                    if (string.IsNullOrEmpty(relaycode))
                    {
                        loadingScreen.SetActive(false);
                        Debug.LogError("Could not create or start the Relay host.");
                        return;
                    }
                    Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(JoinedLobby.Id, new()
                    {
                        Data = new() {
                        {KEY_START_GAME,new (DataObject.VisibilityOptions.Member,relaycode) }
                    }
                    });
                    JoinedLobby = lobby;
                    LobbyUI.Instance.Hide();
                    JoinedLobbyUI.Instance.Hide();
                }
                catch (LobbyServiceException e)
                {
                    loadingScreen.SetActive(false);
                    Debug.Log(e);
                }
            }
            else
            {
                Debug.Log("Not Enough players to start a game");//Should not happen the button should be unpressable
            }
        }
    }
}

